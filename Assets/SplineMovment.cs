using UnityEngine;
using UnityEngine.Splines;

public class SplineMovment : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 1f;
    private float progress;
    private float splineLength;
    
    private Vector3 currentPosition;
    private Vector3 nextPosition;
    private Vector3 direction;
    void Start()
    {
        splineLength = spline.CalculateLength();
    }

    void Update()
    {
        progress += speed * Time.deltaTime / splineLength;
        
        currentPosition = spline.EvaluatePosition(progress);
        transform.position = currentPosition;

        if (progress > 1f)
        {
            progress = 0f;
        }
        
        nextPosition = spline.EvaluatePosition(progress + 0.05f);
        direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
