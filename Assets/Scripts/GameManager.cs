using UnityEngine;

/// <summary>
/// Core game manager handling game state, fuel, progression, and level management integration.
/// Implements Singleton pattern.
/// </summary>
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[SerializeField] private UIController uiController;

	[Header("Fuel System")]
	[SerializeField] private float fuelUsage = 0.05f;
	private float fuelLevel = 1f;

	[Header("Progression System")]
	[SerializeField] private Transform player;
	[SerializeField] private float levelDistanceIncrement = 100f;
	
	private int currentDistance;
	private int currentLevel = 1;
	private int nextLevelDistance = 100;
	private int bestDistance;
	private Vector3 startPosition;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		// Removed DontDestroyOnLoad to ensure fresh state per level load
		// DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		Time.timeScale = 1.0f; // Ensure time is running
		uiController = FindObjectOfType<UIController>();
		if (uiController != null)
			uiController.UpdateCoins(PlayerPrefs.GetInt("coins", 0).ToString());
		
		bestDistance = PlayerPrefs.GetInt("BestDistance", 0);
		
		if (player != null)
			startPosition = player.position;

        // --- LEVEL INTEGRATION START ---
        currentLevel = GameSession.SelectedLevel;
        if (LevelManager.Instance != null)
        {
            LevelData data = LevelManager.Instance.GetLevelData(currentLevel);
            if (data != null)
            {
                fuelUsage = data.fuelConsumptionRate;
                nextLevelDistance = data.distanceToFinish; // Use this as the goal
                
                // Apply Gravity if player is present
                 if (player != null)
                 {
                     Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                     if (rb != null) rb.gravityScale = data.gravityScale;
                     
                     PlayerController pc = player.GetComponent<PlayerController>();
                     if (pc != null) pc.ApplyPhysics(data.friction);
                 }

                 // Spawn Procedural Terrain Generator if needed
                  if (data.useProceduralGeneration)
                  {
                      GameObject terrainGen = new GameObject("ProceduralTerrainGenerator");
                      TerrainGenerator tg = terrainGen.AddComponent<TerrainGenerator>();
                      if (player != null) tg.SetPlayer(player);
                  }
             }
         }
        // --- LEVEL INTEGRATION END ---
	}

	void OnDestroy()
	{
		if (Instance == this) Instance = null;
	}

	void Update()
	{
		UseFuel();
		UpdateProgression();
	}

	void UseFuel()
	{
		fuelLevel -= fuelUsage * Time.deltaTime;
		if (uiController != null)
			uiController.UpdateFuelLevel(fuelLevel);
	}

	void UpdateProgression()
	{
		if (player == null) return;
		
		// Calculate current distance
		currentDistance = Mathf.RoundToInt(player.position.x - startPosition.x);
		
		// Check for level progression (Level Complete)
		if (currentDistance >= nextLevelDistance)
		{
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.UnlockLevel(currentLevel + 1);
                Debug.Log("Level " + currentLevel + " Complete! Unlocked Level " + (currentLevel + 1));
                
                // Show Victory or Load Next Level
                // For now, let's load the Victory scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Victory);
            }
            else
            {
                // Fallback to old infinite behavior if no LevelManager
			    currentLevel++;
			    nextLevelDistance = currentLevel * Mathf.RoundToInt(levelDistanceIncrement);
			    OnLevelUp();
            }
		}
		
		// Update best distance
		if (currentDistance > bestDistance)
		{
			bestDistance = currentDistance;
			PlayerPrefs.SetInt("BestDistance", bestDistance);
		}
	}

	void OnLevelUp()
	{
		// Optional: Add bonus coins or fuel on level up
		AddCoins(50 * currentLevel);
		Debug.Log("Level Up! Now at level " + currentLevel);
	}

	public void Refuel()
	{
		fuelLevel = 1f;
	}

	public float GetFuelLevel()
	{
		return fuelLevel;
	}

	public bool IsFuel()
	{
		return fuelLevel > 0 ? true : false;
	}

	public void AddCoins(int coinsToAdd)
	{
		var coins = PlayerPrefs.GetInt("coins", 0) + coinsToAdd;
		PlayerPrefs.SetInt("coins", coins);
		if (uiController != null)
			uiController.UpdateCoins(coins.ToString());
	}

	public int GetCurrentDistance()
	{
		return currentDistance;
	}

	public int GetCurrentLevel()
	{
		return currentLevel;
	}

	public int GetBestDistance()
	{
		return bestDistance;
	}

	public int GetNextLevelDistance()
	{
		return nextLevelDistance;
	}

	public void Pause()
	{
		Time.timeScale = 0f;
	}

	public void Resume()
	{
		Time.timeScale = 0.9f;
	}

	public void Exit()
	{
		Application.Quit();
	}
}
