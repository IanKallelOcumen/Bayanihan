using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    public int levelIndex;
    [TextArea] public string description;

    [Header("Terrain Generation")]
    public GameObject terrainPrefab; // Optional: If null, use procedural generation
    
    [Header("Procedural Generation Settings")]
    public bool useProceduralGeneration = true;
    public float noiseScale = 0.05f;      // Smoothness (Lower is smoother)
    public float noiseAmplitude = 5.0f;   // Height (Higher is steeper)
    public float groundDepth = 10.0f;     // How deep the ground goes below the curve
    public float segmentLength = 1.0f;    // Distance between points
    public int initialSegments = 100;     // How much to generate at start
    
    [Header("Feature Generation")]
    public float coinFrequency = 0.1f;    // Chance per segment
    public float fuelFrequency = 0.02f;   // Chance per segment
    public GameObject coinPrefab;
    public GameObject fuelPrefab;

    [Header("Physics Settings")]
    public float gravityScale = 1.0f;
    public float friction = 1.0f;
    
    [Header("Difficulty")]
    public float fuelConsumptionRate = 0.05f;
    public int distanceToFinish = 1000;
}
