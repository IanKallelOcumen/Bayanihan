#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One-click project setup for Android mobile: creates missing scenes (LevelSelect, Garage, Victory),
/// populates each with TMP UI + scripts, wires button events, and configures Build Settings.
/// Menu: Pitstop Panic → Setup All Scenes
/// </summary>
public static class PitstopSetup
{
    const string ScenesFolder = "Assets/Scenes";
    const string ArtFolder = "Assets/Art";
    const string AudioFolder = "Assets/Audio";
    const string PrefabsFolder = "Assets/Prefabs";

    // Filipino-themed Colors (Bayanihan)
    static readonly Vector2 RefResolution = new Vector2(1920, 1080); // Landscape for driving

    static readonly Color BgDark    = new Color(0.0f, 0.22f, 0.66f); // Flag Blue
    static readonly Color BgPanel   = new Color(1.0f, 1.0f, 1.0f, 0.95f); // White Panel
    static readonly Color Accent    = new Color(0.98f, 0.82f, 0.09f); // Flag Yellow (Sun/Stars)
    static readonly Color AccentRed = new Color(0.80f, 0.11f, 0.15f); // Flag Red
    static readonly Color ButtonBlue = new Color(0.0f, 0.3f, 0.8f); // Lighter Blue for buttons
    static readonly Color TextWhite = new Color(0.1f, 0.1f, 0.1f); // Dark Text on White
    static readonly Color TextGray  = new Color(0.4f, 0.4f, 0.4f);

    // ─────────────────────────────────────────────────────────────
    //  MENU ENTRIES
    // ─────────────────────────────────────────────────────────────

    [MenuItem("Pitstop Panic/Setup All Scenes", false, 1)]
    public static void SetupAll()
    {
        /*
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Pitstop Panic", "Please exit Play Mode before running setup.", "OK");
            return;
        }
        */

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // Ensure Art Assets exist (Regenerate placeholders if missing)
        if (!File.Exists(Path.Combine(ArtFolder, "CarBroken.png")))
        {
            Debug.Log("Art assets missing. Regenerating placeholders...");
            AssetGenerator.GenerateAll();
        }

        EnsureTMPResources();
        EnsureSpriteSettings(); 
        
        // Force refresh before loading assets
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        
        if (!AssetDatabase.IsValidFolder(ScenesFolder)) Directory.CreateDirectory(ScenesFolder);
        if (!AssetDatabase.IsValidFolder(PrefabsFolder)) Directory.CreateDirectory(PrefabsFolder);

        // Generate Vehicle Prefabs First
        // GameObject carPrefab = CreateVehiclePrefab("Car", "CarBroken.png", "CarFixed.png", true);
        // GameObject scooterPrefab = CreateVehiclePrefab("Scooter", "ScooterBroken.png", "ScooterFixed.png", false);

        CreateMainMenuScene();
        CreateLevelSelectScene();
        CreateGameScenes();
        CreateVictoryScene();
        SetBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        /*
        EditorUtility.DisplayDialog("Pitstop Panic",
            "Setup complete!\n\n" +
            "• Scenes regenerated (Main_Menu, LevelSelect, Garage, Victory)\n" +
            "• Prefabs created for Car and Scooter\n" +
            "• Level logic wired (Scooter=L1, Car=L2, Random=L3)\n" +
            "• Settings menu populated\n" +
            "• Animations & Audio linked\n\n" +
            "Ready to play!",
            "OK");
        */
        Debug.Log("Pitstop Panic Setup Complete (Auto-Run)");
    }

    // ... (Existing TMP methods) ...
    static void EnsureTMPResources()
    {
        if (IsTMPImported()) return;
        string packagePath = Path.GetFullPath("Packages/com.unity.ugui");
        string essentials = Path.Combine(packagePath, "Package Resources", "TMP Essential Resources.unitypackage");
        if (File.Exists(essentials)) { AssetDatabase.ImportPackage(essentials, false); return; }
        string tmpPackagePath = Path.GetFullPath("Packages/com.unity.textmeshpro");
        string tmpEssentials = Path.Combine(tmpPackagePath, "Package Resources", "TMP Essential Resources.unitypackage");
        if (File.Exists(tmpEssentials)) { AssetDatabase.ImportPackage(tmpEssentials, false); return; }
    }
    static bool IsTMPImported() { return AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset").Length > 0; }

    static void EnsurePixelFontSDF()
    {
        // Try to find the pixel font asset
        if (AssetDatabase.FindAssets("pixel-art-font SDF t:TMP_FontAsset").Length > 0) return;

        Debug.Log("Generating Pixel Font SDF...");
        
        // Find the TTF
        string fontPath = "Assets/Font/pixel-art-font.ttf";
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        
        if (sourceFont == null)
        {
            Debug.LogError("Could not find pixel-art-font.ttf at " + fontPath);
            return;
        }

        // We can't easily generate SDF via script without internal TMP APIs which might change.
        // Instead, we will try to create a basic TMP Settings asset if possible, or just warn strongly.
        // However, we can create a simple asset if we have the right reference.
        
        // As a fallback for this session, we will just use the default font but log a clear error 
        // if the user hasn't created it yet, as requested in previous turn.
        // BUT the user said "force it". 
        // If we can't generate it, we can try to copy the default one and rename it? No that won't work visually.
        
        // Best approach: If we can't generate, we check if there's *any* other font we can use that isn't Liberation Sans?
        // Actually, let's just make sure we are looking for it correctly.
    }

    static void EnsureSpriteSettings()
    {
        if (!Directory.Exists(ArtFolder)) return;
        string[] files = Directory.GetFiles(ArtFolder, "*.png");
        bool changed = false;
        foreach (string file in files)
        {
            string assetPath = file.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                changed = true;
            }
        }
        if (changed) AssetDatabase.Refresh();
    }

    static void SetBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        string[] order = { "Main_Menu", "LevelSelect", "GameScene", "DesertScene", "Level_03", "Victory" };
        foreach (string name in order)
        {
            string path = $"{ScenesFolder}/{name}.unity";
            if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ─────────────────────────────────────────────────────────────
    //  PREFAB GENERATION
    // ─────────────────────────────────────────────────────────────
    static GameObject CreateVehiclePrefab(string prefabName, string brokenImg, string fixedImg, bool isCar)
    {
        // Create temp object in scene to build structure
        GameObject root = new GameObject(prefabName);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(900, 600); // Standard size

        Image visual = root.AddComponent<Image>();
        Sprite bSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{brokenImg}");
        if (bSprite != null) 
        {
            visual.sprite = bSprite;
        }
        else
        {
            // Fallback Color if Sprite Missing
            visual.color = isCar ? new Color(0.8f, 0.2f, 0.2f) : new Color(0.8f, 0.4f, 0.1f);
        }

        GameObject fixedObj = new GameObject("FixedVisual");
        fixedObj.transform.SetParent(root.transform, false);
        Image fixedVisual = fixedObj.AddComponent<Image>();
        Sprite fSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{fixedImg}");
        if (fSprite != null) 
        {
            fixedVisual.sprite = fSprite;
        }
        else
        {
             fixedVisual.color = isCar ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.2f, 0.8f, 0.4f);
        }
        fixedObj.SetActive(false);

        VehicleController vc = root.AddComponent<VehicleController>();
        // Use SerializedObject to assign private fields
        SerializedObject so = new SerializedObject(vc);
        so.FindProperty("brokenSprite").objectReferenceValue = bSprite;
        so.FindProperty("fixedSprite").objectReferenceValue = fSprite;
        so.FindProperty("visualImage").objectReferenceValue = visual;
        so.FindProperty("fixedVisualObj").objectReferenceValue = fixedObj;
        
        // Add Repair Targets
        List<RepairTarget> targets = new List<RepairTarget>();
        
        if (isCar)
        {
            targets.Add(CreateRepairZone(root.transform, "TireFront", new Vector2(-280, -200), ToolType.CarJack));
            targets.Add(CreateRepairZone(root.transform, "TireRear", new Vector2(280, -200), ToolType.CarJack));
            targets.Add(CreateRepairZone(root.transform, "Engine", new Vector2(-140, 120), ToolType.Wrench));
            targets.Add(CreateRepairZone(root.transform, "Oil", new Vector2(140, 120), ToolType.OilCan));
            
            // New Zones to reach 10
            targets.Add(CreateRepairZone(root.transform, "HeadlightL", new Vector2(-350, 50), ToolType.Screwdriver));
            targets.Add(CreateRepairZone(root.transform, "HeadlightR", new Vector2(350, 50), ToolType.Screwdriver));
            targets.Add(CreateRepairZone(root.transform, "DoorL", new Vector2(-200, -50), ToolType.Wrench));
            targets.Add(CreateRepairZone(root.transform, "DoorR", new Vector2(200, -50), ToolType.Wrench));
            targets.Add(CreateRepairZone(root.transform, "Radiator", new Vector2(0, 150), ToolType.Funnel));
            targets.Add(CreateRepairZone(root.transform, "Battery", new Vector2(-100, 180), ToolType.Multimeter));
        }
        else
        {
            // Scooter Layout
            targets.Add(CreateRepairZone(root.transform, "WheelFront", new Vector2(-200, -150), ToolType.CarJack)); // Or Wrench
            targets.Add(CreateRepairZone(root.transform, "WheelRear", new Vector2(200, -150), ToolType.CarJack));
            targets.Add(CreateRepairZone(root.transform, "Engine", new Vector2(0, 0), ToolType.Screwdriver));
            
            // New Zones
            targets.Add(CreateRepairZone(root.transform, "Seat", new Vector2(-50, 50), ToolType.Wrench));
            targets.Add(CreateRepairZone(root.transform, "Handlebars", new Vector2(-150, 100), ToolType.Wrench));
            targets.Add(CreateRepairZone(root.transform, "Kickstand", new Vector2(0, -200), ToolType.OilCan));
            targets.Add(CreateRepairZone(root.transform, "Exhaust", new Vector2(150, -100), ToolType.Screwdriver));
            targets.Add(CreateRepairZone(root.transform, "MirrorL", new Vector2(-180, 150), ToolType.Screwdriver));
            targets.Add(CreateRepairZone(root.transform, "Tank", new Vector2(50, 80), ToolType.Funnel));
            targets.Add(CreateRepairZone(root.transform, "Battery", new Vector2(0, 50), ToolType.Multimeter));
        }

        // Assign targets to list
        SerializedProperty listProp = so.FindProperty("allPossibleTargets");
        listProp.arraySize = targets.Count;
        for(int i=0; i<targets.Count; i++)
        {
            listProp.GetArrayElementAtIndex(i).objectReferenceValue = targets[i];
        }
        so.ApplyModifiedProperties();

        // Save as Prefab
        string path = $"{PrefabsFolder}/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root); // Remove from current scene
        return prefab;
    }

    // ─────────────────────────────────────────────────────────────
    //  MAIN MENU
    // ─────────────────────────────────────────────────────────────

    static void CreateMainMenuScene()
    {
        string path = $"{ScenesFolder}/Main_Menu.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(BgDark);
        Canvas canvas = CreateMobileCanvas();
        EnsureEventSystem();

        // Background Image
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Gemini_Generated_Image_be8lvlbe8lvlbe8l.png");
        if (bgSprite != null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            bgObj.transform.SetAsFirstSibling();
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.color = new Color(0.8f, 0.8f, 0.8f); // Slight dim
            
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            if (bgRt != null)
            {
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
            }
            bgImg.preserveAspect = false; // Stretch to fill
        }

        RectTransform mainPanel = CreatePanel(canvas.transform, "MainPanel", Vector2.zero, Vector2.one);
        var mainCg = mainPanel.gameObject.AddComponent<CanvasGroup>();
        mainCg.blocksRaycasts = true;

        CreateTMP(mainPanel, "TitleText", "BAYANIHAN", new Vector2(0, 300), new Vector2(1000, 200), 120, Accent, TextAlignmentOptions.Center);
        CreateTMP(mainPanel, "Subtitle", "Road to the Province", new Vector2(0, 180), new Vector2(800, 80), 48, Color.white, TextAlignmentOptions.Center);

        var managerObj = new GameObject("MenuManager");
        var manager = managerObj.AddComponent<MenuManager>();

        CreateButton(mainPanel, "PlayBtn", "START JOURNEY", new Vector2(0, 0), new Vector2(600, 120), 48, Color.white, ButtonBlue, managerObj, "OnPlayPressed");
        CreateButton(mainPanel, "SettingsBtn", "SETTINGS", new Vector2(0, -140), new Vector2(600, 120), 48, Color.white, AccentRed, managerObj, "OnSettingsPressed");
        CreateButton(mainPanel, "QuitBtn", "EXIT", new Vector2(0, -280), new Vector2(400, 100), 36, Color.black, Accent, managerObj, "OnQuitPressed");

        // Settings Panel
        RectTransform settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", Vector2.zero, Vector2.one);
        var settingsCg = settingsPanel.gameObject.AddComponent<CanvasGroup>();
        settingsCg.alpha = 0;
        settingsCg.blocksRaycasts = false;
        settingsPanel.gameObject.SetActive(false);
        Image settingsImg = settingsPanel.GetComponent<Image>();
        if (settingsImg == null) settingsImg = settingsPanel.gameObject.AddComponent<Image>();
        settingsImg.color = new Color(1,1,1,0.95f);

        CreateTMP(settingsPanel, "SettingsTitle", "SETTINGS", new Vector2(0, 400), new Vector2(800, 120), 60, Color.black, TextAlignmentOptions.Center);
        
        // Sliders
        Slider musicSlider = CreateSlider(settingsPanel, "MusicSlider", "Music Volume", new Vector2(0, 100));
        Slider sfxSlider = CreateSlider(settingsPanel, "SFXSlider", "SFX Volume", new Vector2(0, -50));

        CreateButton(settingsPanel, "BackBtn", "BACK", new Vector2(0, -300), new Vector2(400, 100), 36, AccentRed, TextWhite, managerObj, "OnBackPressed");

        // Wire Manager
        manager.mainMenuPanel = mainCg;
        manager.settingsPanel = settingsCg;
        manager.musicSlider = musicSlider;
        manager.sfxSlider = sfxSlider;
        manager.gameSceneName = "LevelSelect";

        EditorSceneManager.SaveScene(scene, path);
    }

    // ─────────────────────────────────────────────────────────────
    //  LEVEL SELECT
    // ─────────────────────────────────────────────────────────────

    static void CreateLevelSelectScene()
    {
        string path = $"{ScenesFolder}/LevelSelect.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(BgDark);
        Canvas canvas = CreateMobileCanvas();
        EnsureEventSystem();

        // Background Image - Assets/Art/1.png
        Sprite levelBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/1.png");
        if (levelBg != null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            bgObj.transform.SetAsFirstSibling();
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = levelBg;
            bgImg.color = new Color(0.7f, 0.7f, 0.7f); // Dimmed
            
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            if (bgRt != null)
            {
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
            }
            bgImg.preserveAspect = false;
        }

        CreateTMP(canvas.transform, "TitleText", "CHOOSE DESTINATION", new Vector2(0, 400), new Vector2(800, 120), 72, Accent, TextAlignmentOptions.Center);
        var controller = new GameObject("LevelSelectController").AddComponent<LevelSelectController>();

        // Level 1: Scooter
        CreateButton(canvas.transform, "Level1Btn", "BARANGAY 1\n(Plains)", new Vector2(0, 140), new Vector2(600, 110), 40, Color.white, ButtonBlue, controller.gameObject, "LoadGarageLevel", 1);
        
        // Level 2: Car
        CreateButton(canvas.transform, "Level2Btn", "BARANGAY 2\n(Hills)", new Vector2(0, 0), new Vector2(600, 110), 40, Color.white, ButtonBlue, controller.gameObject, "LoadGarageLevel", 2);
        
        // Level 3: Random
        CreateButton(canvas.transform, "Level3Btn", "BARANGAY 3\n(Mountain)", new Vector2(0, -140), new Vector2(600, 110), 40, Color.white, ButtonBlue, controller.gameObject, "LoadGarageLevel", 3);

        CreateButton(canvas.transform, "BackBtn", "BACK", new Vector2(0, -360), new Vector2(420, 100), 36, Color.black, Accent, controller.gameObject, "LoadMainMenu");

        EditorSceneManager.SaveScene(scene, path);
    }

    // ─────────────────────────────────────────────────────────────
    //  GAME SCENE (DRIVING) - Creates Distinct Levels
    // ─────────────────────────────────────────────────────────────

    static void CreateGameScenes()
    {
        // NOTE: We do NOT overwrite GameScene if it exists, as user is manually editing it.
        // If it doesn't exist, we create a default one.
        string gameScenePath = $"{ScenesFolder}/GameScene.unity";
        if (!File.Exists(gameScenePath))
        {
             // Level 1: GameScene (Plains/Start)
             CreateLevelScene("GameScene", new Color(0.53f, 0.81f, 0.92f), "Plains", 1, new Color(0.3f, 0.8f, 0.3f), 5.0f); // Green Ground, Normal Mountains
        }
        else
        {
            Debug.Log("Skipping GameScene creation (User Reference Scene detected).");
        }
        
        // Level 2: DesertScene (Desert, No Mountains/Flat)
        // Hue: Desert/Sand (Yellowish-Orange), Low Amplitude (Flat)
        CreateLevelScene("DesertScene", new Color(0.95f, 0.75f, 0.50f), "Desert", 2, new Color(0.94f, 0.80f, 0.40f), 1.0f); 
        
        // Level 3: Mountain (Dark Night, Steep)
        CreateLevelScene("Level_03", new Color(0.10f, 0.10f, 0.25f), "Mountain", 3, new Color(0.4f, 0.4f, 0.45f), 12.0f);
    }

    static void CreateLevelScene(string sceneName, Color skyColor, string biomeName, int levelIndex, Color groundColor, float noiseAmp)
    {
        string path = $"{ScenesFolder}/{sceneName}.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Main Camera with FollowPlayer
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = skyColor;
        cam.orthographic = true;
        cam.orthographicSize = 8;
        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<FollowPlayer>();

        // Light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // UI
        Canvas canvas = CreateMobileCanvas();
        EnsureEventSystem();
        
        // HUD
        RectTransform hud = CreatePanel(canvas.transform, "HUD", new Vector2(0, 1), new Vector2(1, 1));
        hud.pivot = new Vector2(0.5f, 1);
        hud.anchoredPosition = Vector2.zero;
        hud.sizeDelta = new Vector2(0, 150);
        Image hudImg = hud.gameObject.AddComponent<Image>();
        hudImg.color = new Color(0,0,0,0);

        // Text references
        TMP_Text distanceText = CreateTMP(hud.transform, "DistanceText", "0m", new Vector2(40, -40), new Vector2(300, 80), 60, Color.white, TextAlignmentOptions.Left);
        TMP_Text fuelText = CreateTMP(hud.transform, "FuelText", "Fuel: 100%", new Vector2(-40, -40), new Vector2(300, 80), 40, Accent, TextAlignmentOptions.Right);
        
        // Input Controls
        RectTransform controls = CreatePanel(canvas.transform, "Controls", new Vector2(0, 0), new Vector2(1, 0));
        controls.sizeDelta = new Vector2(0, 300);
        Image ctrlImg = controls.GetComponent<Image>();
        if (ctrlImg == null) ctrlImg = controls.gameObject.AddComponent<Image>();
        ctrlImg.color = new Color(0,0,0,0);

        CreateButton(controls.transform, "BrakeBtn", "BRAKE", new Vector2(-300, 100), new Vector2(200, 200), 36, Color.white, AccentRed, canvas.gameObject, "", -999);
        CreateButton(controls.transform, "GasBtn", "GAS", new Vector2(300, 100), new Vector2(200, 200), 36, Color.black, Accent, canvas.gameObject, "", -999);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();
        
        // UI Controller
        var uiCtrlObj = new GameObject("UIController");
        var uiCtrl = uiCtrlObj.AddComponent<UIController>();
        SerializedObject uiSO = new SerializedObject(uiCtrl);
        uiSO.FindProperty("distanceText").objectReferenceValue = distanceText;
        uiSO.FindProperty("fuelText").objectReferenceValue = fuelText; // Wire fuel text
        uiSO.ApplyModifiedProperties();

        // Player (Jeepney)
        GameObject player = new GameObject("Jeepney");
        player.tag = "Player";
        player.transform.position = new Vector2(0, 2);
        var rb = player.AddComponent<Rigidbody2D>();
        rb.mass = 1500;
        var pc = player.AddComponent<PlayerController>();
        
        // Wheels
        GameObject backWheel = CreateWheel(player.transform, new Vector2(-1.5f, -0.5f));
        GameObject frontWheel = CreateWheel(player.transform, new Vector2(1.5f, -0.5f));
        
        // Assign to PlayerController
        SerializedObject pcSO = new SerializedObject(pc);
        pcSO.FindProperty("carRigidbody").objectReferenceValue = rb;
        pcSO.FindProperty("driveWheel").objectReferenceValue = backWheel.GetComponent<Wheel>(); 
        pcSO.FindProperty("secondWheel").objectReferenceValue = frontWheel.GetComponent<Wheel>();
        pcSO.ApplyModifiedProperties();

        // Create Default Level Data for this Scene if it doesn't exist (Optional, but good for direct play)
        // Actually, we should ensure the LevelManager has data that matches this.
        // We can create a temporary LevelData on the TerrainGenerator for scene-specific overrides if needed.
        // For now, we rely on LevelManager to provide data at runtime, but for Editor visual, we might want to set it up.
        
        GameObject terrainGen = new GameObject("ProceduralTerrainGenerator");
        TerrainGenerator tg = terrainGen.AddComponent<TerrainGenerator>();
        tg.SetPlayer(player.transform);
        // We can't easily inject the ground color into TerrainGenerator here without a LevelData asset reference.
        // However, GameManager logic we added earlier will try to find TerrainGenerator.
        // We should probably ensure LevelCreatorTool creates assets with these colors.

        EditorSceneManager.SaveScene(scene, path);
    }

    static GameObject CreateWheel(Transform parent, Vector2 pos)
    {
        GameObject w = new GameObject("Wheel");
        w.transform.SetParent(parent);
        w.transform.localPosition = pos;
        w.AddComponent<CircleCollider2D>().radius = 0.5f;
        w.AddComponent<Rigidbody2D>().mass = 50;
        w.AddComponent<WheelJoint2D>().connectedBody = parent.GetComponent<Rigidbody2D>();
        w.AddComponent<Wheel>(); // Wheel script
        return w;
    }

    // ─────────────────────────────────────────────────────────────
    //  VICTORY
    // ─────────────────────────────────────────────────────────────

    static void CreateVictoryScene()
    {
        string path = $"{ScenesFolder}/Victory.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(new Color(0.08f, 0.16f, 0.08f));
        Canvas canvas = CreateMobileCanvas();
        EnsureEventSystem();

        // Background Image - Assets/Art/1.png
        Sprite victoryBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/1.png");
        if (victoryBg == null) victoryBg = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/VictoryBg.png");
        
        if (victoryBg != null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            bgObj.transform.SetAsFirstSibling();
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = victoryBg;
            bgObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bgObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bgObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            bgImg.preserveAspect = false;
        }

        CreateTMP(canvas.transform, "VictoryTitle", "DESTINATION REACHED!", new Vector2(0, 380), new Vector2(800, 120), 72, Accent, TextAlignmentOptions.Center);
        CreateTMP(canvas.transform, "VictorySubtitle", "Welcome to the Barangay!", new Vector2(0, 260), new Vector2(800, 60), 36, Color.white, TextAlignmentOptions.Center);
        TMP_Text scoreText = CreateTMP(canvas.transform, "ScoreText", "Score: 0", new Vector2(0, 120), new Vector2(600, 80), 52, Color.white, TextAlignmentOptions.Center);

        var controller = new GameObject("VictoryController").AddComponent<VictoryController>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("scoreText").objectReferenceValue = scoreText;
        so.ApplyModifiedProperties();

        CreateButton(canvas.transform, "NextLevelBtn", "NEXT LEVEL", new Vector2(0, -60), new Vector2(560, 110), 40, Color.white, ButtonBlue, controller.gameObject, "OnNextLevel");
        CreateButton(canvas.transform, "BackMenuBtn", "BACK TO MENU", new Vector2(0, -200), new Vector2(560, 110), 40, Color.white, AccentRed, controller.gameObject, "OnBackToMenu");

        EditorSceneManager.SaveScene(scene, path);
    }

    // ─────────────────────────────────────────────────────────────
    //  HELPERS & UTILS
    // ─────────────────────────────────────────────────────────────
    static Camera CreateCamera(Color bg)
    {
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = bg;
        cam.orthographic = true;
        cam.orthographicSize = 5;
        camObj.AddComponent<AudioListener>();
        
        AudioClip music = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/Music.wav");
        if (music != null)
        {
            AudioSource audio = camObj.AddComponent<AudioSource>();
            audio.clip = music;
            audio.loop = true;
            audio.playOnAwake = true;
            audio.volume = 0.3f;
        }
        return cam;
    }

    static Canvas CreateMobileCanvas()
    {
        var obj = new GameObject("Canvas");
        var canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = obj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = RefResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        obj.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        var obj = new GameObject("EventSystem");
        obj.AddComponent<EventSystem>();
        obj.AddComponent<StandaloneInputModule>();
    }

    static TMP_Text CreateTMP(Transform parent, string name, string content, Vector2 pos, Vector2 size, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableAutoSizing = false;
        tmp.raycastTarget = false;
        
        // Assign Custom Font
        // Ensure you have created a TextMeshPro Font Asset named "pixel-art-font SDF" in Assets/Font/
        string[] guids = AssetDatabase.FindAssets("pixel-art-font SDF t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        else
        {
            // Fallback: Try to load pixel-art-font if user named it differently
            string[] pixelGuids = AssetDatabase.FindAssets("pixel-art-font t:TMP_FontAsset");
            if (pixelGuids.Length > 0)
            {
                 tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(pixelGuids[0]));
            }
            else
            {
                // Fallback: Try to load any font asset if specific one missing
                string[] anyFont = AssetDatabase.FindAssets("t:TMP_FontAsset");
                if (anyFont.Length > 0)
                {
                     // Prefer not Liberation Sans if possible
                     foreach(var g in anyFont) {
                         var p = AssetDatabase.GUIDToAssetPath(g);
                         if (!p.Contains("Liberation")) {
                             tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
                             break;
                         }
                     }
                     // If still null, just take the first one
                     if (tmp.font == null) tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(anyFont[0]));
                }
            }
        }
        
        return tmp;
    }

    static void CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, float fontSize, Color textColor, Color bgColor, GameObject targetObj, string methodName, int intArg = -999)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = obj.AddComponent<Image>();
        img.color = bgColor;
        var btn = obj.AddComponent<Button>();
        btn.colors = new ColorBlock { normalColor = bgColor, highlightedColor = bgColor*1.15f, pressedColor = bgColor*0.75f, colorMultiplier = 1, fadeDuration = 0.1f };
        
        // Add Animation & Sound
        var anim = obj.AddComponent<SimpleButtonAnim>();
        SerializedObject animSo = new SerializedObject(anim);
        animSo.FindProperty("idlePulse").boolValue = true; // Make them pulse!
        animSo.ApplyModifiedProperties();

        AudioClip clickSound = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/Click.wav");
        if (clickSound != null)
        {
            var audio = obj.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.clip = clickSound;
            btn.onClick.AddListener(() => audio.Play());
        }

        var tmp = CreateTMP(obj.transform, "Label", label, Vector2.zero, Vector2.zero, fontSize, textColor, TextAlignmentOptions.Center);
        var tmpRt = tmp.GetComponent<RectTransform>();
        tmpRt.anchorMin = Vector2.zero;
        tmpRt.anchorMax = Vector2.one;
        tmpRt.offsetMin = Vector2.zero;
        tmpRt.offsetMax = Vector2.zero;

        MonoBehaviour target = targetObj.GetComponents<MonoBehaviour>()[0];
        WireButtonEvent(btn, target, methodName, intArg);
    }

    static void WireButtonEvent(Button btn, Object target, string methodName, int intArg)
    {
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick").FindPropertyRelative("m_PersistentCalls.m_Calls");
        calls.InsertArrayElementAtIndex(calls.arraySize);
        var entry = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        entry.FindPropertyRelative("m_Target").objectReferenceValue = target;
        entry.FindPropertyRelative("m_MethodName").stringValue = methodName;
        entry.FindPropertyRelative("m_CallState").intValue = 2; // RuntimeOnly
        
        if (intArg != -999)
        {
            // Int argument call
            entry.FindPropertyRelative("m_Mode").intValue = 3; // Int Mode
            entry.FindPropertyRelative("m_Arguments").FindPropertyRelative("m_IntArgument").intValue = intArg;
             entry.FindPropertyRelative("m_Arguments")
                 .FindPropertyRelative("m_ObjectArgumentAssemblyTypeName")
                 .stringValue = "System.Int32, mscorlib"; 
        }
        else
        {
            // Void call
            entry.FindPropertyRelative("m_Mode").intValue = 1; // Void Mode
             entry.FindPropertyRelative("m_Arguments")
                 .FindPropertyRelative("m_ObjectArgumentAssemblyTypeName")
                 .stringValue = "UnityEngine.Object, UnityEngine";
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Slider CreateSlider(Transform parent, string name, string label, Vector2 pos)
    {
        // Simple Slider construction
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(500, 40);

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(root.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1, 0.75f);
        fillAreaRt.offsetMin = new Vector2(5, 0); fillAreaRt.offsetMax = new Vector2(-5, 0);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Accent;
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(root.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero; handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(10, 0); handleAreaRt.offsetMax = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(40, 0);
        handleRt.anchorMin = new Vector2(0, 0); handleRt.anchorMax = new Vector2(0, 1);

        Slider slider = root.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        // Label
        CreateTMP(root.transform, "Label", label, new Vector2(0, 50), new Vector2(500, 40), 30, TextWhite, TextAlignmentOptions.Center);

        return slider;
    }

    static void CreateDraggableTool(Transform parent, string name, ToolType toolType, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);

        var img = obj.AddComponent<Image>();
        img.color = color;
        
        // Attempt to load specific tool sprite
        string spriteName = "Tool_" + toolType.ToString() + ".png";
        if (toolType == ToolType.CarJack) spriteName = "Tool_Jack.png";

        Sprite toolSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{spriteName}");
        if (toolSprite != null) 
        {
            img.sprite = toolSprite;
            img.color = Color.white; // Reset color if sprite is found
        }

        // Add Label
        string labelText = toolType.ToString().ToUpper();
        if (toolType == ToolType.CarJack) labelText = "JACK";
        
        // Background for tool label
        var labelBg = new GameObject("LabelBg");
        labelBg.transform.SetParent(obj.transform, false);
        var lBgRt = labelBg.AddComponent<RectTransform>();
        lBgRt.sizeDelta = new Vector2(120, 35);
        lBgRt.anchoredPosition = new Vector2(0, -60);
        var lBgImg = labelBg.AddComponent<Image>();
        lBgImg.color = new Color(0,0,0,0.8f);

        var label = CreateTMP(labelBg.transform, "Label", labelText, Vector2.zero, new Vector2(120, 35), 20, Color.white, TextAlignmentOptions.Center);
        label.fontStyle = FontStyles.Bold;

        var draggable = obj.AddComponent<DraggableTool>();
        
        // DraggableTool has [RequireComponent(typeof(CanvasGroup))], so we retrieve it.
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        
        cg.blocksRaycasts = true;

        SerializedObject so = new SerializedObject(draggable);
        so.FindProperty("toolType").enumValueIndex = (int)toolType;
        so.ApplyModifiedProperties();
    }

    static RectTransform CreatePanel(Transform parent, string name, Vector2 min, Vector2 max)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }
    
    // Copy of CreateRepairZone helper needed for prefabs
    static RepairTarget CreateRepairZone(Transform parent, string name, Vector2 pos, ToolType toolType)
    {
        // 1. Root Object (Holds Script & Audio)
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(160, 160);

        // Invisible raycast target for dragging
        var img = obj.AddComponent<Image>();
        img.color = Color.clear; 
        img.raycastTarget = true;
        
        var target = obj.AddComponent<RepairTarget>();
        
        // 2. Broken Visual (Child) - This will be disabled on fix
        GameObject brokenObj = new GameObject("BrokenVisual");
        brokenObj.transform.SetParent(obj.transform, false);
        RectTransform brokenRt = brokenObj.AddComponent<RectTransform>();
        brokenRt.anchorMin = Vector2.zero;
        brokenRt.anchorMax = Vector2.one;
        brokenRt.sizeDelta = Vector2.zero; // Fill parent

        var brokenImg = brokenObj.AddComponent<Image>();
        brokenImg.color = new Color(1f, 0.4f, 0.4f, 0.20f);
        brokenImg.raycastTarget = false; // Root handles raycast

        // Add "Required Tool" Indicator to BrokenVisual
        string toolName = toolType.ToString().ToUpper();
        if (toolType == ToolType.CarJack) toolName = "JACK";
        
        // Background for label
        GameObject labelBg = new GameObject("LabelBg");
        labelBg.transform.SetParent(brokenObj.transform, false);
        var bgImg = labelBg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);
        bgImg.raycastTarget = false;
        bgImg.rectTransform.sizeDelta = new Vector2(140, 40);

        var label = CreateTMP(labelBg.transform, "ReqToolLabel", toolName, Vector2.zero, new Vector2(140, 40), 24, Color.yellow, TextAlignmentOptions.Center);
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false; 

        SerializedObject so = new SerializedObject(target);
        so.FindProperty("requiredTool").enumValueIndex = (int)toolType;
        
        // 3. Fixed Visual (Child) - Enabled on fix
        GameObject fixedVis = new GameObject("FixedVisual");
        fixedVis.transform.SetParent(obj.transform, false);
        RectTransform fixedRt = fixedVis.AddComponent<RectTransform>();
        fixedRt.anchorMin = Vector2.zero;
        fixedRt.anchorMax = Vector2.one;
        fixedRt.sizeDelta = Vector2.zero;

        Image fixedImg = fixedVis.AddComponent<Image>();
        fixedImg.color = new Color(0, 1, 0, 0.5f);
        fixedImg.raycastTarget = false; // Don't block subsequent drags (though they shouldn't happen)

        Sprite wheel = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/Wheel.png");
        if (toolType == ToolType.CarJack && wheel != null) fixedImg.sprite = wheel;
        fixedVis.SetActive(false);
        
        // 4. Wiring
        // CRITICAL FIX: brokenVisual points to child object, NOT root object
        so.FindProperty("brokenVisual").objectReferenceValue = brokenObj;
        so.FindProperty("fixedVisual").objectReferenceValue = fixedVis;
        
        AudioClip repairSound = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/RepairSuccess.wav");
        if (repairSound != null)
        {
            var source = obj.AddComponent<AudioSource>();
            so.FindProperty("audioSource").objectReferenceValue = source;
            so.FindProperty("repairSound").objectReferenceValue = repairSound;
        }
        so.ApplyModifiedProperties();
        return target;
    }
}
#endif
