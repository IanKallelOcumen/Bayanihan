using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu: Start Game -> Level Select, Settings, Exit.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public void OnStartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.LevelSelect);
    }

    public void OnSettings()
    {
        // Load settings scene or open panel - add your Settings scene name
        // SceneManager.LoadScene("Settings");
    }

    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
