using UnityEngine;

// NEW SCRIPT
// Put this on a Quad that is a child of the camera, with your background image on its material.
// The Quad follows the camera, and the image inside it scrolls slowly as the camera moves,
// so it looks far away and repeats forever (the track is endless).
// For more depth, make several layers (sky, hills, bushes) with different Parallax Factors.
public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("Leave empty to use the Main Camera.")]
    public Transform cam;

    [Tooltip("How fast the image scrolls. 0 = stays still (very far away), 1 = moves with the track (very close).")]
    [Range(0f, 1f)] public float parallaxFactor = 0.1f;

    [Tooltip("Stretch the image's width so it keeps its own shape, no matter how wide the Quad is.")]
    public bool keepImageAspect = true;

    Renderer rend;
    float startX;
    float tilingX = 1f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (cam == null) cam = Camera.main.transform;
        startX = cam.position.x;

        Texture tex = rend.material.mainTexture;
        if (keepImageAspect && tex != null)
        {
            Vector3 size = transform.lossyScale;
            tilingX = (size.x / size.y) * (tex.height / (float)tex.width);
            rend.material.mainTextureScale = new Vector2(tilingX, 1f);
        }
    }

    void LateUpdate()
    {
        // How far the camera has moved, turned into "how many Quad widths", times the factor.
        float moved = cam.position.x - startX;
        float offset = moved * parallaxFactor / transform.lossyScale.x * tilingX;
        rend.material.mainTextureOffset = new Vector2(offset, 0f);
    }
}