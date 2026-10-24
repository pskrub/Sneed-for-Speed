using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Shows distance, speed and how close the chaser is, plus a game over screen.
public class GameUI : MonoBehaviour
{
    public SnailController snail;
    public Chaser chaser;

    public TMP_Text distanceText;
    public TMP_Text speedText;
    public TMP_Text dangerText;

    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        snail.Died += ShowGameOver;
    }

    void OnDestroy()
    {
        if (snail != null) snail.Died -= ShowGameOver;
    }

    void Update()
    {
        if (distanceText != null) distanceText.text = $"{snail.Distance:0} cm";
        if (speedText != null) speedText.text = $"Speed {snail.Speed:0.0}";

        if (dangerText != null && chaser != null)
        {
            float gap = chaser.Gap;
            dangerText.text = gap < 3f ? "IT'S RIGHT BEHIND YOU!" : $"Chaser: {gap:0.0} behind";
            dangerText.color = gap < 3f ? Color.red : Color.white;
        }
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"You crawled {snail.Distance:0} cm";
    }

    // Hook this up to the Restart button's OnClick.
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
