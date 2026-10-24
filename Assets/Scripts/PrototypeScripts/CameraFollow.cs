using UnityEngine;

// Side-view camera that follows the snail along the X axis.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    [Tooltip("Positive X keeps the snail left of center so you can see the road ahead.")]
    public float xOffset = 2f;
    public float smoothing = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 p = transform.position;
        float wantedX = target.position.x + xOffset;
        p.x = Mathf.Lerp(p.x, wantedX, smoothing * Time.deltaTime);
        transform.position = p;
    }
}
