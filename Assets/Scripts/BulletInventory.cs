using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInventory : MonoBehaviour
{
    public static BulletInventory Instance;

    [Header("The Fixed 6-Slot Chamber")]
    // This array holds the exact 6 bullets currently loaded in your gun
    public BulletData[] chamberSlots = new BulletData[6]; 
    
    [Header("Reload State")]
    public bool isReloading = false;
    public float reloadDuration = 2f;
    private int currentShotIndex = 0;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        InitializeDefaultChamber();
    }

    void InitializeDefaultChamber()
    {
        // Populate with starter bullets if empty in Inspector
        for (int i = 0; i < chamberSlots.Length; i++)
        {
            if (chamberSlots[i].bulletPrefab == null)
            {
                chamberSlots[i] = new BulletData { bulletName = "Standard", type = BulletType.Standard };
            }
        }
    }

    // Fetches the next bullet from the chamber sequence
    public bool GetNextBullet(out BulletData firedBullet)
    {
        if (isReloading || currentShotIndex >= chamberSlots.Length)
        {
            firedBullet = default;
            if (!isReloading) StartCoroutine(ShuffleAndReloadRoutine());
            return false;
        }

        firedBullet = chamberSlots[currentShotIndex];
        currentShotIndex++;
        
        // If the 6th shot is fired, automatically trigger the reload/shuffle
        if (currentShotIndex >= chamberSlots.Length)
        {
            StartCoroutine(ShuffleAndReloadRoutine());
        }
        return true;
    }

    // 2-second reload that shuffles the EXISTING 6 bullets into a new random order
    IEnumerator ShuffleAndReloadRoutine()
    {
        isReloading = true;
        Debug.Log("Reloading: Spinning the chamber...");
        yield return new WaitForSeconds(reloadDuration);

        // Fisher-Yates shuffle algorithm to randomize order of the current 6 bullets
        for (int i = chamberSlots.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            BulletData temp = chamberSlots[i];
            chamberSlots[i] = chamberSlots[randomIndex];
            chamberSlots[randomIndex] = temp;
        }

        currentShotIndex = 0;
        isReloading = false;
        Debug.Log("Chamber spun! New bullet order locked in.");
    }

    // Method used at shops or map rewards to swap a bullet in your chamber
    public void SwapBulletInSlot(int slotIndex, BulletData newBullet)
    {
        if (slotIndex >= 0 && slotIndex < chamberSlots.Length)
        {
            chamberSlots[slotIndex] = newBullet;
            Debug.Log($"Replaced slot {slotIndex} with {newBullet.bulletName}. Discarded old bullet.");
        }
    }
    
    public int CurrentShotIndex
    {
        get { return currentShotIndex; }
    }
    public void ResetChamberToDefault()
    {
        isReloading = false;
        currentShotIndex = 0;
        
        // Reinitialize the chamber with 6 standard bullets (or your starting loadout)
        chamberSlots = new BulletData[6];
        for (int i = 0; i < 6; i++)
        {
            chamberSlots[i] = new BulletData
            {
                bulletName = "Standard",
                type = BulletType.Standard,
                bulletPrefab = null
            };
        }
    }
}