using UnityEngine;
using TMPro;

public class LivesDisplay : MonoBehaviour
{
    private TextMeshProUGUI livesText;

    void Awake()
    {
        livesText = GetComponent<TextMeshProUGUI>();
        if (livesText == null)
        {
            Debug.LogError("LivesDisplay requires a TextMeshProUGUI component on the same GameObject!");
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && livesText != null)
        {
            livesText.text = $"Lives: {GameManager.Instance.playerLives} / {GameManager.Instance.maxLives}";
        }
    }
}