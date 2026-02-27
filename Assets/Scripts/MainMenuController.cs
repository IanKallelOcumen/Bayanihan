using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// Main menu controller handling UI transitions and scene loading.
/// Consolidates logic from previous MenuManager and MainMenuController.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup settingsPanel;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float slideDistance = 1000f;

    private void Start()
    {
        // 1. Force Time to run (fixes frozen menus after quitting game)
        Time.timeScale = 1f;

        // 2. Initialize Panels
        InitializePanel(mainMenuPanel, true);
        InitializePanel(settingsPanel, false);
    }

    private void Update()
    {
        // Failsafe: Ensure time never stops in the menu
        if (Time.timeScale != 1f) Time.timeScale = 1f;
    }

    // --- BUTTON EVENTS (Linked in Inspector) ---

    public void OnStartGame()
    {
        // Load Level Select scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.LevelSelect);
    }

    public void OnSettings()
    {
        if (settingsPanel == null) 
        {
            Debug.LogWarning("Settings Panel not assigned!");
            return;
        }
        StartCoroutine(Transition(mainMenuPanel, settingsPanel, -1)); 
    }

    public void OnBackFromSettings()
    {
        if (mainMenuPanel == null) return;
        StartCoroutine(Transition(settingsPanel, mainMenuPanel, 1));
    }

    public void OnExitGame()
    {
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- LOGIC ---

    private void InitializePanel(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
        cg.gameObject.SetActive(visible); 
        
        RectTransform rt = cg.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;
    }

    private IEnumerator Transition(CanvasGroup outPanel, CanvasGroup inPanel, int direction)
    {
        // Setup Incoming Panel
        inPanel.gameObject.SetActive(true);
        inPanel.alpha = 0f; 
        
        RectTransform outRect = outPanel.GetComponent<RectTransform>();
        RectTransform inRect = inPanel.GetComponent<RectTransform>();

        if (outRect != null) outRect.anchoredPosition = Vector2.zero;
        if (inRect != null) inRect.anchoredPosition = new Vector2(slideDistance * direction, 0);
        
        outPanel.interactable = false;
        inPanel.interactable = false;

        float timer = 0f;

        while (timer < transitionDuration)
        {
            // Use UNSCALED time so animations work even if game logic is paused
            timer += Time.unscaledDeltaTime; 
            
            float t = timer / transitionDuration;
            float easeT = Mathf.SmoothStep(0, 1, t); 

            // Slide & Fade
            outPanel.alpha = Mathf.Lerp(1f, 0f, easeT);
            if (outRect != null) 
                outRect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-slideDistance * direction, 0), easeT);

            inPanel.alpha = Mathf.Lerp(0f, 1f, easeT);
            if (inRect != null)
                inRect.anchoredPosition = Vector2.Lerp(new Vector2(slideDistance * direction, 0), Vector2.zero, easeT);

            yield return null;
        }

        // Finalize
        InitializePanel(outPanel, false); 
        InitializePanel(inPanel, true);   
    }
}
