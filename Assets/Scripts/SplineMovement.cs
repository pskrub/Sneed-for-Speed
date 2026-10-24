using UnityEngine;
using UnityEngine.Splines;

public class SplineMovement : MonoBehaviour
{
    public SplineContainer activeSpline;
    public float speed = 1f;
    public float progress;
    public float splineLength;
    
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
        
        currentPosition = activeSpline.EvaluatePosition(progress);
        transform.position = currentPosition;

        nextPosition = activeSpline.EvaluatePosition(progress + 0.05f);
        direction = nextPosition - currentPosition;
        if (direction.x >= 0)
        {
            upwards = Vector3.up;
        }
        else
        {
            upwards = -Vector3.up;
        }
        transform.rotation = Quaternion.LookRotation(direction, upwards);
    }
}
