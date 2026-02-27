using UnityEngine;

public class SeamlessSpawner : MonoBehaviour
{
    public GameObject layerPrefab; // The specific image prefab (3, 5, or 8)
    
    // IMPORTANT: The exact width of your sprite in Unity units!
    // You can see this by dragging the sprite into the scene and checking its size.
    public float spriteWidth = 20f; 
    
    public float parallaxFactor = 0.5f; // Must match the prefab's setting!
    public int sortingOrder = -10;
    public float spawnY = -2f; // Height adjustment

    private Transform cam;
    private GameObject lastSpawnedObject;

    void Start()
    {
        cam = Camera.main.transform;

        // --- LEVEL INTEGRATION START ---
        if (LevelManager.Instance != null)
        {
            LevelData data = LevelManager.Instance.GetLevelData(GameSession.SelectedLevel);
            if (data != null)
            {
                // If procedural generation is enabled AND this spawner looks like the ground (parallax ~ 1.0), disable it.
                if (data.useProceduralGeneration && Mathf.Abs(parallaxFactor - 1.0f) < 0.1f)
                {
                    gameObject.SetActive(false);
                    return; // Stop execution
                }

                if (data.terrainPrefab != null) layerPrefab = data.terrainPrefab;
                parallaxFactor = data.parallaxFactor;
                spriteWidth = data.spriteWidth;
            }
        }
        // --- LEVEL INTEGRATION END ---
        
        // Spawn the first two pieces immediately so there's no gap at start
        SpawnPiece(cam.position.x - 5f); 
        SpawnPiece(lastSpawnedObject.transform.position.x + spriteWidth); 
    }

    void Update()
    {
        // Check if the camera is approaching the end of the last piece
        // Since the object is moving (parallax), we check its dynamic position
        if (cam.position.x > lastSpawnedObject.transform.position.x - (spriteWidth / 2))
        {
            // Spawn a new piece exactly at the end of the last one
            SpawnPiece(lastSpawnedObject.transform.position.x + spriteWidth);
        }
    }

    void SpawnPiece(float xPos)
    {
        Vector3 spawnPos = new Vector3(xPos, spawnY, 0);
        GameObject newObj = Instantiate(layerPrefab, spawnPos, Quaternion.identity);

        // 1. Configure the Mover
        ParallaxObject mover = newObj.GetComponent<ParallaxObject>();
        mover.parallaxFactor = parallaxFactor; // Pass the setting from here

        // 2. Configure the Visuals
        SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;

        // 3. Keep track of it
        lastSpawnedObject = newObj;
    }
}