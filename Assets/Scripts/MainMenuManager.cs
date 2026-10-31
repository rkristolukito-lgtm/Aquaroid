using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartNewRun()
    {
        // 1. Reset player bullet chamber to fresh defaults
        if (BulletInventory.Instance != null)
        {
            BulletInventory.Instance.ResetChamberToDefault();
        }

        // 2. Reset Map progress / Da'at state if MapManager exists
        if (MapManager.Instance != null)
        {
            MapManager.Instance.daatUnlocked = false;
        }

        // 3. Load your first Combat Room (or Kabbalah Map if run starts from map)
        SceneManager.LoadScene("CombatRoomScene"); 
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}