using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages level progression, unlocking, and data persistence.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private List<LevelData> allLevels;
    [SerializeField] private int defaultUnlockedLevels = 1;

    private const string PREF_UNLOCKED_LEVEL = "LevelUnlocked_";
    private const string PREF_LEVEL_STARS = "LevelStars_";
    private const string PREF_LEVEL_SCORE = "LevelScore_";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Ensure levels are sorted by index
        if (allLevels != null)
        {
            allLevels = allLevels.OrderBy(l => l.levelIndex).ToList();
        }
    }

    /// <summary>
    /// Gets the total number of levels configured.
    /// </summary>
    public int GetTotalLevels()
    {
        return allLevels != null ? allLevels.Count : 0;
    }

    /// <summary>
    /// Retrieves LevelData for a specific level index (1-based).
    /// </summary>
    public LevelData GetLevelData(int levelIndex)
    {
        if (allLevels == null || levelIndex < 1 || levelIndex > allLevels.Count)
            return null;
        
        return allLevels.Find(l => l.levelIndex == levelIndex);
    }

    /// <summary>
    /// Checks if a level is unlocked. Level 1 is always unlocked.
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex <= defaultUnlockedLevels) return true;
        return PlayerPrefs.GetInt(PREF_UNLOCKED_LEVEL + levelIndex, 0) == 1;
    }

    /// <summary>
    /// Unlocks a level.
    /// </summary>
    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex > allLevels.Count) return; // Don't unlock non-existent levels
        
        PlayerPrefs.SetInt(PREF_UNLOCKED_LEVEL + levelIndex, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Saves the stars earned for a level if it's a new high.
    /// </summary>
    public void SaveLevelStars(int levelIndex, int stars)
    {
        int currentStars = GetLevelStars(levelIndex);
        if (stars > currentStars)
        {
            PlayerPrefs.SetInt(PREF_LEVEL_STARS + levelIndex, stars);
            PlayerPrefs.Save();
        }
    }

    public int GetLevelStars(int levelIndex)
    {
        return PlayerPrefs.GetInt(PREF_LEVEL_STARS + levelIndex, 0);
    }
    
    public void SaveLevelScore(int levelIndex, int score)
    {
        int currentScore = GetLevelScore(levelIndex);
        if (score > currentScore)
        {
            PlayerPrefs.SetInt(PREF_LEVEL_SCORE + levelIndex, score);
            PlayerPrefs.Save();
        }
    }
    
    public int GetLevelScore(int levelIndex)
    {
        return PlayerPrefs.GetInt(PREF_LEVEL_SCORE + levelIndex, 0);
    }

    /// <summary>
    /// Resets all progress (Debug only).
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Progress Reset");
    }
}
