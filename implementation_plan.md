# Money Survivor: Implementation Plan & Architecture

This document outlines the architectural decisions and implementation details for **Money Survivor**.

## 1. Zero-Friction Setup Architecture
The game relies entirely on `GameSetup.cs`, an Editor script that dynamically generates the entirety of the project structure from pure code.
*   **Why?** To prevent Unity Scene merge conflicts and prefab corruption during AI pair-programming.
*   **How?** By clicking `MoneySurvivor → Setup Entire Project`, the script auto-creates folders, generates pixel-art sprites into `Assets/Sprites`, spawns all ScriptableObjects, constructs Prefabs (Player, Weapons, Enemies, Menus), and saves them all into cleanly built `.unity` Scenes.

## 2. Core Game Loop & State Machine
*   **`GameManager.cs`**: A persistent Singleton (`DontDestroyOnLoad`) that tracks `TimeSurvived`, `EnemiesKilled`, and `CurrentLevel`. It manages the robust `GameState` enum (`MainMenu`, `Playing`, `Paused`, `LevelUp`, `GameOver`).
*   **`EventBus.cs`**: A static event hub. Instead of tightly coupling components, systems globally broadcast events (e.g., `OnPlayerDeath`, `OnEnemyKilled`, `OnPlayerHPChanged`). UI and managers listen to these events to react independently.

## 3. Weapon System
Weapons inherit from `WeaponBase.cs`, which manages an internal fire-rate clock derived from `WeaponData` ScriptableObjects.
*   **`SingleShot`**: Fires a basic projectile targeting the nearest enemy `transform`.
*   **`CoinToss`**: Spawns physics-based (`Rigidbody2D`) projectiles that bounce in an arc with gravity.
*   **`BoomerangWeapon`**: Spawns a `BoomerangProjectile` that lerps outward, stalls, and lerps back to the player.
*   **`BillWhip`**: Spawns multiple `BoomerangProjectile` visuals sequentially along a sweeping arc using `WhipVisualFader`, entirely disabling their physics components to keep them purely semantic.
*   **`CompoundInterest`**: Attaches an overlapping `CircleCollider2D` aura that ticks damage over time to enemies within range.
*   **`OrbitalWeapon`**: Spawns `OrbitalProjectile`s with their `Rigidbody2D` simulations disabled, manually setting their `.position` in `Update()` to calculate perfectly smooth trigonometric orbits around the player.

## 4. Enemies & Object Pooling
*   **`ObjectPool.cs`**: Standard queue-based dictionary pool for enemies.
*   **`EnemySpawner.cs`**: Manages scaling difficulty. It uses `TimeSurvived` to dynamically increase the spawn rate and limit the maximum number of active enemies on screen at once to maintain high frame rates.

## 5. Pure OnGUI System
To avoid complex Canvas hierarchies and RectTransform scaling issues through pure code, all floating UIs and menus are built entirely bypassing the Unity UI package relying strictly on the immediate-mode `OnGUI()` calls.
*   **`HUDController`**: Renders custom styled boxes and labels for HP, XP bars, active level, and the new Paused overlay.
*   **`MainMenuUI` / `GameOverUI` / `LevelUpUI`**: Draw transparent tinted backgrounds overlaying dynamically generated textures based on the pixel art splash screens.
