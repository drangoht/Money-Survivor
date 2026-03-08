# Money Survivor: Implementation Plan & Architecture

This document outlines the architectural decisions and implementation details for **Money Survivor**.

---

## 1. Zero-Friction Setup Architecture

The game relies entirely on **`GameSetup.cs`** (Editor script), which dynamically generates the project structure from code.

- **Why?** To avoid Unity scene merge conflicts and prefab corruption during iterative or AI-assisted development.
- **How?** Run **MoneySurvivor → Setup Entire Project**. The script:
  - Creates folders (`Assets/Prefabs`, `Assets/Prefabs/Enemies`, `Assets/Prefabs/Obstacles`, `Assets/Scenes`, `Assets/Sprites`, `Assets/ScriptableObjects`, etc.).
  - Generates or loads sprites (including procedural backgrounds and office obstacles).
  - Creates all ScriptableObjects (Enemies, Weapons, PowerUps).
  - Builds Prefabs (Player, Enemies, Weapons, Obstacles, XP Orb, Chest, Particles).
  - Builds and saves the MainMenu and Game scenes.
  - Adds both scenes to Build Settings.

---

## 2. Core Game Loop & State Machine

- **`GameManager.cs`**  
  Singleton (`DontDestroyOnLoad`) tracking `TimeSurvived`, `EnemiesKilled`, `CurrentLevel`. Manages `GameState`: `MainMenu`, `Playing`, `Paused`, `LevelUp`, `GameOver`. Handles Start Game, Pause, Resume, Retry, and Return to Menu.

- **`EventBus.cs`**  
  Static event hub. Systems broadcast events (`OnPlayerDeath`, `OnEnemyKilled`, `OnPlayerHPChanged`, `OnXPChanged`, `OnLevelUp`, `OnChestOpened`, etc.). UI and managers subscribe without hard dependencies.

---

## 3. Weapon System

Weapons inherit from **`WeaponBase.cs`**, which drives fire-rate and uses **`WeaponData`** ScriptableObjects with **`WeaponLevelStats`** per level.

| Weapon Script      | Behaviour |
|--------------------|-----------|
| **SingleShot**     | Fires a projectile at the nearest enemy. |
| **CoinToss**       | Spawns multiple coin projectiles in a radial pattern. |
| **BillWhip**       | Sweeps an arc of bill visuals, damaging enemies in range. |
| **CompoundInterest** | Persistent aura (CircleCollider2D) ticking damage. |
| **BoomerangWeapon**  | Credit card projectile that goes out and returns. |
| **CryptominerWeapon**| Drops stationary `LingeringAOE` rigs at intervals. |
| **StockOptionsWeapon** | Fires volatile projectiles with randomized damage. |

Damage is multiplied by **`PlayerStats.damageMultiplier`**. Level-up can add a new weapon or upgrade an existing one (next `WeaponLevelStats`).

---

## 4. Enemies & Spawning

- **`EnemyData`** (ScriptableObject)  
  Defines per-type: `hp`, `moveSpeed`, `contactDamage`, `xpValue`, `bodyColor`, `hitParticleColor`, `isBoss`, and scaling factors per difficulty tier.

- **`EnemyBase`**  
  Moves toward player, deals contact damage, takes damage from weapons, dies and drops XP orbs; bosses also drop a chest. Uses two colliders: one trigger (contact damage), one non-trigger (blocking vs obstacles).

- **`EnemySpawner`**  
  - **Regular waves:** Weighted mix of Bankman, ExWife, Children, Bouncer by difficulty tier (tier every 30 s; spawn interval shortens over time).
  - **IRS boss:** Every `bossInterval` (180 s). After 15 min: `bossIntervalLate` (90 s). After 20 min: `bossIntervalVeryLate` (10 s).
  - **CEO boss:** Once at `ceoSpawnTime` (600 s).
  - **MegaBoss:** Every `megaBossInterval` (120 s). After 15 min: `megaBossIntervalLate` (60 s). After 20 min: `bossIntervalVeryLate` (10 s). Huge (5.6× scale), 1500 HP, 90 contact damage, 700 XP, aura particles, drops chest.
  - **Thresholds:** `moreBossesAfterTime` = 900 s (15 min), `moreBossesAfterTime2` = 1200 s (20 min). **After 22 min:** regular spawn interval uses `insaneSpawnInterval` (very fast).

Spawn position is at `spawnRadius` from the player (off-screen). Difficulty tier is set on the spawned enemy for HP/speed scaling.

---

## 5. Player & Bounds

- **`PlayerController`**  
  WASD / arrow keys and gamepad; movement only when `GameState.Playing` and alive. Optional **movement bounds**: when `useMovementBounds` is true, position is clamped to `minBounds` / `maxBounds` (default ±95) so the player cannot leave the play area.

- **`PlayerStats`**  
  HP, move speed, damage multiplier, pickup radius, XP multiplier, invincibility duration, repel force. Handles damage, death (raises `OnPlayerDeath`), and application of power-up effects.

---

## 6. Backgrounds & Parallax

- **Two-layer background:**
  - **Far layer (BackgroundFar):** Quad with generated “blue sky + clouds” texture; scrolls via texture offset (`ScrollingBackground`, `moveTransformWithScroll = false`).
  - **Near layer (BackgroundNear):** Quad with generated tiled foreground (lighter tiles + rectangular “glass” windows). Uses **`moveTransformWithScroll = true`**: the quad’s **transform** is moved by the same scroll delta so that obstacles (parented to this quad) and the texture stay aligned.

- **`ScrollingBackground`**  
  When `moveTransformWithScroll` is true, it also moves all **XPOrb** and **LingeringAOE** (crypto miner) instances by the same delta so they don’t slide relative to the ground.

---

## 7. Obstacles (Office Furniture)

- **Prefabs:** OfficeDesk, OfficeChair, OfficeWall (generated in `GameSetup`: wood desk, dark chair, light gray wall textures).
- **Placement:** Random positions inside the play area (avoiding player spawn radius), parented to **BackgroundNear** so they move with the foreground. Some walls get random 0° or 90° rotation.
- **Collision:** `BoxCollider2D`, non-trigger, so player and enemies cannot pass through (insurmountable). Enemies use a second, non-trigger collider so they are blocked by obstacles.

---

## 8. XP, Level-Up & Power-Ups

- **`XPOrb`**  
  Collected when in pickup radius (with optional magnet behaviour). Calls **`XPManager.AddXP`** and is removed/pooled.

- **`XPManager`**  
  Tracks current XP and level; level-up threshold increases by curve. On level-up, triggers **`LevelUpManager`** and raises events.

- **`LevelUpManager`**  
  Pauses game, offers three choices (new weapon, weapon upgrade, or power-up from a configured list). A one-time **Refresh choices** option can appear as a fourth selectable card. Max 3 weapons, each weapon up to level 10. Then resumes.

- **Chests**  
  Dropped by bosses. On open: random **PowerUpData** applied, chest open animation/particles, reward UI.

---

## 9. UI (OnGUI)

To avoid Canvas/RectTransform complexity, all runtime UI uses **OnGUI**:

- **MainMenuUI** – Play / Quit.
- **HUDController** – HP bar, XP bar, level, timer, Net Worth (kill count), pause panel with run stats and weapon list.
- **LevelUpUI** – Three upgrade cards (keyboard/gamepad).
- **GameOverUI** – Shown on `OnPlayerDeath`; time survived, kills, level; optional **Previous run** (last run’s time, kills, score from PlayerPrefs); Retry / Menu.
- **ChestRewardUI** – Toast for chest reward.

---

## 10. Object Pooling

- **`ObjectPool.cs`**  
  Tag-based pools for enemies (and optionally other objects). Implements **IPoolable** (OnSpawn / OnDespawn). Used to avoid repeated Instantiate/Destroy during waves.

---

## 11. Boss-Specifics (IRS, CEO, MegaBoss)

- **IRS**  
  EnemyData: 600 HP, 0.9 speed, 55 contact damage, 120 XP, `isBoss = true`. Prefab scale 1.8. Drops chest on death.

- **CEO**  
  EnemyData: 6000 HP, 2.5 speed, 70 contact damage, 1200 XP, `isBoss = true`. Prefab scale 4.0. Drops chest on death.

- **MegaBoss**  
  EnemyData: 1500 HP, 0.5 speed, 90 contact damage, 700 XP, `isBoss = true`. Prefab scale 5.6; has a child **BossAura** ParticleSystem (local space, red-tinted, sphere shape) for a persistent aura effect. Drops chest on death like IRS and CEO.
