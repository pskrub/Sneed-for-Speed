using System;
using UnityEngine;

// Moves the snail to the right along the road.
// Speed always drains a little each second ("don't stop moving"),
// and the match-3 board adds or removes speed via ChangeSpeed().
public class SnailController : MonoBehaviour
{
    [Header("Speed")]
    public float startSpeed = 2f;
    public float minSpeed = 0f;
    public float maxSpeed = 12f;
    [Tooltip("How much speed is lost every second if you stop matching.")]
    public float decayPerSecond = 0.35f;

    public float Speed { get; private set; }
    public bool IsDead { get; private set; }
    public float Distance => transform.position.x - startX;

    public event Action Died;

    float startX;

    void Start()
    {
        Speed = startSpeed;
        startX = transform.position.x;
    }

    void Update()
    {
        if (IsDead) return;

        Speed = Mathf.Max(minSpeed, Speed - decayPerSecond * Time.deltaTime);
        transform.position += Vector3.right * Speed * Time.deltaTime;
    }

    public void ChangeSpeed(float amount)
    {
        if (IsDead) return;
        Speed = Mathf.Clamp(Speed + amount, minSpeed, maxSpeed);
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Died?.Invoke();
    }
}
