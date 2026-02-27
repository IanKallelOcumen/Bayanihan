using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Material terrainMaterial; // Assign a material (e.g., Sprites-Default)

    [Header("Debug / Status")]
    [SerializeField] private float currentX = 0f;
    [SerializeField] private int totalSegmentsGenerated = 0;
    
    private Mesh mesh;
    private MeshFilter meshFilter;
    private EdgeCollider2D edgeCollider;
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector2> colliderPoints = new List<Vector2>();

    // Current Level Settings
    private LevelData currentLevelData;
    private float seedX; // Random offset for noise

    // Optimization: Chunking and Memory Management
    private const int CHUNK_SIZE = 50; // Generate this many segments at a time
    private float generationThreshold = 50f; // Generate more when player is this close to end
    private float destroyThreshold = 100f; // Remove segments this far behind player

    // Store raw data points to reconstruct mesh efficiently
    private struct TerrainSegment
    {
        public Vector3 vTop;
        public Vector3 vBottom;
        public float xPos;
    }
    private List<TerrainSegment> segments = new List<TerrainSegment>();

    public void SetPlayer(Transform player)
    {
        this.playerTransform = player;
    }

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        
        mesh = new Mesh();
        mesh.MarkDynamic(); // Optimize for frequent updates
        meshFilter.mesh = mesh;

        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        // Initialize with current level data
        if (LevelManager.Instance != null)
        {
            currentLevelData = LevelManager.Instance.GetLevelData(GameSession.SelectedLevel);
            if (terrainMaterial == null)
            {
                terrainMaterial = LevelManager.Instance.GetDefaultTerrainMaterial();
                // If still null, try to load a default
                if (terrainMaterial == null)
                {
                    // Fallback to a default sprite material
                    Shader shader = Shader.Find("Sprites/Default");
                    if (shader != null) terrainMaterial = new Material(shader);
                }
            }
            if (meshFilter.GetComponent<MeshRenderer>().sharedMaterial == null)
            {
                meshFilter.GetComponent<MeshRenderer>().sharedMaterial = terrainMaterial;
            }
        }

        if (currentLevelData == null)
        {
            Debug.LogWarning("No LevelData found! Using defaults.");
            currentLevelData = ScriptableObject.CreateInstance<LevelData>();
            currentLevelData.useProceduralGeneration = true;
        }

        seedX = Random.Range(0f, 1000f);

        // Generate Initial Terrain
        if (currentLevelData.useProceduralGeneration)
        {
            GenerateChunk(currentLevelData.initialSegments);
        }
    }

    void Update()
    {
        if (playerTransform == null || currentLevelData == null || !currentLevelData.useProceduralGeneration) return;

        // Check distance to end of generated terrain
        if (playerTransform.position.x + generationThreshold > currentX)
        {
            GenerateChunk(CHUNK_SIZE);
            CleanupOldSegments();
        }
    }

    void GenerateChunk(int segmentCount)
    {
        float scale = currentLevelData.noiseScale;
        float amp = currentLevelData.noiseAmplitude;
        float step = currentLevelData.segmentLength;
        float depth = currentLevelData.groundDepth;

        for (int i = 0; i < segmentCount; i++)
        {
            float x = currentX;
            float y = Mathf.PerlinNoise((x + seedX) * scale, 0) * amp;

            // Create Segment Data
            TerrainSegment seg = new TerrainSegment
            {
                vTop = new Vector3(x, y, 0),
                vBottom = new Vector3(x, y - depth, 0),
                xPos = x
            };
            segments.Add(seg);

            // Spawn Features (Coins/Fuel)
            SpawnFeatures(x, y, currentLevelData);

            currentX += step;
        }

        UpdateMesh();
    }

    void CleanupOldSegments()
    {
        // Remove segments that are far behind the player
        float thresholdX = playerTransform.position.x - destroyThreshold;
        
        // RemoveAll is efficient enough for this list size
        int removedCount = segments.RemoveAll(s => s.xPos < thresholdX);
        
        if (removedCount > 0)
        {
            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        if (segments.Count < 2) return;

        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colliderPoints.Clear();

        float depth = currentLevelData.groundDepth;

        for (int i = 0; i < segments.Count; i++)
        {
            TerrainSegment seg = segments[i];

            // Add Vertices
            vertices.Add(seg.vTop);
            vertices.Add(seg.vBottom);

            // Add UVs
            uvs.Add(new Vector2(seg.vTop.x * 0.5f, seg.vTop.y * 0.5f));
            uvs.Add(new Vector2(seg.vBottom.x * 0.5f, seg.vBottom.y * 0.5f));

            // Add Collider Point (Top only)
            colliderPoints.Add(new Vector2(seg.vTop.x, seg.vTop.y));

            // Add Triangles (if not the last segment)
            if (i < segments.Count - 1)
            {
                int vIndex = i * 2;
                
                // Triangle 1
                triangles.Add(vIndex);     // Top Left
                triangles.Add(vIndex + 1); // Bottom Left
                triangles.Add(vIndex + 2); // Top Right

                // Triangle 2
                triangles.Add(vIndex + 1); // Bottom Left
                triangles.Add(vIndex + 3); // Bottom Right
                triangles.Add(vIndex + 2); // Top Right
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        edgeCollider.points = colliderPoints.ToArray();
    }
    
    void SpawnFeatures(float x, float y, LevelData data)
    {
        // Simple random chance
        if (data.coinPrefab != null && Random.value < data.coinFrequency)
        {
            Vector3 pos = new Vector3(x, y + 1.5f, 0);
            Instantiate(data.coinPrefab, pos, Quaternion.identity);
        }
        else if (data.fuelPrefab != null && Random.value < data.fuelFrequency)
        {
            // Ensure fuel isn't too frequent or too rare?
            // For now simple random
            Vector3 pos = new Vector3(x, y + 1.0f, 0);
            Instantiate(data.fuelPrefab, pos, Quaternion.identity);
        }
    }
}
