using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static MotionDef;


public struct DirectionEntry
{
    public int direction;
    public float time;

    public DirectionEntry(int direction, float time)
    {
        this.direction = direction;
        this.time = time;
    }
}

public class DirectionReader : MonoBehaviour
{

    [Tooltip("Seconds of direction history to keep.")]
    public float historyLength = 1f;

    [Tooltip("Optional text that shows what the reader sees. Great for testing.")]
    public TMP_Text debugText;

    // Other scripts can read these, but only this script can change them.
    public int CurrentDirection { get; private set; } = 5;
    public AttackButton LastButton { get; private set; }
    public float LastButtonTime { get; private set; } = -1f;
    public List<DirectionEntry> History => history;

    // Other scripts can "listen" to this to react when a button is pressed.
    public event Action<AttackButton> ButtonPressed;

    readonly List<DirectionEntry> history = new List<DirectionEntry>();
    int lastDirection = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Reader running");
        // Bare noget keyboard check halløj
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        //Int til at styre og checke om frem og tilbage bliver trykket på samme tid. Så giver det nul.
        int x = 0;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1;

        // Det samme men op/ned
        int y = 0;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1;

        // Retningsudregning ud fra numpad.
        CurrentDirection = 5 + x + 3 * y;

        // Bruger en struct (en bundled datatype som ses øverst i script), der holder styr på historien af inputs.
        Debug.Log(CurrentDirection);
        if (CurrentDirection != lastDirection)
        {
            history.Add(new DirectionEntry(CurrentDirection, Time.time));
            lastDirection = CurrentDirection;
        }

        // Er ikke helt sikker på hvad det her gør (Drop old entries from the front, but always keep the newest one)
        // while the list has more than one entry and the first one is older than historyLength (Time.time - history[0].time > historyLength), remove it with RemoveAt(0).
        // Why “more than one”? So the newest entry is never removed. If a player holds → for two seconds and then presses the button, the history still knows they’re holding →
        while (history.Count > 1 && Time.time - history[0].time > historyLength)
            history.RemoveAt(0);

        if (keyboard.uKey.wasPressedThisFrame) PressButton(AttackButton.LeftPunch);
        if (keyboard.iKey.wasPressedThisFrame) PressButton(AttackButton.RightPunch);
        if (keyboard.jKey.wasPressedThisFrame) PressButton(AttackButton.LeftKick);
        if (keyboard.kKey.wasPressedThisFrame) PressButton(AttackButton.RightKick);

        UpdateDebugText();
    }

        void PressButton(AttackButton button)
        {
            LastButton = button;
            LastButtonTime = Time.time;
            ButtonPressed?.Invoke(button); // "?." = only if someone is listening
        }

        // Clear history kaldes efter hver gang man har lavet en motion så inputtet ikke bliver lavet to gange.
        public void ClearHistory()
    {
        history.Clear();

    }

    void UpdateDebugText()
    {
        if (debugText == null) return;

        string historyLine = "";
        foreach (var entry in history)
            historyLine += entry.direction + " ";

        string buttonLine = LastButtonTime < 0f ? "-" : LastButton.ToString();

        debugText.text = $"Now: {CurrentDirection}\nHistory: {historyLine}\nButton: {buttonLine}";
    }
}



