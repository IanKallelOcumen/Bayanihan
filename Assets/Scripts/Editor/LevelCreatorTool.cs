#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class LevelCreatorTool : EditorWindow
{
    [MenuItem("Tools/Bayanihan/Create Default Levels")]
    public static void CreateLevels()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data/Levels"))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Levels");
        }

        CreateLevel("Level_01", 1, "The Start", LevelData.BiomeType.Plains, 1000, "GameScene", new Color(0.3f, 0.8f, 0.3f), 5.0f);
        CreateLevel("Level_02", 2, "The Desert", LevelData.BiomeType.Desert, 2000, "DesertScene", new Color(0.94f, 0.80f, 0.40f), 1.0f);
        CreateLevel("Level_03", 3, "The Mountains", LevelData.BiomeType.Mountain, 3000, "Level_03", new Color(0.4f, 0.4f, 0.45f), 12.0f);

        AssetDatabase.SaveAssets();
        Debug.Log("Created Levels 01, 02, 03.");
        
        // Auto-wire to LevelManager
        LevelManager lm = FindFirstObjectByType<LevelManager>();
        if (lm != null)
        {
             // We can re-use the SetupLevelSystem method from MasterSetupTool if accessible, 
             // but let's just assume the user will click "Setup Level System" in MasterSetupTool as per workflow.
             Debug.Log("Please run 'Master Setup > Setup Level System' to link these new levels.");
        }
    }

    private static void CreateLevel(string name, int index, string desc, LevelData.BiomeType biome, int distance, string sceneName, Color groundColor, float noiseAmp)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.levelName = name;
        level.sceneName = sceneName; // Explicitly set scene name
        level.levelIndex = index;
        level.description = desc;
        level.biomeType = biome;
        level.distanceToFinish = distance;
        level.useProceduralGeneration = true;
        level.groundColor = groundColor;
        level.noiseAmplitude = noiseAmp;
        
        string path = $"Assets/Data/Levels/{name}.asset";
        AssetDatabase.CreateAsset(level, path);
    }
}
#endif
