using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Fuel")]
    public TMP_Text fuelText; // Replaced Slider with Text for now as per setup

    [Header("Coins")]
    public TMP_Text coinsText;
    
    [Header("Level & Distance")]
    public TMP_Text distanceText;
    public TMP_Text levelText;

    // [Header("Pedals")] - Handled by TouchInput or simple buttons now
    // [SerializeField] GameObject brakeNormalImage;
    // [SerializeField] GameObject brakePressedImage;
    // [SerializeField] GameObject gasNormalImage;
    // [SerializeField] GameObject gasPressedImage;
    
	private GameManager gameManager;

	void Start()
	{
		gameManager = GameManager.Instance;
	}

	void Update()
	{
        // float brakeInput = TouchInput.GetRawBrakeInput();
        // float gasInput = TouchInput.GetRawGasInput();

        // if (brakeInput > 0) DisplayBrakePressed(true);
        // else DisplayBrakePressed(false);
        // if (gasInput > 0) DisplayGasPressed(true);
        // else DisplayGasPressed(false);
        
        // Update distance and level displays
        UpdateProgressionUI();
	}
	
	void UpdateProgressionUI()
	{
		if (gameManager == null) return;
        if (gameManager.GetNextLevelDistance() == 0) return; // Safety check
		
		// Update distance display
		if (distanceText != null)
		{
			int currentDist = gameManager.GetCurrentDistance();
			int nextLevel = gameManager.GetNextLevelDistance();
			// int best = gameManager.GetBestDistance();
			distanceText.text = $"{currentDist}m / {nextLevel}m";
		}
		
		// Update level display
		if (levelText != null)
		{
			levelText.text = $"Barangay {gameManager.GetCurrentLevel()}";
		}
	}

    public void UpdateFuelLevel(float newLevel)
	{
        if (fuelText != null)
            fuelText.text = $"Fuel: {(int)(newLevel * 100)}%";
	}

    public void UpdateCoins(string newScore)
	{
        if (coinsText != null)
            coinsText.text = newScore;
	}

	/*
	public void DisplayBrakePressed(bool isPressed)
	{
        if (brakeNormalImage) brakeNormalImage.SetActive(!isPressed);
        if (brakePressedImage) brakePressedImage.SetActive(isPressed);
	}

    public void DisplayGasPressed(bool isPressed)
	{
        if (gasNormalImage) gasNormalImage.SetActive(!isPressed);
        if (gasPressedImage) gasPressedImage.SetActive(isPressed);
    }
    */
}
