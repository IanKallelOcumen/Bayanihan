using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public CanvasGroup mainMenuPanel;
    public CanvasGroup settingsPanel;

    [Header("Settings")]
    public float duration = 0.5f;
    public float slideDistance = 1000f;
    // This string is correct, provided you use the fix in OnPlayPressed below
    public string gameSceneName = "GameScene"; 

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

    // --- BUTTON EVENTS ---

    public void OnPlayPressed()
    {
        // FIX: We explicitly type "UnityEngine.SceneManagement.SceneManager"
        // This bypasses the error if you accidentally named another script "SceneManager"
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsPressed()
    {
        if (settingsPanel == null) return;
        StartCoroutine(Transition(mainMenuPanel, settingsPanel, -1)); 
    }

    public void OnBackPresssed()
    {
        if (mainMenuPanel == null) return;
        StartCoroutine(Transition(settingsPanel, mainMenuPanel, 1));
    }

    public void OnQuitPressed()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
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

        outRect.anchoredPosition = Vector2.zero;
        inRect.anchoredPosition = new Vector2(slideDistance * direction, 0);
        
        outPanel.interactable = false;
        inPanel.interactable = false;

        float timer = 0f;

        while (timer < duration)
        {
            // Use UNSCALED time so animations work even if game logic is paused
            timer += Time.unscaledDeltaTime; 
            
            float t = timer / duration;
            float easeT = Mathf.SmoothStep(0, 1, t); 

            // Slide & Fade
            outPanel.alpha = Mathf.Lerp(1f, 0f, easeT);
            outRect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-slideDistance * direction, 0), easeT);

            inPanel.alpha = Mathf.Lerp(0f, 1f, easeT);
            inRect.anchoredPosition = Vector2.Lerp(new Vector2(slideDistance * direction, 0), Vector2.zero, easeT);

            yield return null;
        }

        // Finalize
        InitializePanel(outPanel, false); 
        InitializePanel(inPanel, true);   
    }
}