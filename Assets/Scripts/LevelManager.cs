using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private List<LevelData> levels;
    
    [Header("Default Assets")]
    [SerializeField] private GameObject defaultCoinPrefab;
    [SerializeField] private GameObject defaultFuelPrefab;
    [SerializeField] private Material defaultTerrainMaterial; // For procedural terrain

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (levels == null || levels.Count == 0)
        {
            CreateDefaultLevels();
        }
    }

    public Material GetDefaultTerrainMaterial()
    {
        return defaultTerrainMaterial;
    }

    private void CreateDefaultLevels()
    {
        levels = new List<LevelData>();

        // Level 1: Easy
        LevelData l1 = ScriptableObject.CreateInstance<LevelData>();
        l1.levelName = "Green Hills";
        l1.levelIndex = 1;
        l1.description = "A nice drive through the hills.";
        l1.gravityScale = 1.0f;
        l1.friction = 1.0f;
        l1.fuelConsumptionRate = 0.05f;
        l1.distanceToFinish = 500;
        l1.useProceduralGeneration = true;
        l1.noiseScale = 0.03f;
        l1.noiseAmplitude = 3.0f;
        l1.segmentLength = 1.0f;
        l1.coinFrequency = 0.15f;
        l1.fuelFrequency = 0.03f;
        l1.coinPrefab = defaultCoinPrefab;
        l1.fuelPrefab = defaultFuelPrefab;
        levels.Add(l1);

        // Level 2: Medium
        LevelData l2 = ScriptableObject.CreateInstance<LevelData>();
        l2.levelName = "Desert Dunes";
        l2.levelIndex = 2;
        l2.description = "Hot and sandy. Watch your fuel!";
        l2.gravityScale = 1.2f;
        l2.friction = 0.8f;
        l2.fuelConsumptionRate = 0.08f;
        l2.distanceToFinish = 1000;
        l2.useProceduralGeneration = true;
        l2.noiseScale = 0.06f;
        l2.noiseAmplitude = 6.0f;
        l2.segmentLength = 1.0f;
        l2.coinFrequency = 0.1f;
        l2.fuelFrequency = 0.02f;
        l2.coinPrefab = defaultCoinPrefab;
        l2.fuelPrefab = defaultFuelPrefab;
        levels.Add(l2);

        // Level 3: Hard
        LevelData l3 = ScriptableObject.CreateInstance<LevelData>();
        l3.levelName = "Moon Base";
        l3.levelIndex = 3;
        l3.description = "Low gravity madness.";
        l3.gravityScale = 0.4f;
        l3.friction = 0.5f;
        l3.fuelConsumptionRate = 0.03f;
        l3.distanceToFinish = 1500;
        l3.useProceduralGeneration = true;
        l3.noiseScale = 0.04f;
        l3.noiseAmplitude = 10.0f;
        l3.segmentLength = 1.0f;
        l3.coinFrequency = 0.2f;
        l3.fuelFrequency = 0.015f;
        l3.coinPrefab = defaultCoinPrefab;
        l3.fuelPrefab = defaultFuelPrefab;
        levels.Add(l3);
    }

    public LevelData GetLevelData(int levelIndex)
    {
        // 1-based index to 0-based list
        int index = levelIndex - 1;
        if (index >= 0 && index < levels.Count)
        {
            return levels[index];
        }
        
        Debug.LogWarning($"Level {levelIndex} not found! Returning first level.");
        return levels.Count > 0 ? levels[0] : null;
    }

    public int GetTotalLevels()
    {
        return levels.Count;
    }

    public void UnlockLevel(int levelIndex)
    {
        int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (levelIndex > currentUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", levelIndex);
            PlayerPrefs.Save();
        }
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= PlayerPrefs.GetInt("UnlockedLevel", 1);
    }
}
