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

        // 2. Ensure MapManager exists, then reset run path
        EnsureMapManagerExists();
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ResetRunPath();
        }

        // 3. Open the Kabbalah Map scene first
        SceneManager.LoadScene("KabbalahMapScene"); 
    }

    // --- DEBUG TESTING METHOD ---
    public void StartDebugFullMap()
    {
        if (BulletInventory.Instance != null)
        {
            BulletInventory.Instance.ResetChamberToDefault();
        }

        // Enable static debug unlock flag
        MapManager.debugUnlockAllPending = true;

        EnsureMapManagerExists();
        if (MapManager.Instance != null)
        {
            MapManager.Instance.UnlockAllNodesForTesting();
        }

        SceneManager.LoadScene("KabbalahMapScene");
    }

    // Helper to spawn a MapManager on the fly if it's missing from the Main Menu
    private void EnsureMapManagerExists()
    {
        if (MapManager.Instance == null)
        {
            GameObject managerObj = new GameObject("MapManager_AutoSpawned");
            managerObj.AddComponent<MapManager>();
            Debug.Log("MapManager was missing from Main Menu. Auto-spawned a fresh instance.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}