using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BulletRewardManager : MonoBehaviour
{
    [Header("Phase 1: Choice Panel")]
    public GameObject rewardPanel; 
    public Button[] choiceButtons; 
    public TextMeshProUGUI[] choiceButtonTexts; 
    public Button skipButton;      

    [Header("Phase 2: Slot Replacement Panel")]
    public GameObject replacementPanel; 
    public Button[] chamberSlotButtons; 
    public TextMeshProUGUI[] chamberSlotTexts; 
    public Button backButton;           

    [Header("Special Bullet Prefabs")]
    public GameObject splitterPrefab;
    public GameObject shockwavePrefab;
    public GameObject explosivePrefab;

    private BulletData selectedNewBullet;

    void Start()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (replacementPanel != null) replacementPanel.SetActive(false);
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipReward);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(ReturnToChoices);
        }
    }

    public void OpenRewardScreen()
    {
        if (rewardPanel != null) rewardPanel.SetActive(true);
        if (replacementPanel != null) replacementPanel.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            BulletData randomBullet = GetRandomBulletData();
            
            int index = i;
            choiceButtonTexts[i].text = $"New: {randomBullet.bulletName} ({randomBullet.type})";
            
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnBulletChosen(randomBullet));
        }
    }

    BulletData GetRandomBulletData()
    {
        // Exclude Standard bullets from rewards
        BulletType[] specialTypes = { BulletType.Splitter, BulletType.Shockwave, BulletType.Explosive };
        BulletType chosenType = specialTypes[Random.Range(0, specialTypes.Length)];

        GameObject assignedPrefab = null;
        switch (chosenType)
        {
            case BulletType.Splitter:
                assignedPrefab = splitterPrefab;
                break;
            case BulletType.Shockwave:
                assignedPrefab = shockwavePrefab;
                break;
            case BulletType.Explosive:
                assignedPrefab = explosivePrefab;
                break;
        }

        return new BulletData
        {
            bulletName = chosenType.ToString(),
            type = chosenType,
            bulletPrefab = assignedPrefab
        };
    }

    void OnBulletChosen(BulletData newBullet)
    {
        selectedNewBullet = newBullet;

        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (replacementPanel != null) replacementPanel.SetActive(true);

        UpdateChamberSlotUI();
    }

    void UpdateChamberSlotUI()
    {
        if (BulletInventory.Instance == null) return;

        for (int i = 0; i < chamberSlotButtons.Length; i++)
        {
            if (i < BulletInventory.Instance.chamberSlots.Length)
            {
                BulletData currentBullet = BulletInventory.Instance.chamberSlots[i];
                chamberSlotTexts[i].text = $"Slot {i + 1}: {currentBullet.bulletName}";

                int slotIndex = i;
                chamberSlotButtons[i].onClick.RemoveAllListeners();
                chamberSlotButtons[i].onClick.AddListener(() => ConfirmReplacement(slotIndex));
            }
        }
    }

    void ConfirmReplacement(int slotIndex)
    {
        if (BulletInventory.Instance != null && slotIndex < BulletInventory.Instance.chamberSlots.Length)
        {
            BulletInventory.Instance.chamberSlots[slotIndex] = selectedNewBullet;
            Debug.Log($"Replaced slot {slotIndex + 1} with {selectedNewBullet.bulletName}");
        }

        CloseAndOpenMap();
    }

    void ReturnToChoices()
    {
        if (replacementPanel != null) replacementPanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(true);
    }

    void OnSkipReward()
    {
        CloseAndOpenMap();
    }

    void CloseAndOpenMap()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (replacementPanel != null) replacementPanel.SetActive(false);
        
        SceneManager.LoadScene("KabbalahMapScene"); 
    }
}