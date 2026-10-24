using JetBrains.Annotations;
using System.Threading;
using TMPro;
using UnityEngine;

public enum PromptState { Gap, Active, Feedback }



public class PromptManager : MonoBehaviour
{
    public SnailController snail;
    public DirectionReader reader;
    public MotionRecognizer recognizer;
    public MotionDef[] motions;

    public float missPenalty = -1.5f;
    public float timeoutPenalty = -1.0f;

    public TMP_Text debugText;

    private float timer;
    private float promptTime;

    public string lastResult;
    MotionDef currentPrompt;

    PromptState state;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (snail.IsDead) return;



        timer -= Time.deltaTime;

        switch (state)
        {
            case PromptState.Gap:
                if (timer <= 0) StartPrompt();
                break;
            case PromptState.Active:
                if (timer <= 0f) Finish(false, "TOO SLOW", timeoutPenalty);
                break;
            case PromptState.Feedback:
                if (timer <= 0f) { state = PromptState.Gap; timer = 0.5f; }
                break;
        }
        UpdateDebugText();
    }

    public void StartPrompt()
    {
        currentPrompt = motions[Random.Range(0, motions.Length)];
        promptTime = 3;
        timer = promptTime;
        state = PromptState.Active;
    }

    public void Finish(bool hit, string message, float speedChange)
    {
        lastResult = message;
        snail.ChangeSpeed(speedChange);

        state = PromptState.Feedback;
        timer = 0.3f;
    }

    void OnEnable() { if (reader != null) reader.ButtonPressed += OnButton; }
    void OnDisable() { if (reader != null) reader.ButtonPressed -= OnButton; }

    public void OnButton(AttackButton button)
    {
        if (snail.IsDead) return;

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

    void UpdateDebugText()
    {
        if (debugText == null) return;

        if (state == PromptState.Active)
            debugText.text = $"{currentPrompt.displayName}  {string.Join(" ", currentPrompt.sequence)} + {currentPrompt.button}\n{timer:0.0}s";
        else
            debugText.text = lastResult;
    }

}
