using UnityEngine;

// Whatever is chasing the snail (a bird, a boot, a wave of salt...).
// It slowly gets faster over time. If it reaches the snail, game over.
public class Chaser : MonoBehaviour
{
    public SnailController snail;

    public float startSpeed = 1.5f;
    [Tooltip("Speed added every second, so the game gets harder over time.")]
    public float acceleration = 0.04f;
    [Tooltip("Snail is caught when the gap is smaller than this.")]
    public float catchDistance = 0.6f;
    [Tooltip("Chaser never falls further behind than this, so it stays a threat.")]
    public float maxGap = 12f;

    public float Speed { get; private set; }
    public float Gap => snail.transform.position.x - transform.position.x;

    void Start()
    {
        Speed = startSpeed;
    }

    void Update()
    {
        if (snail == null || snail.IsDead) return;

        Speed += acceleration * Time.deltaTime;
        transform.position += Vector3.right * Speed * Time.deltaTime;

        // Rubber band: don't let the player escape forever.
        if (Gap > maxGap)
        {
            Vector3 p = transform.position;
            p.x = snail.transform.position.x - maxGap;
            transform.position = p;
        }

        if (Gap <= catchDistance)
            snail.Die();
    }
}
