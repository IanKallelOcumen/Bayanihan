# 🏠 Bayanihan Hill Climb

A 2D physics-based endless runner inspired by the Filipino tradition of *Bayanihan* (moving a house). Players control a team of carriers transporting a *Bahay Kubo* (nipa hut) across uneven terrain, balancing speed with stability.

## 🎮 Gameplay Features

* **Physics-Based Movement:** Uses `WheelJoint2D` physics for realistic friction and momentum over hills.
* **Balance Mechanic:** The "Car" (Carriers + House) must maintain balance. If the angle tilts too far, the run ends.
* **Resource Management:** Players burn fuel (Stamina/Water) over time and must collect pickups to keep going.
* **Dynamic Environment:** Procedural cloud generation with parallax scrolling and wind effects.
* **Interactive UI:** animated menus with "squish" effects on buttons and dynamic background wiggles.

## 🛠 Project Setup & Requirements

### 1. Tag Configuration

The scripts rely on specific Unity Tags to detect collisions. Ensure these are defined in the Tags & Layers settings:

* `Player`: Tag your main vehicle/carrier object.
* `Ground`: Tag the floor/terrain colliders.

### 2. Scene Hierarchy

To make the scripts function correctly, your scene should contain:

* **Game Manager:** An empty GameObject named `"Game Manager"` with the `GameManager.cs` attached.
* **Scene Manager:** An empty GameObject named `"Scene Manager"` with a scene management script attached.
* **UI:** A Canvas containing your coin counters, fuel sliders, and pause menus.

### 3. The "Vehicle" Rig

The Bayanihan carrier rig requires a specific setup for the physics to work:

1. **Main Body (Bamboo Poles):** `Rigidbody2D` + `BoxCollider2D`.
2. **Wheels (Carriers):** Two objects with `CircleCollider2D` + `Rigidbody2D` + `Wheel.cs`.
3. **Joints:** Connect Wheels to Body using `WheelJoint2D`.
4. **The House:** A visual object using `KeepUpright.cs` (Class `HouseFollower`) to sit on top of the poles.

## 📂 Script Documentation

### Core Systems

* **`GameManager.cs`**: The brain of the game. Manages fuel consumption, score (coins), and global game state (Pause/Resume).
* **`Death.cs`**: Detects if the player hits the "Ground" (i.e., the house tipped over) and triggers the game over sequence.
* **`BalanceCheck.cs`**: Monitors the Z-rotation of the carrier. Invokes `OnFall` event if the tilt exceeds the `maxTilt` angle.

### Vehicle & Physics

* **`Wheel.cs`**: Handles motor torque. Apply force when moving, braking torque when stopping, and emits dust particles when touching the ground.
* **`KeepUpright.cs`**: A cosmetic script that forces the Bahay Kubo to hover between two points (the carriers) while maintaining an upright rotation.
* **`FuelLevel.cs`**: Calculates the distance between the car and the next fuel pickup for UI display.

### Environment

* **`CloudSpawner.cs`**: Procedurally spawns cloud prefabs ahead of the camera at random heights and scales.
* **`CloudMover.cs`**: Moves clouds based on camera movement (Parallax) and a constant "Wind" value.
* **`ParallaxObject.cs`**: Generic script for background layers (mountains, trees) to move at different speeds relative to the camera.

### User Interface (UI)

* **`MenuManager.cs`**: Handles sliding panel transitions between Main Menu and Settings using `CanvasGroup`.
* **`MeterNeedle.cs`**: Visualizes player input (throttle) on a speedometer-style gauge.
* **`LowFuelIndicator.cs`**: Flashes a warning light/image when fuel drops below a specific percentage (default 20%).
* **`BackgroundWiggle.cs`**: Creates a floating/rotating effect for menu backgrounds. Includes a "Kick" function to react to button presses.
* **`ButtonPressAnimator.cs`**: Adds a "juicy" scale-down effect when clicking UI buttons.

### Collectibles

* **`Coin.cs`**: Adds currency to `PlayerPrefs` and destroys itself upon collision with the Player.
* **`PickupAnimation.cs`**: A visual effect script that makes items float up and fade away when collected.

---

*Generated based on source code provided.*
