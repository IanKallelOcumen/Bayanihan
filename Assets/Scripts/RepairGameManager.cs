using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using System.Collections.Generic;
using System.Collections;

public class RepairGameManager : MonoBehaviour
{
    public static RepairGameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private string victorySceneName = "Victory";
    [SerializeField] private float timeLimit = 120f;
    [SerializeField] private bool countDown = true;
    [SerializeField] private int scorePerPart = 100;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text partsLeftText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text instructionText; 
    [SerializeField] private UnityEngine.UI.Image carImage; 

    [Header("Vehicle Config")]
    [SerializeField] private GameObject carPrefab; // Used if we spawn
    [SerializeField] private GameObject scooterPrefab; // Used if we spawn
    [SerializeField] private Transform spawnPoint; // Where to put the vehicle

    [Header("Debug Info")]
    [SerializeField] private int brokenPartsCount;
    [SerializeField] private float timer;
    [SerializeField] private int score;
    [SerializeField] private int vehiclesFixed;
    [SerializeField] private int totalVehiclesToFix;
    [SerializeField] private RepairTarget[] repairTargets; 

    private bool isGameActive;
    private VehicleController currentVehicle;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        timer = 0f;
        score = 0;
        vehiclesFixed = 0;
        isGameActive = true;

        if (countDown) timer = timeLimit;

        // Set Vehicle Count based on Level
        int level = GameSession.SelectedLevel;
        if (level == 1) totalVehiclesToFix = 2; // "Multiple"
        else if (level == 2) totalVehiclesToFix = 3;
        else totalVehiclesToFix = Random.Range(4, 7); // 4-6

        SpawnNextVehicle();
        
        Debug.Log($"Game Started. Level {level}, Vehicles to fix: {totalVehiclesToFix}");
    }

    private void SpawnNextVehicle()
    {
        if (currentVehicle != null) Destroy(currentVehicle.gameObject);

        // 1. Determine Vehicle Type based on GameSession
        int level = GameSession.SelectedLevel;
        GameObject prefabToSpawn = carPrefab; // Default
        
        if (level == 1) 
        {
            // Mostly Scooter, rare Car
            prefabToSpawn = (Random.value > 0.8f) ? carPrefab : scooterPrefab;
        }
        else if (level == 2) 
        {
             // Mostly Car, rare Scooter
            prefabToSpawn = (Random.value > 0.8f) ? scooterPrefab : carPrefab;
        }
        else 
        {
            // Random Mixed
            prefabToSpawn = Random.value > 0.5f ? carPrefab : scooterPrefab;
        }

        // 2. Spawn Vehicle
        if (prefabToSpawn != null && spawnPoint != null)
        {
            GameObject obj = Instantiate(prefabToSpawn, spawnPoint);
            // Reset position/scale just in case
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            currentVehicle = obj.GetComponent<VehicleController>();
        }

        // 3. Initialize Vehicle (Randomize Problems 3-10)
        if (currentVehicle != null)
        {
            StartCoroutine(currentVehicle.DriveIn());
            
            // Randomize active parts count: 3 to 10
            currentVehicle.Initialize(3, 10);
            
            // 4. Update References
            repairTargets = currentVehicle.GetComponentsInChildren<RepairTarget>(true);
            
            // Only count active ones
            List<RepairTarget> activeTargets = new List<RepairTarget>();
            foreach(var t in repairTargets)
            {
                if (t.gameObject.activeSelf) activeTargets.Add(t);
            }
            brokenPartsCount = activeTargets.Count;
        }
        else
        {
             // Fallback for existing scene setup (if not using prefabs)
            if (repairTargets == null || repairTargets.Length == 0)
            {
                repairTargets = FindObjectsByType<RepairTarget>(FindObjectsSortMode.None);
            }
            brokenPartsCount = repairTargets.Length;
        }
        
        UpdateUI();
    }

    private void Update()
    {
        if (isGameActive)
        {
            if (countDown)
            {
                timer -= Time.deltaTime;
                if (timer <= 0) timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
            
            UpdateUI();
        }
    }

    public void OnPartFixed()
    {
        if (!isGameActive) return;

        brokenPartsCount--;
        AddScore(scorePerPart);
        UpdateUI();

        if (brokenPartsCount <= 0)
        {
            StartCoroutine(WinGameSequence());
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    private IEnumerator WinGameSequence()
    {
        Debug.Log("Vehicle Repaired!");
        vehiclesFixed++;
        
        // Play Cutscene
        if (currentVehicle != null)
        {
            yield return currentVehicle.DriveAway();
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        if (vehiclesFixed >= totalVehiclesToFix)
        {
            isGameActive = false;
            Debug.Log("Victory! All vehicles fixed.");
            
            GameSession.LastLevelScore = score;
            // GameSession.LastLevelStars = CalculateStars(); // If you had stars logic

            yield return new WaitForSeconds(1.0f);
            UnityEngine.SceneManagement.SceneManager.LoadScene(victorySceneName);
        }
        else
        {
            // Next Vehicle
            SpawnNextVehicle();
        }
    }

    private void UpdateUI()
    {
        if (timerText != null) timerText.text = $"Time: {timer:F0}";
        if (partsLeftText != null) partsLeftText.text = $"Parts Left: {brokenPartsCount}";
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }
}
