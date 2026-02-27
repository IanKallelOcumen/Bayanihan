using UnityEngine;

/// <summary>
/// Spawns background clouds at random intervals and positions relative to the camera.
/// </summary>
public class CloudSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("List of cloud prefabs to spawn randomly.")]
    public GameObject[] cloudPrefabs;
    [Tooltip("Time in seconds between spawns.")]
    public float spawnInterval = 1.5f; 
    [Tooltip("Horizontal distance ahead of the camera to spawn clouds.")]
    public float spawnDistanceX = 15f; 
    
    [Header("Height & Scale")]
    public float minHeightFromCam = 3f; 
    public float maxHeightFromCam = 8f;
    
    public float minScale = 3f; 
    public float maxScale = 6f; 

    private Transform cam;
    private float timer;

    void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
            PrewarmSky(); 
        }
        else
        {
            Debug.LogWarning("CloudSpawner: Main Camera not found!");
            enabled = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnCloud(spawnDistanceX);
            timer = 0;
        }
    }

    void PrewarmSky()
    {
        // Spawn a few clouds instantly
        for (int i = 0; i < 4; i++)
        {
            float randomX = Random.Range(-10f, 10f);
            SpawnCloud(randomX);
        }
    }

    void SpawnCloud(float xOffset)
    {
        // FIX: Calculate Y relative to Camera position, not World position
        float spawnY = cam.position.y + Random.Range(minHeightFromCam, maxHeightFromCam);
        
        Vector3 spawnPos = new Vector3(cam.position.x + xOffset, spawnY, 0);
        
        // FORCE ZERO ROTATION
        GameObject selectedCloud = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        GameObject newCloud = Instantiate(selectedCloud, spawnPos, Quaternion.identity);

        float randomScale = Random.Range(minScale, maxScale);
        newCloud.transform.localScale = new Vector3(randomScale, randomScale, 1f);

        CloudMover mover = newCloud.GetComponent<CloudMover>();
        SpriteRenderer sr = newCloud.GetComponent<SpriteRenderer>();

        if (mover != null && sr != null)
        {
            // Set speed slower
            float t = Mathf.InverseLerp(minScale, maxScale, randomScale);
            
            // Lowered Wind Speed (0.2 to 1.0)
            mover.windSpeed = Mathf.Lerp(0.2f, 1.0f, t);
            mover.parallaxEffect = Mathf.Lerp(0.95f, 0.5f, t);

            // Ensure they are behind everything
            sr.sortingOrder = -20; 
        }
    }
}