using UnityEngine;

public class GameManager : MonoBehaviour
{
	UIController uiController;

	[Header("Fuel System")]
	[SerializeField] float fuelUsage = 0.05f;
	float fuelLevel = 1f;

	[Header("Progression System")]
	[SerializeField] Transform player;
	[SerializeField] float levelDistanceIncrement = 100f;
	
	private int currentDistance;
	private int currentLevel = 1;
	private int nextLevelDistance = 100;
	private int bestDistance;
	private Vector3 startPosition;

	void Start()
	{
		Time.timeScale = 0.9f;
		uiController = GameObject.Find("UI").GetComponent<UIController>();
		uiController.UpdateCoins(PlayerPrefs.GetInt("coins").ToString());
		
		// Load best distance
		bestDistance = PlayerPrefs.GetInt("BestDistance", 0);
		
		// Store starting position for distance calculation
		if (player != null)
		{
			startPosition = player.position;
		}
	}

	void Update()
	{
		UseFuel();
		UpdateProgression();
	}

	void UseFuel()
	{
		fuelLevel -= fuelUsage * Time.deltaTime;
		uiController.UpdateFuelLevel(fuelLevel);
	}

	void UpdateProgression()
	{
		if (player == null) return;
		
		// Calculate current distance
		currentDistance = Mathf.RoundToInt(player.position.x - startPosition.x);
		
		// Check for level progression
		if (currentDistance >= nextLevelDistance)
		{
			currentLevel++;
			nextLevelDistance = currentLevel * Mathf.RoundToInt(levelDistanceIncrement);
			OnLevelUp();
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
		var coins = PlayerPrefs.GetInt("coins") + coinsToAdd;
		PlayerPrefs.SetInt("coins", coins);
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
