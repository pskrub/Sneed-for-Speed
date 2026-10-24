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
        timer -= Time.deltaTime;

        switch (state)
        {
            case PromptState.Gap:
                if (timer <= 0) StartPrompt();
                break;
            case PromptState.Active:
                if (Timer <= 0f) Finish(false, "MISSED");
                break;
            case PromptState.Feedback:
                if (timer <= 0f) { state = PromptState.Gap; timer = 0.5; }
        }
        UpdateDebugText();
    }

    public void StatePrompt()
    {
        currentPrompt = motions[Random.Range(0, motions.Length)];
        promptTime = 3;
        timer = promptTime;
    }

    public void Finish(bool hit, string message)
    {
        lastResult = message;
        if (hit == false)
            snail.ChangeSpeed(timeoutPenalty);
        if (hit == true)
            snail.ChangeSpeed(current.speedReward);
        if (message == "Too slow")
            snail.ChangeSpeed(timeoutPenalty);
        if (message == "Miss")
            snail.ChangeSpeed(missPenalty);

        state = PromptState.Feedback; timer = 0.3f;
    }

    void OnEnable() { if (reader != null) reader.ButtonPressed += OnButton; }
    void OnDisable() { if (reader != null) reader.ButtonPressed -= OnButton; }

    public void OnButton(AttackButton button)
    {
        if (state != Active)
            return;

        if (button != current.button)
            Finish(false, "Wrong Button");

        if (recognizer.Check(current) == false)
            Finish(false, "Miss");

        if (hit == true)
            reader.ClearHistory();
            Finish(true, "Hit!");
       
    }
}
