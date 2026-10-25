#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// NEW SCRIPT (editor tool, not part of the game)
// Adds the menu "Snail > Create Arrow Prompts".
// It creates a set of arrow-only prompts in Assets/Motions/Arrows and adds them
// to the PromptManager in the open scene. Running it again updates the same files
// instead of making duplicates, so you can tweak the values below and run it again.
public static class ArrowPromptCreator
{
    const string Folder = "Assets/Motions/Arrows";

    [MenuItem("Snail/Create Arrow Prompts")]
    static void Create()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Motions")) AssetDatabase.CreateFolder("Assets", "Motions");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Motions", "Arrows");

        var created = new List<MotionDef>();

        // Numpad directions:  7 8 9  /  4 5 6  /  1 2 3   (6 = forward/right, 2 = down)
        //                file name              shown name               sequence                          time  diff  reward

        // Difficulty 0: single arrows (from the start)
        Add(created, "A0_Forward", "Forward", new[] { 6 }, 0.5f, 0, 0.5f);
        Add(created, "A0_Back", "Back", new[] { 4 }, 0.5f, 0, 0.5f);
        Add(created, "A0_Down", "Down", new[] { 2 }, 0.5f, 0, 0.5f);
        Add(created, "A0_Up", "Up", new[] { 8 }, 0.5f, 0, 0.5f);

        // Difficulty 1: diagonals and double taps (after ~20 s)
        Add(created, "A1_DownForward", "Down-Forward", new[] { 3 }, 0.5f, 1, 0.6f);
        Add(created, "A1_DownBack", "Down-Back", new[] { 1 }, 0.5f, 1, 0.6f);
        Add(created, "A1_UpForward", "Up-Forward", new[] { 9 }, 0.5f, 1, 0.6f);
        Add(created, "A1_UpBack", "Up-Back", new[] { 7 }, 0.5f, 1, 0.6f);
        Add(created, "A1_Dash", "Dash", new[] { 6, 6 }, 0.4f, 1, 0.8f);
        Add(created, "A1_BackDash", "Back Dash", new[] { 4, 4 }, 0.4f, 1, 0.8f);
        Add(created, "A1_Hop", "Hop", new[] { 2, 8 }, 0.5f, 1, 0.7f);

        // Difficulty 2: quarter circles (after ~40 s)
        Add(created, "A2_QuarterForward", "Slime Shot", new[] { 2, 3, 6 }, 0.5f, 2, 1.0f);
        Add(created, "A2_QuarterBack", "Shell Spin", new[] { 2, 1, 4 }, 0.5f, 2, 1.0f);
        Add(created, "A2_QuarterUpForward", "Rising Slime", new[] { 8, 9, 6 }, 0.5f, 2, 1.0f);
        Add(created, "A2_ZigZag", "Zig Zag", new[] { 6, 4, 6 }, 0.6f, 2, 1.1f);

        // Difficulty 3: dragon punch motions (after ~60 s)
        Add(created, "A3_DragonPunch", "Shell-ryuken", new[] { 6, 2, 3 }, 0.5f, 3, 1.4f);
        Add(created, "A3_ReverseDragon", "Reverse Shell-ryuken", new[] { 4, 2, 1 }, 0.5f, 3, 1.4f);
        Add(created, "A3_UpSwing", "Up Swing", new[] { 2, 3, 6, 9 }, 0.6f, 3, 1.4f);

        // Difficulty 4: half circles (after ~80 s)
        Add(created, "A4_HalfCircleForward", "Big Slurp", new[] { 4, 1, 2, 3, 6 }, 0.8f, 4, 1.7f);
        Add(created, "A4_HalfCircleBack", "Back Slurp", new[] { 6, 3, 2, 1, 4 }, 0.8f, 4, 1.7f);
        Add(created, "A4_ZigZagDash", "Zig Zag Dash", new[] { 6, 4, 6, 4 }, 0.8f, 4, 1.6f);
        Add(created, "A4_SlimeCombo", "Slime Combo", new[] { 2, 3, 6, 6 }, 0.7f, 4, 1.6f);

        // Difficulty 5: double motions (after ~100 s)
        Add(created, "A5_DoubleQuarter", "Mega Slime", new[] { 2, 3, 6, 2, 3, 6 }, 1.0f, 5, 2.3f);
        Add(created, "A5_DoubleQuarterBack", "Mega Spin", new[] { 2, 1, 4, 2, 1, 4 }, 1.0f, 5, 2.3f);
        Add(created, "A5_SnailStorm", "Snail Storm", new[] { 2, 3, 6, 2, 1, 4 }, 1.1f, 5, 2.4f);
        Add(created, "A5_FullHalfCircles", "Ultra Slurp", new[] { 4, 1, 2, 3, 6, 3, 2, 1, 4 }, 1.4f, 5, 2.8f);

        AssetDatabase.SaveAssets();

        // Add them to the PromptManager in the open scene (skips ones already in the list).
        var promptManager = Object.FindFirstObjectByType<PromptManager>();
        if (promptManager != null)
        {
            Undo.RecordObject(promptManager, "Add arrow prompts");
            var list = new List<MotionDef>();
            if (promptManager.motions != null) list.AddRange(promptManager.motions);
            foreach (var m in created)
                if (!list.Contains(m)) list.Add(m);
            promptManager.motions = list.ToArray();
            EditorUtility.SetDirty(promptManager);
            EditorSceneManager.MarkSceneDirty(promptManager.gameObject.scene);
            Debug.Log($"Created {created.Count} arrow prompts in {Folder} and added them to PromptManager. Save the scene (Ctrl+S).");
        }
        else
        {
            Debug.Log($"Created {created.Count} arrow prompts in {Folder}. No PromptManager found in the open scene, so drag them into its Motions list yourself.");
        }

        Selection.objects = created.ToArray();
    }

    static void Add(List<MotionDef> list, string fileName, string displayName, int[] sequence, float timeLimit, int difficulty, float speedReward)
    {
        string path = $"{Folder}/{fileName}.asset";

        var motion = AssetDatabase.LoadAssetAtPath<MotionDef>(path);
        bool isNew = motion == null;
        if (isNew) motion = ScriptableObject.CreateInstance<MotionDef>();

        motion.displayName = displayName;
        motion.sequence = sequence;
        motion.timeLimit = timeLimit;
        motion.difficulty = difficulty;
        motion.speedReward = speedReward;
        motion.button = AttackButton.LeftPunch; // ignored in arrows-only mode

        if (isNew) AssetDatabase.CreateAsset(motion, path);
        else EditorUtility.SetDirty(motion);

        list.Add(motion);
    }
}
#endif