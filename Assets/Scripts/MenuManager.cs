using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] public CanvasGroup mainMenuPanel;
    [SerializeField] public CanvasGroup settingsPanel;

    [Header("Settings UI")]
    [SerializeField] public Slider musicSlider;
    [SerializeField] public Slider sfxSlider;

    [Header("Configuration")]
    [SerializeField] public string gameSceneName = "LevelSelect";

    private void Start()
    {
        ShowMainMenu();
        
        // Initialize Volume if sliders exist
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.5f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void OnPlayPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsPressed()
    {
        ShowSettings();
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OnBackPressed()
    {
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        TogglePanel(mainMenuPanel, true);
        TogglePanel(settingsPanel, false);
    }

    private void ShowSettings()
    {
        TogglePanel(mainMenuPanel, false);
        TogglePanel(settingsPanel, true);
    }

    private void TogglePanel(CanvasGroup cg, bool show)
    {
        if (cg == null) return;
        cg.alpha = show ? 1 : 0;
        cg.blocksRaycasts = show;
        cg.interactable = show;
        cg.gameObject.SetActive(show); // Ensure visuals are off
    }

    public void SetMusicVolume(float val)
    {
        PlayerPrefs.SetFloat("MusicVol", val);
        AudioListener.volume = val; // Simple global volume for now
    }

    public void SetSFXVolume(float val)
    {
        PlayerPrefs.SetFloat("SFXVol", val);
    }
}
