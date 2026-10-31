using UnityEngine;
using TMPro;

public class ChamberHUD : MonoBehaviour
{
    public TextMeshProUGUI chamberDisplayText;

    void Update()
    {
        if (BulletInventory.Instance == null || chamberDisplayText == null) return;

        // If reloading, show reload message
        if (BulletInventory.Instance.isReloading)
        {
            chamberDisplayText.text = "CHAMBER: [ RELOADING... ]";
            return;
        }

        string displayStr = "CHAMBER: \n[ ";
        int currentIndex = BulletInventory.Instance.CurrentShotIndex;

        // Loop through the 6 chamber slots
        for (int i = 0; i < BulletInventory.Instance.chamberSlots.Length; i++)
        {
            // Determine abbreviation based on bullet type
            string code = "STA";
            switch (BulletInventory.Instance.chamberSlots[i].type)
            {
                case BulletType.Standard: code = "STA"; break;
                case BulletType.Splitter: code = "SPL"; break;
                case BulletType.Shockwave: code = "SHO"; break;
                case BulletType.Explosive: code = "EXP"; break;
            }

            // If this bullet has already been fired, cross it out or dim it visually
            if (i < currentIndex)
            {
                displayStr += $"<color=#777777>[{code}]</color> "; // Grayed out for spent bullets
            }
            // If this is the exact bullet about to be fired next, highlight it
            else if (i == currentIndex)
            {
                displayStr += $"<color=#00FF00><b>>{code}<</b></color> "; // Green / highlighted for next shot
            }
            // Future un-fired bullets in the chamber
            else
            {
                displayStr += $"{code} ";
            }
        }
        
        displayStr += "]";
        chamberDisplayText.text = displayStr;
    }
}