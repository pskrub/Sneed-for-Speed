using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MotionDef", menuName = "Snail/Motion")]
public class MotionDef : ScriptableObject
{
    public enum AttackButton
    {
        LeftPunch,
        RightPunch,
        LeftKick,
        RightKick
    }

    public string displayName;
    public int[] sequence;
    public float timeLimit;
    public int difficulty;
    public float speedReward;

    public AttackButton button;


    
}
