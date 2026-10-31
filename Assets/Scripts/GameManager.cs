using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Enemy Tracking")]
    public int enemyCount = 0;   
    public int totalEnemiesToSpawn = 6; 
    private bool levelFinished = false;

    [Header("Player Lives")]
    public int playerLives = 10;
    public int maxLives = 10;

    [Header("References")]
    public BulletRewardManager rewardManager; 
    public GameObject gameOverPanel; 

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(false);
        }
        
        levelFinished = false;
        enemyCount = 6; // Fixed baseline count
        playerLives = maxLives; // Reset lives to 10 at the start of every level/room
        
        Debug.Log($"GameManager initialized. Enemy target set to 6. Lives reset to: {playerLives}");
    }

    // Call this from your Enemy script when its HP hits 0
    public void NotifyEnemyDefeated()
    {
        if (levelFinished) return;
        
        levelFinished = true;
        Debug.Log("--- ALL ENEMIES DEFEATED ---");

        if (rewardManager != null)
        {
            rewardManager.OpenRewardScreen();
        }
        else
        {
            Debug.LogWarning("RewardManager is NULL on GameManager! Falling back to map.");
            SceneManager.LoadScene("KabbalahMapScene");
        }
    }

    // Call this from your player script when taking damage
    public void LoseLife()
    {
        if (levelFinished) return;

        playerLives--;
        Debug.Log($"Player took damage! Health remaining: {playerLives} / {maxLives}");

        if (playerLives <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        Debug.Log("Player Died - Showing Game Over Screen");
        levelFinished = true; 

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            RestartGame();
        }
        
        yield return null;
    }

    public void RetryGame()
    {
        if (BulletInventory.Instance != null)
        {
            BulletInventory.Instance.ResetChamberToDefault();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void RestartGame()
    {
        if (BulletInventory.Instance != null)
        {
            BulletInventory.Instance.ResetChamberToDefault();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}