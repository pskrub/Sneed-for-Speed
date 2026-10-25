using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// NEW SCRIPT
// Distance, best score, speed, a chaser warning, a 3-2-1 countdown and the game over screen.
// Builds all of its own UI in code: put it on an empty object under the Canvas.
// Restart with Enter, Space or R.
public class GameHUD : MonoBehaviour
{
    public SnailSpeedCalc snail;
    public SplineMovement snailSpline;
    public ChaserSpeedCalc chaser;

    [Header("Start")]
    public bool useCountdown = true;
    public float countdownStep = 0.8f;

    [Header("Layout")]
    [Tooltip("Same as the camera's Viewport Rect Y. The HUD texts go above this line.")]
    [Range(0f, 0.6f)] public float promptAreaHeight = 0.4f;

    [Header("Danger warning")]
    [Tooltip("Show the red warning when the chaser's Gap is smaller than this (Gap is in track pieces).")]
    public float dangerGap = 0.25f;

    // NEW: wait this long after the snail is caught before showing the game over screen,
    // so the caught animation can be seen first
    [Header("Game over")]
    public float gameOverDelay = 1.5f;

    // NEW (nerd): an object to show on the game over screen, e.g. the Nerd. Hidden until game over.
    public GameObject showOnGameOver;

    // NEW (menu button): a Main Menu button, shown on the game over screen. Hook its On Click () to GoToMainMenu.
    public GameObject mainMenuButton;

    TMP_Text distanceText, bestText, speedText, dangerText, countdownText;
    TMP_Text gameOverTitle, gameOverText;
    GameObject gameOverPanel;

    float best;
    bool gameOver;
    float gameOverTime;
    const string BestKey = "SnailBestDistance";

    void Awake()
    {
        Time.timeScale = 1f;
        BuildLayout();
    }

    void Start()
    {
        best = PlayerPrefs.GetFloat(BestKey, 0f);
        if (snail != null) snail.Died += OnDied;
        if (useCountdown) StartCoroutine(Countdown());
        if (showOnGameOver != null) showOnGameOver.SetActive(false); // NEW (nerd): hidden while playing
        if (mainMenuButton != null) mainMenuButton.SetActive(false); // NEW (menu button): hidden while playing
    }

    void OnDestroy()
    {
        if (snail != null) snail.Died -= OnDied;
    }

    // Pauses the whole game (timeScale 0) while counting down, so no other script needs to know about it.
    IEnumerator Countdown()
    {
        Time.timeScale = 0f;

        for (int i = 3; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(countdownStep);
        }

        countdownText.text = "GO!";
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(0.6f);
        countdownText.text = "";
    }

    void Update()
    {
        if (snailSpline != null)
        {
            distanceText.text = $"{snailSpline.distance:0} cm";
            speedText.text = $"Speed {snailSpline.speed:0.0}";
        }
        bestText.text = $"Best: {best:0} cm";

        bool running = !gameOver && Time.timeScale > 0f;
        if (running && chaser != null && chaser.Gap < dangerGap)
        {
            dangerText.text = "IT'S RIGHT BEHIND YOU!";
            dangerText.transform.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.time * 18f));
        }
        else
        {
            dangerText.text = "";
        }

        if (gameOver && Time.time - gameOverTime > 0.7f)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame))
                Restart();

            // NEW (menu button): M or Escape goes back to the main menu
            if (kb != null && (kb.mKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                GoToMainMenu();
        }
    }

    void OnDied()
    {
        gameOver = true;
        gameOverTime = Time.time + gameOverDelay; // CHANGED: was Time.time. Restart works 0.7 s after the screen shows

        float distance = snailSpline != null ? snailSpline.distance : 0f;
        bool newBest = distance > best;
        if (newBest)
        {
            best = distance;
            PlayerPrefs.SetFloat(BestKey, best);
            PlayerPrefs.Save();
        }

        gameOverText.text = $"You crawled {distance:0} cm\n" +
                            (newBest ? "NEW BEST!" : $"Best: {best:0} cm") +
                            "\n\n<size=60%>Press Enter, Space or R to try again</size>" +
                            "\n<size=50%>M or Esc for the main menu</size>"; // NEW (menu button)
        StartCoroutine(ShowGameOverLater()); // CHANGED: was gameOverPanel.SetActive(true); right away
    }

    // NEW: shows the game over screen after gameOverDelay seconds
    IEnumerator ShowGameOverLater()
    {
        yield return new WaitForSeconds(gameOverDelay);
        gameOverPanel.SetActive(true);

        // NEW (nerd): show him too, drawn on top of the game over screen
        if (showOnGameOver != null)
        {
            showOnGameOver.SetActive(true);
            showOnGameOver.transform.SetAsLastSibling();
        }

        // NEW (menu button): show the Main Menu button, drawn on top so it can be clicked
        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
            mainMenuButton.transform.SetAsLastSibling();
        }
    }

    // NEW (menu button): back to the main menu (scene 0). Hook the button's On Click () to this.
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        if (scene.buildIndex < 0)
        {
            Debug.LogError("Can't restart: add this scene to File > Build Profiles (Scene List).");
            return;
        }
        SceneManager.LoadScene(scene.buildIndex);
    }

    // ---------- Building the UI ----------

    void BuildLayout()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        transform.SetAsLastSibling(); // draw on top of the prompt area

        distanceText = CreateText("Distance", transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -20f), 56f, TextAlignmentOptions.TopLeft);
        distanceText.fontStyle = FontStyles.Bold;
        bestText = CreateText("Best", transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -90f), 32f, TextAlignmentOptions.TopLeft);
        bestText.color = new Color(1f, 1f, 1f, 0.7f);

        speedText = CreateText("Speed", transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -20f), 44f, TextAlignmentOptions.TopRight);
        dangerText = CreateText("Danger", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), 48f, TextAlignmentOptions.Top);
        dangerText.fontStyle = FontStyles.Bold;
        dangerText.color = Color.red;

        float worldCenter = promptAreaHeight + (1f - promptAreaHeight) / 2f;
        countdownText = CreateText("Countdown", transform, new Vector2(0.5f, worldCenter), new Vector2(0.5f, 0.5f), Vector2.zero, 180f, TextAlignmentOptions.Center);
        countdownText.fontStyle = FontStyles.Bold;
        countdownText.rectTransform.sizeDelta = new Vector2(1400f, 300f);
        countdownText.text = "";

        // Game over overlay over the whole screen.
        gameOverPanel = new GameObject("GameOver", typeof(RectTransform), typeof(Image));
        gameOverPanel.transform.SetParent(transform, false);
        var panelRect = (RectTransform)gameOverPanel.transform;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = gameOverPanel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);
        panelImage.raycastTarget = false;

        gameOverTitle = CreateText("Title", gameOverPanel.transform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, 140f, TextAlignmentOptions.Center);
        gameOverTitle.text = "CAUGHT!";
        gameOverTitle.color = new Color(1f, 0.35f, 0.3f);
        gameOverTitle.fontStyle = FontStyles.Bold;
        gameOverTitle.rectTransform.sizeDelta = new Vector2(1400f, 220f);

        gameOverText = CreateText("Score", gameOverPanel.transform, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero, 56f, TextAlignmentOptions.Center);
        gameOverText.rectTransform.sizeDelta = new Vector2(1400f, 400f);

        gameOverPanel.SetActive(false);
    }

    static TMP_Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.color = Color.white;

        var rt = text.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(900f, 120f);
        return text;
    }
}