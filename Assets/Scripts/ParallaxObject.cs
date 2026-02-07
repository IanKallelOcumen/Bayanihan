using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    private Transform cam;
    private Vector3 lastCamPos;

    // 1.0 = Moves exactly with camera (Static background / Far away)
    // 0.0 = Doesn't move with camera (Foreground / Close)
    public float parallaxFactor; 
    
    public float destroyDistance = 30f; // Delete when far behind

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        // 1. Calculate camera movement
        float deltaX = cam.position.x - lastCamPos.x;

        // 2. Move this object by a percentage of that movement
        transform.position += new Vector3(deltaX * parallaxFactor, 0, 0);

        lastCamPos = cam.position;

        // 3. Cleanup
        if (transform.position.x < cam.position.x - destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}