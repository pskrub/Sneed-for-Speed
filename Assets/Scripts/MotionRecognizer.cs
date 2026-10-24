using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using static MotionDef;

public class MotionRecognizer : MonoBehaviour
{
    public DirectionReader reader;
    
    
    [Header("Leniency")]
    [Tooltip("Hvor mange fejlinputs må der være?")]
    public int maxStrayInputs = 2;

    [Tooltip("Input window i forhold til button presses.")]
    public float buttonWindow = 0.2f;

    [Header("Bare for at teste")]
    public MotionDef testMotion;
    public TMP_Text testText;

    // ---------- Test harness ----------

    void OnEnable()
    {
        if (reader != null) reader.ButtonPressed += OnTestButton;
    }

    void OnDisable()
    {
        if (reader != null) reader.ButtonPressed -= OnTestButton;
    }

    void OnTestButton(AttackButton button)
    {
        if (testMotion == null) return;

        bool hit = Check(testMotion);
        if (testText != null) testText.text = hit ? "HIT!" : "MISS";
        if (hit) reader.ClearHistory();
    }


    // Rent faktisk checket her-------------------------------------------------------

    // ---------- The actual check ----------

    public bool Check(MotionDef motion)
    {
        int[] seq = motion.sequence;

        // Button-only prompt: no directions needed.
        if (seq == null || seq.Length == 0) return true;

        var history = reader.History;
        if (history.Count == 0) return false;

        int s = seq.Length - 1;      // which step of the motion we're looking for (start at the last)
        int h = history.Count - 1;   // which history entry we're looking at (start at the newest)
        int strays = 0;

        float lastStepStart = 0f;    // when the player reached the LAST direction of the motion
        float lastStepEnd = 0f;      // when they left it again (or now, if still holding it)
        float firstStepStart = 0f;   // when they started the FIRST direction of the motion

        while (s >= 0 && h >= 0)
        {
            int dir = history[h].direction;

            if (dir == seq[s])
            {
                // Found this step.
                if (s == seq.Length - 1)
                {
                    lastStepStart = history[h].time;
                    bool stillHeld = h == history.Count - 1;
                    lastStepEnd = stillHeld ? Time.time : history[h + 1].time;
                }
                firstStepStart = history[h].time;

                s--;
                h--;
                strays = 0;
            }
            else if (IsOptionalDiagonal(seq, s) && dir == seq[s - 1])
            {
                // Player skipped this diagonal (e.g. did 2 6 instead of 2 3 6). Allowed.
                // Move to the previous step without using up this history entry.
                s--;
            }
            else
            {
                // An unexpected direction. Allow a few, then give up.
                strays++;
                if (strays > maxStrayInputs) return false;
                h--;
            }
        }

        // Ran out of history before finding every step.
        if (s >= 0) return false;

        // The button must come soon after the last direction.
        if (Time.time - lastStepEnd > buttonWindow) return false;


        if (lastStepStart - firstStepStart > motion.timeLimit) return false;
        // TODO: the whole motion must be done fast enough.
        //       If the time from firstStepStart to lastStepStart is MORE than
        //       motion.timeLimit, return false.

        return true;
    }

    // A diagonal in the middle of a motion may be skipped.
    // The first and last steps are never optional.
    static bool IsOptionalDiagonal(int[] seq, int s)
    {
        if (s <= 0 || s >= seq.Length - 1) return false;
        int d = seq[s];
        return d == 1 || d == 3 || d == 7 || d == 9;
    }
}
