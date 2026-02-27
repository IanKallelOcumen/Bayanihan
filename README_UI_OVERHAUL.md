# UI Overhaul & Project Update

This repository contains the latest UI/UX overhaul for the Bayanihan project.

## 1. UI Inventory
A complete inventory of all UI elements is available in `UI_Inventory.csv` in the project root.
- **Tools > Bayanihan > Generate UI Inventory** to update.

## 2. Font Replacement
All legacy fonts have been replaced with a unified font.
- **Tools > Bayanihan > Font Replacer** to re-run.
- Supported: `Text` and `TextMeshProUGUI`.

## 3. UI Validation
- Screenshots are saved to `/UI_Validation`.
- Attach `UIValidator` script to a GameObject and call `CaptureScreenshots()`.

## 4. Button Styling
- All buttons now support 9-slice sprite swapping.
- **Tools > Bayanihan > Button Styler** to apply the new style batch-wise.

## 5. Color Palette & Spacing
- `ColorPaletteSO`: Centralized color management.
- `SpacingSystemSO`: Centralized padding/spacing management (4px, 8px, 16px, 24px, 32px).
- `LayoutSpacingEnforcer`: Attach to any LayoutGroup to enforce spacing rules.

## 6. Levels
- Three new levels (`Level_01`, `Level_02`, `Level_03`) have been created in `Assets/Data/Levels`.
- Use **Tools > Bayanihan > Create Default Levels** to regenerate them if missing.
- Use **Tools > Bayanihan > Master Setup > Setup Level System** to wire them into the `LevelManager`.

## 7. Terrain Fixes
- `TerrainGenerator` now checks for null `TerrainData` on Awake.
- Automatically generates a 513x513 procedural height map using Perlin Noise.

## How to Test
1. **Open Unity Editor.**
2. **Run Tools > Bayanihan > Generate UI Inventory** -> Verify `UI_Inventory.csv` is created.
3. **Run Tools > Bayanihan > Create Default Levels** -> Verify Levels 01-03 exist in `Assets/Data/Levels`.
4. **Run Tools > Bayanihan > Master Setup > Setup Level System** -> Check Console for "Found and assigned 3 levels".
5. **Press Play** in `GameScene` or `Main_Menu`.
6. Verify Terrain generates without errors.
7. Verify UI looks consistent and modernized.

**Checklist:**
- [x] UI Inventory CSV
- [x] Font Replacer Tool
- [x] UI Repositioning Logic (SpacingSystem)
- [x] Button 9-Slice Tool
- [x] Color Palette System
- [x] Spacing System (ScriptableObject)
- [x] Levels 01, 02, 03 Created
- [x] Terrain Generator Fixes (HeightMap)
- [x] UI Validator Script

*If any item is unchecked or any screenshot missing, the delivery is incomplete.*
