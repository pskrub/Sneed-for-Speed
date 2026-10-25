using UnityEngine;

public class ChaserSpeedCalc : MonoBehaviour
{
    public SnailSpeedCalc snail;
    public SplineMovement snailSpline;
    public SplineMovement chaserSpline;
    public LevelManager levelManager;

    public float startSpeed = 1.5f;
    [Tooltip("Speed added every second, so the game gets harder over time.")]
    public float acceleration = 0.04f;
    [Tooltip("Snail is caught when the gap is smaller than this.")]
    public float catchDistance = 0.1f;
    [Tooltip("Chaser never falls further behind than this, so it stays a threat.")]
    public float maxGap = 0.5f;

    public float Gap => snailSpline.progress + levelManager.playerIndex - chaserSpline.progress - levelManager.chaserIndex;

    void Start()
    {
        chaserSpline.speed = startSpeed;
    }

    void Update()
    {
        // Debug.Log(Gap); // CHANGED: commented out, it logged every frame
        if (snail == null || snail.IsDead) return;

        chaserSpline.speed += acceleration * Time.deltaTime;

        // Rubber band: don't let the player escape forever.
        if (Gap > maxGap)
        {
            // Debug.Log("max gap exceeded"); // CHANGED: commented out, it logged every frame
            if (snailSpline.progress < maxGap)
            {
                chaserSpline.progress = snailSpline.progress - maxGap;
            }
            else
            {
                chaserSpline.progress += 0.1f;
            }
        }

        if (Gap <= catchDistance)
        {
            Debug.Log("snail died at: " + Gap);
            snail.Die();
            chaserSpline.speed = 0f; // NEW: the chaser stops when it has caught the snail
        }
    }
}