using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] private string garageSceneName = "Garage";
    [SerializeField] private string mainMenuSceneName = "Main_Menu";

    public void LoadGarageLevel(int level)
    {
        GameSession.SelectedLevel = level;
        // In the new system, we might want to load a "Game" scene directly if Garage isn't the gameplay scene
        // But assuming Garage is where the game starts or vehicle is selected
        SceneManager.LoadScene(garageSceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
