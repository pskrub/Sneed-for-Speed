using JetBrains.Annotations;
using System.Threading;
using TMPro;
using UnityEngine;

public enum PromptState { Gap, Active, Feedback }



public class PromptManager : MonoBehaviour
{
    public SnailSpeedCalc snail; // CHANGED: was SnailController (your snail now uses SnailSpeedCalc)
    public DirectionReader reader;
    public MotionRecognizer recognizer;
    public MotionDef[] motions;

    public float missPenalty = -1.5f;
    public float timeoutPenalty = -1.0f;

    public TMP_Text debugText;

    // CHANGED: these four were private. PromptUI needs to read them, so they're public now.
    public float timer;
    public float promptTime;

    public string lastResult;
    public MotionDef currentPrompt;

    public PromptState state;

    // NEW: difficulty, so the game speeds up over time
    public float levelPerSecond = 0.1f;      // level 10 after about 100 seconds
    public float startPromptTime = 3f;       // seconds per prompt at the start
    public float promptTimePerLevel = 0.2f;  // each level gives this much less time
    public float minPromptTime = 0.8f;       // never less than this
    public float level;

    // NEW: hits in a row, shown by PromptUI
    public int streak;

    // NEW (arrows only): untick to play with arrows only. A prompt then counts as soon as
    // the arrows are done, with no punch or kick. Button-only prompts are skipped.
    public bool useButtons = false;

    // NEW (diagonals): untick to take the diagonal arrows (↘ ↙ ↗ ↖) out of every prompt.
    // ↓↘→ becomes ↓→. Prompts that are only a diagonal are skipped.
    public bool useDiagonals = true;
    MotionDef noDiagonalPrompt; // NEW (diagonals): a reused copy of the current prompt, without diagonals




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // NEW (difficulty menu): use the difficulty picked in the main menu.
        // If you press Play in the game scene directly, the Inspector checkboxes are used instead.
        if (GameSettings.chosen)
        {
            useDiagonals = GameSettings.useDiagonals;
            useButtons = GameSettings.useButtons;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (snail.IsDead) return;
        if (Time.timeScale == 0f) return; // NEW: paused during GameHUD's 3-2-1 countdown

        level += levelPerSecond * Time.deltaTime; // NEW: difficulty goes up over time

        timer -= Time.deltaTime;

        switch (state)
        {
            case PromptState.Gap:
                if (timer <= 0) StartPrompt();
                break;
            case PromptState.Active:
                // NEW (arrows only): check every frame whether the arrows are done, no button needed
                if (!useButtons && !IsButtonOnly(currentPrompt) && recognizer.Check(currentPrompt))
                {
                    reader.ClearHistory();
                    Finish(true, "HIT!", currentPrompt.speedReward);
                    break;
                }
                if (timer <= 0f) Finish(false, "TOO SLOW", timeoutPenalty);
                break;
            case PromptState.Feedback:
                // CHANGED: the pause was always 0.5f, now it gets shorter with the level (never under 0.15)
                if (timer <= 0f) { state = PromptState.Gap; timer = Mathf.Max(0.15f, 0.5f - level * 0.03f); }
                break;
        }
        UpdateDebugText();
    }

    public void StartPrompt()
    {
        // NEW: clear warning instead of a crash when the Motions list is empty
        if (motions.Length == 0)
        {
            Debug.LogWarning("PromptManager has no motions! Drag them into the Motions list.");
            timer = 1f;
            return;
        }

        currentPrompt = motions[Random.Range(0, motions.Length)];

        // NEW: only use prompts that are unlocked at this level (difficulty <= level / 2).
        // Tries more random picks until one fits.
        int maxDifficulty = Mathf.FloorToInt(level / 2f);
        // CHANGED (arrows only): 50 → 100 tries, and also re-pick button-only prompts when useButtons is off
        // CHANGED (diagonals): also re-pick prompts that are only diagonals when useDiagonals is off
        for (int i = 0; i < 100 && (currentPrompt.difficulty > maxDifficulty || (!useButtons && IsButtonOnly(currentPrompt)) || (!useDiagonals && OnlyDiagonals(currentPrompt))); i++)
            currentPrompt = motions[Random.Range(0, motions.Length)];

        // NEW (diagonals): use a copy of the prompt with the diagonal arrows taken out
        if (!useDiagonals) currentPrompt = WithoutDiagonals(currentPrompt);

        // NEW (arrows only): forget old arrow presses, so only arrows done after the prompt appears count
        if (!useButtons) reader.ClearHistory();

        // CHANGED: was promptTime = 3; now it gets shorter with the level
        promptTime = Mathf.Max(minPromptTime, startPromptTime - level * promptTimePerLevel);
        timer = promptTime;
        state = PromptState.Active;
    }

    public void Finish(bool hit, string message, float speedChange)
    {
        lastResult = message;
        snail.ChangeSpeed(speedChange);

        if (hit) streak++;   // NEW: count hits in a row
        else streak = 0;

        state = PromptState.Feedback;
        timer = 0.3f;
    }

    void OnEnable() { if (reader != null) reader.ButtonPressed += OnButton; }
    void OnDisable() { if (reader != null) reader.ButtonPressed -= OnButton; }

    public void OnButton(AttackButton button)
    {
        if (snail.IsDead) return;
        if (Time.timeScale == 0f) return; // NEW: ignore buttons during the countdown
        if (!useButtons) return; // NEW (arrows only): buttons do nothing in arrows-only mode

        if (state != PromptState.Active)
            return;

        if (button != currentPrompt.button)
        {
            Finish(false, "WRONG BUTTON", missPenalty);
            return;
        }


        if (recognizer.Check(currentPrompt) == false)
        {
            Finish(false, "MISS", missPenalty);
            return;
        }



        reader.ClearHistory();

        Finish(true, "HIT!", currentPrompt.speedReward);

    }

    // NEW (arrows only): true for prompts with no arrows (just a button)
    bool IsButtonOnly(MotionDef motion)
    {
        return motion.sequence == null || motion.sequence.Length == 0;
    }

    // NEW (diagonals): true for the diagonal directions 1, 3, 7 and 9
    bool IsDiagonal(int direction)
    {
        return direction == 1 || direction == 3 || direction == 7 || direction == 9;
    }

    // NEW (diagonals): true if the prompt has arrows, but all of them are diagonals
    bool OnlyDiagonals(MotionDef motion)
    {
        if (IsButtonOnly(motion)) return false;
        foreach (int direction in motion.sequence)
            if (!IsDiagonal(direction)) return false;
        return true;
    }

    // NEW (diagonals): copies the prompt into noDiagonalPrompt, leaving out the diagonal arrows.
    // The motion asset itself is never changed.
    MotionDef WithoutDiagonals(MotionDef motion)
    {
        if (noDiagonalPrompt == null) noDiagonalPrompt = ScriptableObject.CreateInstance<MotionDef>();

        int[] sequence = motion.sequence ?? new int[0];
        int count = 0;
        foreach (int direction in sequence)
            if (!IsDiagonal(direction)) count++;

        int[] newSequence = new int[count];
        int index = 0;
        foreach (int direction in sequence)
            if (!IsDiagonal(direction)) newSequence[index++] = direction;

        noDiagonalPrompt.displayName = motion.displayName;
        noDiagonalPrompt.sequence = newSequence;
        noDiagonalPrompt.timeLimit = motion.timeLimit;
        noDiagonalPrompt.difficulty = motion.difficulty;
        noDiagonalPrompt.speedReward = motion.speedReward;
        noDiagonalPrompt.button = motion.button;
        return noDiagonalPrompt;
    }

    void UpdateDebugText()
    {
        if (debugText == null) return;

        if (state == PromptState.Active)
            debugText.text = $"{currentPrompt.displayName}  {string.Join(" ", currentPrompt.sequence)} + {currentPrompt.button}\n{timer:0.0}s";
        else
            debugText.text = lastResult;
    }

}