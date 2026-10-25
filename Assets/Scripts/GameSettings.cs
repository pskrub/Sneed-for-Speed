using UnityEngine;

// NEW SCRIPT
// Holds the difficulty picked in the main menu. It's "static", so the values stay
// when the game scene loads. Nothing needs to be put on an object.
//   Easy   = straight directions only (no diagonals, no attack buttons)
//   Medium = directions + diagonals
//   Hard   = everything (diagonals + attack buttons)
public static class GameSettings
{
    public static bool chosen;              // true once a difficulty has been picked (or loaded)
    public static bool useDiagonals = true;
    public static bool useButtons = false;
    public static string difficultyName = "Medium";  // CHANGED: "Normal" is now called "Medium"

    public static void Set(string name, bool diagonals, bool buttons)
    {
        difficultyName = name;
        useDiagonals = diagonals;
        useButtons = buttons;
        chosen = true;

        // Remember the choice for next time the game is started.
        PlayerPrefs.SetString("Difficulty", name);
        PlayerPrefs.Save();
    }

    // Loads the last picked difficulty (Medium the first time).
    public static void Load()
    {
        string name = PlayerPrefs.GetString("Difficulty", "Medium");
        if (name == "Easy") Set("Easy", false, false);
        else if (name == "Hard") Set("Hard", true, true);
        else Set("Medium", true, false); // CHANGED: was "Normal" (an old saved "Normal" also ends up here)
    }
}