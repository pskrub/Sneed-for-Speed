using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// NEW SCRIPT
// Draws the current prompt in the bottom part of the screen.
// It builds all of its own UI in code (arrows, button dots, timer bar, texts),
// so you only need an empty object under the Canvas with this script on it.
public class PromptUI : MonoBehaviour
{
    public PromptManager prompts;

    [Header("Layout")]
    [Tooltip("How much of the screen (from the bottom) the prompt area uses. Match the camera's Viewport Rect Y.")]
    [Range(0.1f, 0.6f)] public float areaHeight = 0.4f;
    public float iconSize = 110f;
    public float iconSpacing = 18f;
    public float timerWidth = 900f;

    [Header("Colors")]
    public Color backgroundColor = new Color(0.08f, 0.08f, 0.12f, 0.92f);
    public Color arrowColor = Color.white;
    [Tooltip("Colors for buttons 1-4.")]
    public Color[] buttonColors =
    {
        new Color(1.00f, 0.85f, 0.20f), // 1 yellow
        new Color(1.00f, 0.55f, 0.15f), // 2 orange
        new Color(0.30f, 0.65f, 1.00f), // 3 blue
        new Color(0.35f, 0.90f, 0.45f), // 4 green
    };
    public Color dimDotColor = new Color(1f, 1f, 1f, 0.15f);
    public Color hitColor = new Color(0.35f, 1f, 0.45f);
    public Color missColor = new Color(1f, 0.35f, 0.30f);

    [Header("Help")]
    [Tooltip("Show the actual keys under each prompt (S → S+D → D + U).")]
    public bool showKeyHints = true;

    // NEW: untick to hide the "LEVEL 1" text in the corner
    public bool showLevel = true;

    RectTransform iconRow;
    GameObject timerRoot;
    Image timerFill;
    TMP_Text feedbackText, streakText, levelText, hintText, controlsText;

    Sprite whiteSprite, circleSprite, arrowSprite;
    readonly List<GameObject> icons = new List<GameObject>();
    bool wasActive;

    static readonly string[] buttonKeys = { "U", "I", "J", "K" };

    void Awake()
    {
        whiteSprite = MakeWhiteSprite();
        circleSprite = MakeCircleSprite(64);
        arrowSprite = MakeArrowSprite(64);
        BuildLayout();
    }

    void BuildLayout()
    {
        var area = GetComponent<RectTransform>();
        if (area == null) area = gameObject.AddComponent<RectTransform>();
        area.anchorMin = Vector2.zero;
        area.anchorMax = new Vector2(1f, areaHeight);
        area.offsetMin = Vector2.zero;
        area.offsetMax = Vector2.zero;

        // Background over the whole prompt area.
        var bg = CreateImage("Background", area, whiteSprite, backgroundColor, Vector2.zero);
        var bgRect = bg.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Row where the arrows and button icon go.
        iconRow = CreateRect("Icons", area, new Vector2(0.5f, 0.62f));

        // Big HIT / MISS text, same spot as the icons.
        feedbackText = CreateText("Feedback", area, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, 90f, TextAlignmentOptions.Center);
        feedbackText.fontStyle = FontStyles.Bold;

        // Key hints under the icons.
        hintText = CreateText("KeyHints", area, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero, 32f, TextAlignmentOptions.Center);
        hintText.color = new Color(1f, 1f, 1f, 0.6f);

        // Timer bar.
        timerRoot = CreateRect("Timer", area, new Vector2(0.5f, 0.26f)).gameObject;
        var timerBg = CreateImage("TimerBackground", timerRoot.transform, whiteSprite, new Color(1f, 1f, 1f, 0.12f), new Vector2(timerWidth, 26f));
        timerFill = CreateImage("TimerFill", timerRoot.transform, whiteSprite, hitColor, new Vector2(timerWidth, 26f));
        timerFill.type = Image.Type.Filled;
        timerFill.fillMethod = Image.FillMethod.Horizontal;
        timerFill.fillOrigin = 0;
        timerBg.transform.SetAsFirstSibling();

        // Streak (top right) and level (top left).
        streakText = CreateText("Streak", area, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -20f), 44f, TextAlignmentOptions.TopRight);
        streakText.fontStyle = FontStyles.Bold;
        streakText.color = buttonColors.Length > 0 ? buttonColors[0] : Color.yellow;

        levelText = CreateText("Level", area, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -20f), 36f, TextAlignmentOptions.TopLeft);
        levelText.color = new Color(1f, 1f, 1f, 0.7f);

        // Controls reminder at the bottom.
        controlsText = CreateText("Controls", area, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), 26f, TextAlignmentOptions.Bottom);
        controlsText.color = new Color(1f, 1f, 1f, 0.45f);
        controlsText.text = "Move: WASD        Buttons:  1 = U   2 = I   3 = J   4 = K";
        if (prompts != null && !prompts.useButtons) controlsText.text = "Move: WASD or the arrow keys"; // NEW (arrows only)
    }

    void Update()
    {
        if (prompts == null) return;

        // NEW (difficulty menu): set the controls text every frame, so it matches the difficulty
        // picked in the menu (the menu's choice is applied after this text was first made)
        controlsText.text = prompts.useButtons
            ? "Move: WASD        Buttons:  1 = U   2 = I   3 = J   4 = K"
            : "Move: WASD or the arrow keys";

        // Hidden after game over and during the countdown (the game is paused then).
        bool stopped = prompts.snail == null || prompts.snail.IsDead || Time.timeScale == 0f;
        bool active = !stopped && prompts.state == PromptState.Active && prompts.currentPrompt != null;
        bool feedback = !stopped && prompts.state == PromptState.Feedback;

        // Redraw the icons each time a new prompt starts.
        if (active && !wasActive) BuildIcons(prompts.currentPrompt);
        wasActive = active;

        iconRow.gameObject.SetActive(active);
        timerRoot.SetActive(active);
        hintText.gameObject.SetActive(active && showKeyHints);

        if (active)
        {
            float t = prompts.promptTime > 0f ? Mathf.Clamp01(prompts.timer / prompts.promptTime) : 0f;
            timerFill.fillAmount = t;
            timerFill.color = Color.Lerp(missColor, hitColor, t);
        }

        feedbackText.gameObject.SetActive(feedback);
        if (feedback)
        {
            bool hit = prompts.lastResult == "HIT!";
            string motionName = prompts.currentPrompt != null ? prompts.currentPrompt.displayName : "";
            feedbackText.text = hit && !string.IsNullOrEmpty(motionName) ? motionName.ToUpper() + "!" : prompts.lastResult;
            feedbackText.color = hit ? hitColor : missColor;

            // Small "pop" as it appears.
            float k = Mathf.Clamp01(prompts.timer / 0.3f); // 0.3 = the feedback time in PromptManager.Finish
            feedbackText.transform.localScale = Vector3.one * (1f + 0.3f * k);
        }

        streakText.text = !stopped && prompts.streak >= 2 ? $"x{prompts.streak} STREAK" : "";
        // CHANGED: added "|| !showLevel", so the text stays empty when Show Level is off
        levelText.text = stopped || !showLevel ? "" : $"LEVEL {Mathf.FloorToInt(prompts.level) + 1}";
    }

    // ---------- Drawing one prompt ----------

    void BuildIcons(MotionDef motion)
    {
        foreach (var go in icons) Destroy(go);
        icons.Clear();

        int[] seq = motion.sequence ?? new int[0];
        bool buttonOnly = seq.Length == 0;
        float buttonSize = buttonOnly ? iconSize * 1.4f : iconSize;
        float plusWidth = iconSize * 0.5f;
        bool showButton = prompts.useButtons || buttonOnly; // NEW (arrows only): no "+" or button square when buttons are off

        // Work out the total width so the row is centered.
        var widths = new List<float>();
        foreach (int _ in seq) widths.Add(iconSize);
        if (showButton && !buttonOnly) widths.Add(plusWidth); // CHANGED (arrows only): added showButton
        if (showButton) widths.Add(buttonSize);               // CHANGED (arrows only): added showButton

        float total = iconSpacing * (widths.Count - 1);
        foreach (float w in widths) total += w;

        float x = -total / 2f;
        int item = 0;

        foreach (int dir in seq)
        {
            float w = widths[item++];
            Image arrow;
            if (dir == 5)
            {
                arrow = CreateImage("Neutral", iconRow, circleSprite, arrowColor, Vector2.one * iconSize * 0.35f);
            }
            else
            {
                arrow = CreateImage("Arrow " + dir, iconRow, arrowSprite, arrowColor, Vector2.one * iconSize);
                arrow.rectTransform.localEulerAngles = new Vector3(0f, 0f, AngleFor(dir));
            }
            arrow.rectTransform.anchoredPosition = new Vector2(x + w / 2f, 0f);
            icons.Add(arrow.gameObject);
            x += w + iconSpacing;
        }

        // NEW (arrows only): stop here when there's no button to draw
        if (!showButton)
        {
            hintText.text = KeyHint(seq, -1);
            return;
        }

        if (!buttonOnly)
        {
            float w = widths[item++];
            var plus = CreateText("Plus", iconRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x + w / 2f, 0f), 70f, TextAlignmentOptions.Center);
            plus.rectTransform.sizeDelta = new Vector2(w, iconSize);
            plus.text = "+";
            icons.Add(plus.gameObject);
            x += w + iconSpacing;
        }

        float bw = widths[item];
        var buttonIcon = BuildButtonIcon((int)motion.button, buttonSize);
        buttonIcon.anchoredPosition = new Vector2(x + bw / 2f, 0f);
        icons.Add(buttonIcon.gameObject);

        hintText.text = KeyHint(seq, (int)motion.button);
    }

    // Tekken-style 2x2 dots: punches on top, kicks below. The needed one is lit.
    RectTransform BuildButtonIcon(int lit, float size)
    {
        var root = CreateRect("Button", iconRow, new Vector2(0.5f, 0.5f));
        root.sizeDelta = Vector2.one * size;

        float dot = size * 0.44f;
        float offset = size * 0.26f;
        Vector2[] positions =
        {
            new Vector2(-offset,  offset), // 1 left punch
            new Vector2( offset,  offset), // 2 right punch
            new Vector2(-offset, -offset), // 3 left kick
            new Vector2( offset, -offset), // 4 right kick
        };

        for (int i = 0; i < 4; i++)
        {
            bool on = i == lit;
            Color c = on ? ButtonColor(i) : dimDotColor;
            var img = CreateImage("Dot " + (i + 1), root, circleSprite, c, Vector2.one * (on ? dot * 1.15f : dot));
            img.rectTransform.anchoredPosition = positions[i];

            if (on)
            {
                var num = CreateText("Number", img.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, dot * 0.75f, TextAlignmentOptions.Center);
                num.rectTransform.sizeDelta = Vector2.one * dot * 1.15f;
                num.text = (i + 1).ToString();
                num.color = new Color(0.1f, 0.1f, 0.12f);
                num.fontStyle = FontStyles.Bold;
            }
        }
        return root;
    }

    Color ButtonColor(int i)
    {
        return i < buttonColors.Length ? buttonColors[i] : Color.white;
    }

    static string KeyHint(int[] seq, int button)
    {
        string key = button >= 0 && button < buttonKeys.Length ? buttonKeys[button] : "?";
        if (seq.Length == 0) return "Press " + key;

        var parts = new List<string>();
        foreach (int dir in seq) parts.Add(KeysFor(dir));
        if (button < 0) return string.Join("  →  ", parts); // NEW (arrows only): no button key at the end
        return string.Join("  →  ", parts) + "   +   " + key;
    }

    static string KeysFor(int dir)
    {
        switch (dir)
        {
            case 1: return "S+A";
            case 2: return "S";
            case 3: return "S+D";
            case 4: return "A";
            case 6: return "D";
            case 7: return "W+A";
            case 8: return "W";
            case 9: return "W+D";
            default: return "·";
        }
    }

    // The arrow sprite points right (0°). Rotate counter-clockwise for the others.
    static float AngleFor(int dir)
    {
        switch (dir)
        {
            case 6: return 0f;
            case 9: return 45f;
            case 8: return 90f;
            case 7: return 135f;
            case 4: return 180f;
            case 1: return 225f;
            case 2: return 270f;
            case 3: return 315f;
            default: return 0f;
        }
    }

    // ---------- Small UI helpers ----------

    static RectTransform CreateRect(string name, Transform parent, Vector2 anchor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        return rt;
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        img.rectTransform.sizeDelta = size;
        return img;
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
        rt.sizeDelta = new Vector2(1400f, 150f);
        return text;
    }

    // ---------- Sprites made in code, so no art is needed ----------

    static Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color32[16];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[size * size];
        float r = size / 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - d); // soft 1-pixel edge
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    static Sprite MakeArrowSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                float v = (y + 0.5f) / size;
                float off = Mathf.Abs(v - 0.5f);

                bool stem = u >= 0.10f && u <= 0.55f && off <= 0.11f;
                bool head = u >= 0.50f && u <= 0.92f && off <= (0.92f - u) * (0.40f / 0.42f);

                px[y * size + x] = (stem || head) ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}