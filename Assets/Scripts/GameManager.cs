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
		uiController = FindFirstObjectByType<UIController>();
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
                      // Check if one already exists in scene
                      TerrainGenerator tg = FindFirstObjectByType<TerrainGenerator>();
                      if (tg == null)
                      {
                          GameObject terrainGen = new GameObject("ProceduralTerrainGenerator");
                          tg = terrainGen.AddComponent<TerrainGenerator>();
                      }
                      
                      // Configure based on biome
                      // (Assuming TerrainGenerator has public config methods or reads LevelData)
                      // For now, we just ensure it has the player reference
                      if (player != null) tg.SetPlayer(player);
                  }
             }
         }
        // --- LEVEL INTEGRATION END ---
	}

    // Public Getters for UI
    public int GetCurrentDistance() { return currentDistance; }
    public int GetNextLevelDistance() { return nextLevelDistance; }
    public int GetBestDistance() { return bestDistance; }
    public int GetCurrentLevel() { return currentLevel; }
    public bool IsFuel() { return fuelLevel > 0; }

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
        currentDistance = (int)(player.position.x - startPosition.x);
        if (currentDistance > bestDistance)
        {
            bestDistance = currentDistance;
            PlayerPrefs.SetInt("BestDistance", bestDistance);
        }

        if (currentDistance >= nextLevelDistance)
        {
            // Level Complete
            Debug.Log("Level Complete!");
            
            // Calculate Stars based on Fuel
            int stars = 1;
            if (fuelLevel > 0.5f) stars = 2;
            if (fuelLevel > 0.8f) stars = 3;
            
            GameSession.LastLevelScore = (int)(fuelLevel * 1000) + currentDistance;
            GameSession.LastLevelStars = stars;
            
            // Unlock Next Level
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.UnlockLevel(currentLevel + 1);
                LevelManager.Instance.SaveLevelStars(currentLevel, stars);
            }
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("Victory");
            enabled = false; // Stop updating
        }
        
        if (fuelLevel <= 0 && Mathf.Abs(player.GetComponent<Rigidbody2D>().linearVelocity.x) < 0.1f)
        {
            // Game Over (Out of Fuel and stopped)
             UnityEngine.SceneManagement.SceneManager.LoadScene("Main_Menu");
        }
    }

	public void Refuel()
	{
		fuelLevel = 1f;
	}

	public float GetFuelLevel()
	{
		return fuelLevel;
	}

	public void AddCoins(int coinsToAdd)
	{
		var coins = PlayerPrefs.GetInt("coins", 0) + coinsToAdd;
		PlayerPrefs.SetInt("coins", coins);
		if (uiController != null)
			uiController.UpdateCoins(coins.ToString());
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
