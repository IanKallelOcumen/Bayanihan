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
    // [SerializeField] private int totalSegmentsGenerated = 0; // Removed unused warning
    
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

    [Header("Procedural HeightMap")]
    [SerializeField] private TerrainData terrainData; // Unity TerrainData Reference
    private float[,] heightMap;

    void Awake()
    {
        // 8. Fix terrain generation: ensure that the Terrain component’s TerrainData is not null
        Terrain terrain = GetComponent<Terrain>();
        if (terrain != null)
        {
            if (terrain.terrainData == null)
            {
                Debug.LogError("TerrainData missing on Terrain component!");
            }
            else
            {
                terrainData = terrain.terrainData;
            }
        }
        
        // Procedural HeightMap Generation
        GenerateHeightMap();
    }

    private void GenerateHeightMap()
    {
        // Produces a new 513x513 height map
        int resolution = 513;
        heightMap = new float[resolution, resolution];
        
        float seed = Random.Range(0f, 1000f);
        
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                // Simple Perlin Noise for height map
                float xCoord = (float)x / resolution * 10f + seed;
                float yCoord = (float)y / resolution * 10f + seed;
                heightMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }

        if (terrainData != null)
        {
            terrainData.heightmapResolution = resolution;
            terrainData.SetHeights(0, 0, heightMap);
        }
    }

    public void SetPlayer(Transform player)
    {
        this.playerTransform = player;
    }

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        
        // If we are using Mesh-based terrain (2D), initialize it
        if (meshFilter != null)
        {
            mesh = new Mesh();
            mesh.MarkDynamic(); // Optimize for frequent updates
            meshFilter.mesh = mesh;
        }

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
            
            // If terrainMaterial is missing, try to find a default one
            if (terrainMaterial == null)
            {
                // Fallback to a default sprite material if no specific material is assigned
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) 
                {
                    terrainMaterial = new Material(shader);
                }
            }
            
            // Apply Ground Color from LevelData
            if (terrainMaterial != null)
            {
                terrainMaterial.color = currentLevelData.groundColor;
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
        
        // Multi-layer noise (Fractal Brownian Motion)
        int octaves = currentLevelData.octaves;
        float persistence = currentLevelData.persistence;
        float lacunarity = currentLevelData.lacunarity;

        for (int i = 0; i < segmentCount; i++)
        {
            float x = currentX;
            float y = 0;
            float amplitude = amp;
            float frequency = scale;
            
            // Fractal Noise Loop
            for (int o = 0; o < octaves; o++)
            {
                // Use different seed offset per octave to avoid symmetry
                float noiseVal = Mathf.PerlinNoise((x + seedX + (o * 1000f)) * frequency, 0);
                // Map 0..1 to -1..1 for more interesting terrain? Or keep 0..1 for ground.
                // Let's keep 0..1 but center it if needed. 
                // Standard Perlin is 0..1. 
                y += noiseVal * amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            // Create Segment Data
            TerrainSegment seg = new TerrainSegment
            {
                vTop = new Vector3(x, y, 0),
                vBottom = new Vector3(x, y - depth, 0),
                xPos = x
            };
            segments.Add(seg);

            // Spawn Features (Coins/Fuel/Obstacles)
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
