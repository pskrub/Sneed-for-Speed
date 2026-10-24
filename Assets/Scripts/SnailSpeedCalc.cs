using System;
using UnityEngine;

public class SnailSpeedCalc : MonoBehaviour
{
    public SplineMovement player;
    [Header("Speed")]
    public float startSpeed = 2f;
    public float minSpeed = 0f;
    public float maxSpeed = 12f;
    [Tooltip("How much speed is lost every second if you stop matching.")]
    public float decayPerSecond = 0.35f;
    
    public bool IsDead { get; private set; }
    
    public event Action Died;

    void Start()
    {
        player.speed = startSpeed;
    }
    
    void Update()
    {
        if (IsDead) return;

        player.speed = Mathf.Max(minSpeed, player.speed - decayPerSecond * Time.deltaTime);
    }
    
    public void ChangeSpeed(float amount)
    {
        if (IsDead) return;
        player.speed = Mathf.Clamp(player.speed + amount, minSpeed, maxSpeed);
    }
    
    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Died?.Invoke();
    }
}
