using TMPro;
using UnityEngine;
using UnityEngine.UI;

// NEW SCRIPT
// Put this on any object in the main menu scene.
//
// With checkboxes (Toggles in a Toggle Group, only one can be ticked):
//   hook each Toggle's On Value Changed (Boolean) up to SelectEasy / SelectMedium / SelectHard
//   (pick them from the "Dynamic bool" part of the list).
// With plain buttons:
//   hook each Button's On Click () up to SetEasy / SetMedium / SetHard.
public class DifficultyMenu : MonoBehaviour
{
    [Tooltip("Optional text that shows the current choice, e.g. \"Difficulty: Medium\".")]
    public TMP_Text selectedText;

    // CHANGED: one checkbox per difficulty (was one for diagonals and one for buttons)
    public Toggle easyToggle;
    public Toggle mediumToggle;
    public Toggle hardToggle;

    void Start()
    {
        GameSettings.Load(); // start with the last choice
        UpdateText();
    }

    // ---------- For buttons ----------

    // Easy: straight directions only
    public void SetEasy()
    {
        GameSettings.Set("Easy", false, false);
        UpdateText();
    }

    // Medium: directions + diagonals. CHANGED: was SetNormal
    public void SetMedium()
    {
        GameSettings.Set("Medium", true, false);
        UpdateText();
    }

    // Hard: everything
    public void SetHard()
    {
        GameSettings.Set("Hard", true, true);
        UpdateText();
    }

    // ---------- For checkboxes ----------
    // NEW: a toggle also calls its function when it gets UNticked,
    // so these only react when it's ticked.

    public void SelectEasy(bool on) { if (on) SetEasy(); }
    public void SelectMedium(bool on) { if (on) SetMedium(); }
    public void SelectHard(bool on) { if (on) SetHard(); }

    void UpdateText()
    {
        if (selectedText != null) selectedText.text = "Difficulty: " + GameSettings.difficultyName;

        // CHANGED: tick the checkbox that matches the current choice.
        // "WithoutNotify" so this doesn't call the Select functions again.
        string name = GameSettings.difficultyName;
        if (easyToggle != null) easyToggle.SetIsOnWithoutNotify(name == "Easy");
        if (mediumToggle != null) mediumToggle.SetIsOnWithoutNotify(name == "Medium");
        if (hardToggle != null) hardToggle.SetIsOnWithoutNotify(name == "Hard");
    }
}