# Level System Integration Verification Checklist

The leveling system has been extracted and integrated into the project. Please perform the following steps to verify and finalize the setup in the Unity Editor:

## 1. Scene Setup
- [ ] Open `Assets/Scenes/Main_Menu.unity` (or create it if missing).
  - Add `MainMenuController` to a GameObject.
  - Link "Start Game" button to `MainMenuController.OnStartGame()`.
- [ ] Create a new scene named `LevelSelect` (or add to Build Settings).
  - Add `LevelSelectController` to a GameObject.
  - Create UI buttons for Level 1, 2, 3.
  - Link buttons to `LevelSelectController.LoadGarageLevel(1)`, `(2)`, etc.
  - **Note:** The `LoadGarageLevel` method currently loads the `Garage` scene. If you want to go directly to the game, change `garageSceneName` in the Inspector to `GameScene` or update the script.
- [ ] Open `Assets/Scenes/GameScene.unity`.
  - Ensure `GameManager` is present in the scene.
  - Ensure `LevelManager` is present in the scene (create an empty GameObject and add `LevelManager` script).
  - Ensure `SeamlessSpawner` is present.
- [ ] Create a new scene named `Victory` (or add to Build Settings).
  - Add `VictoryController` to a GameObject.
  - Link "Next Level" button to `VictoryController.OnNextLevel()`.
  - Link "Menu" button to `VictoryController.OnBackToMenu()`.

## 2. Level Configuration
- [ ] Select the `LevelManager` GameObject in `GameScene`.
- [ ] In the Inspector, you can verify the `Levels` list.
  - **Auto-Generation:** The script automatically generates 3 default levels (Green Hills, Desert Dunes, Moon Base) on start if the list is empty. You don't need to manually create ScriptableObjects unless you want custom ones.
  - If you want custom levels:
    1. Right-click in Project view -> Create -> Game -> Level Data.
    2. Configure the Level Data asset.
    3. Drag it into the `Levels` list on `LevelManager`.

## 3. Build Settings
- [ ] Go to File -> Build Settings.
- [ ] Add the following scenes in order (or ensure they are present):
  1. `Main_Menu`
  2. `LevelSelect`
  3. `Garage` (Optional, if used)
  4. `GameScene`
  5. `Victory`

## 4. Testing
- [ ] Play from `Main_Menu`.
- [ ] Click Start -> Select Level 1.
- [ ] Verify the game loads with normal gravity.
- [ ] Complete the level (drive 500m).
- [ ] Verify `Victory` scene loads.
- [ ] Click "Next Level".
- [ ] Verify `GameScene` reloads and feels different (Level 2: Desert Dunes has higher gravity/drag).

## Troubleshooting
- If scenes don't load, check the spelling in `SceneNames.cs` matches your scene filenames exactly.
- If terrain looks wrong, ensure `SeamlessSpawner` has a valid default `layerPrefab` or that `LevelManager` is providing one.
