using UnityEngine;
using UnityEngine.Splines;

public class SplineMovement : MonoBehaviour
{
    public SplineContainer activeSpline;
    public float speed = 1f;
    public float progress;
    public float splineLength;
    public float distance; // NEW: total distance travelled, used by GameHUD for the score

    private Vector3 currentPosition;
    private Vector3 nextPosition;
    private Vector3 direction;
    private Vector3 upwards;
    void Start()
    {
        // splineLength = activeSpline.CalculateLength();
    }

    void Update()
    {
        progress += speed * Time.deltaTime / splineLength;
        distance += speed * Time.deltaTime; // NEW: count total distance

        // CHANGED: Mathf.Clamp01(progress) instead of progress. A negative progress (the chaser starting
        // behind the track) made the spline continue backwards into the ground. Now it waits at the start.
        currentPosition = activeSpline.EvaluatePosition(Mathf.Clamp01(progress));
        transform.position = currentPosition;

        // CHANGED: same clamp here
        nextPosition = activeSpline.EvaluatePosition(Mathf.Clamp01(progress + 0.05f));
        direction = nextPosition - currentPosition;
        if (direction.x >= 0)
        {
            upwards = Vector3.up;
        }
        else
        {
            upwards = -Vector3.up;
        }
        // CHANGED: only rotate when direction isn't zero (at the very end of a piece it can be,
        // which spams "Look rotation viewing vector is zero" in the Console)
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction, upwards);
        }
    }
}