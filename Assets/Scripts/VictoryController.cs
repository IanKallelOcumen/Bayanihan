using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryController : MonoBehaviour
{
    // Exposed for PitstopSetup.cs (even if used only for display)
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        if (scoreText != null)
        {
            int lastScore = PlayerPrefs.GetInt("LastScore", 0);
            scoreText.text = $"Final Score: {lastScore}";
        }
    }

    public void OnNextLevel()
    {
        // Increment level
        GameSession.SelectedLevel++;
        
        // Check if we exceeded max levels
        if (LevelManager.Instance != null && GameSession.SelectedLevel > LevelManager.Instance.GetTotalLevels())
        {
            // Loop back or go to menu? Let's go to menu for now as "Game Complete"
            Debug.Log("All Levels Complete!");
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
        else
        {
            // Load the next level (Game Scene reloads with new level data)
            SceneManager.LoadScene(SceneNames.Game);
        }
    }

    public void OnBackToMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
