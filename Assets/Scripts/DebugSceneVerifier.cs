using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSceneVerifier : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- DEBUG SCENE VERIFIER START ---");
        
        // 1. Verify Scene Loading
        Debug.Log($"Current Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"Target LevelSelect Scene Name: {SceneNames.LevelSelect}");
        
        // Check if scene exists in build settings
        bool levelSelectExists = false;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sceneName == SceneNames.LevelSelect) levelSelectExists = true;
        }
        
        if (levelSelectExists) Debug.Log("SUCCESS: LevelSelect scene found in Build Settings.");
        else Debug.LogError($"ERROR: '{SceneNames.LevelSelect}' scene NOT found in Build Settings! Please add it.");

        // 2. Verify Terrain Generation
        if (LevelManager.Instance != null)
        {
            LevelData data = LevelManager.Instance.GetLevelData(GameSession.SelectedLevel);
            if (data != null)
            {
                Debug.Log($"Current Level Data: {data.levelName}");
                Debug.Log($"Use Procedural Generation: {data.useProceduralGeneration}");
                
                if (data.useProceduralGeneration)
                {
                    var generator = FindFirstObjectByType<TerrainGenerator>();
                    if (generator != null) Debug.Log("SUCCESS: TerrainGenerator instance found in scene.");
                    else Debug.LogError("ERROR: TerrainGenerator instance MISSING! GameManager should have spawned it.");
                }
            }
            else
            {
                Debug.LogError("ERROR: LevelData is null!");
            }
        }
        else
        {
            Debug.LogWarning("WARNING: LevelManager instance not found (might be normal if in MainMenu).");
        }
        
        Debug.Log("--- DEBUG SCENE VERIFIER END ---");
    }
}
