using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] private string garageSceneName = "Garage";
    [SerializeField] private string mainMenuSceneName = "Main_Menu";

    public void LoadGarageLevel(int level)
    {
        GameSession.SelectedLevel = level;
        
        // Try to load Level Data if available
        if (LevelManager.Instance != null)
        {
            LevelData data = LevelManager.Instance.GetLevelData(level);
            if (data != null && !string.IsNullOrEmpty(data.sceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(data.sceneName);
                return;
            }
        }
        
        // Fallback: Construct name (Level_01, Level_02...)
        string sceneName = $"Level_{level:00}";
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"Scene {sceneName} not found, loading default {garageSceneName}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(garageSceneName);
        }
    }

    public void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
