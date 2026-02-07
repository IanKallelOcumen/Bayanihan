using UnityEngine;

public class CloudMover : MonoBehaviour
{
    private Transform cam;
    private Vector3 lastCamPos;
    
    public float parallaxEffect; 
    public float windSpeed = 0.5f; // DEFAULT IS NOW SLOW
    public float destroyDistance = 30f; 

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        float deltaX = cam.position.x - lastCamPos.x;
        
        // Parallax Movement
        float parallaxMove = deltaX * parallaxEffect;

        // Wind Movement (Slow constant drift)
        float windMove = -windSpeed * Time.deltaTime;

        transform.position += new Vector3(parallaxMove + windMove, 0, 0);

        lastCamPos = cam.position;

        // Cleanup based on Camera X
        if (transform.position.x < cam.position.x - destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}