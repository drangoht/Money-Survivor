# Money Survivor 💸

**Money Survivor** is a fast-paced, top-down action roguelike ("bullet heaven") built entirely in Unity, themed around surviving a cyberpunk financial apocalypse. Play as a slick corporate CEO, use financially-themed weapons to fight off hordes of corrupt bankers and auditors, and maximize your Net Worth!

## 🤖 Built with AI Pair Programming
This entire game—including core gameplay loops, enemy AI, a complete weapon upgrade system, dynamic UI, office environments, and the pixel art assets themselves—was built through pair programming and prompt-driven development.

### 📚 Development History & Documentation
For full transparency and to document the AI pair-programming process, the complete history and architecture have been preserved:
- [Prompt History](prompt_history.md) – A full log of the exact user prompts used to build the game.
- [Implementation Plan](implementation_plan.md) – The core software architecture and engine decisions.
- [Development Walkthrough](walkthrough.md) – A step-by-step narrative of the problems encountered and solved during development.

The project structure is fundamentally unique: It avoids messy, unmergable Unity scenes and prefab metadata by using a custom **Editor Setup Script** (`GameSetup.cs`). By simply running a single menu command (**MoneySurvivor → Setup Entire Project**), the script dynamically generates all GameObjects, assigns all script logic, builds the necessary prefabs, bakes the scenes, and creates the ScriptableObjects that run the game database.

---

## 🎮 Controls & Input

| Input | Action |
|-------|--------|
| **WASD** / **Arrow keys** / **Left stick** | Move |
| **Escape** | **Open / close pause menu** (when playing or paused) |
| **Start (gamepad)** | Open / close pause menu |
| **Enter** / **Space** / **South button** | Confirm (menus, level-up cards) |
| **W / Up** / **S / Down** | Navigate main menu & level-up choices |
| **A / Left** / **D / Right** | Navigate pause menu (Resume / Menu) |

---

## ⚔️ Weapons (Wealth Acquisition)

You can hold **at most 3 weapons**. Each weapon can be upgraded up to **level 10**. New weapons and upgrades are offered on level-up (and sometimes in boss chests). Weapon icons appear in the HUD and on level-up cards.

| Weapon | Description |
|--------|-------------|
| **Aimed Bullet** | Fires a high-speed projectile at the nearest enemy. Reliable single-target DPS. |
| **Coin Toss** | Throws gold coins in all directions; damage and pierce. Overkill-style spread. |
| **Bill Whip** | Sweeps a massive arc of dollar bills, hitting all enemies in range. |
| **Compound Interest** | A persistent aura around the player that damages and lightly pushes nearby enemies. Grows with level. |
| **Credit Card** | Throws a piercing credit card that boomerangs back. High pierce, multiple cards at higher levels. |
| **Cryptominer** | Drops stationary mining rigs that burn enemies in an area over time. |
| **Stock Options** | Shoots volatile market arrows; damage randomizes significantly per hit. |

Weapon icons use a **square format** and **mauve (#E0B0FF) background** (stripped to transparency at import). Icons are in `Assets/Art/WeaponIcons/` and are assigned when you run **MoneySurvivor → Setup Entire Project** or **Assign Weapon Icons**.

---

## 👹 Enemies & Bosses

### Regular Enemies
| Type | Role | Notes |
|------|------|--------|
| **Bankman** | Basic | Low HP, moderate speed. |
| **ExWife** | Mid | Higher HP and damage. |
| **Children** | Swarm | Fast, low HP. |
| **Bouncer** | Tank | Slow, high HP. |

### Bosses
Bosses are larger, tougher, and **drop a chest** when killed. Enemy strength (HP, contact damage, speed) also scales with **elapsed time** (stronger every 2 minutes).

| Boss | Scale | HP | Contact damage | XP | Spawn rule |
|------|-------|-----|----------------|-----|------------|
| **IRS** | 1.8× | 600 | 55 | 120 | Every 3 min (then every 1.5 min after 15 min, every 10 s after 20 min). |
| **CEO** | 4× | 6000 | 70 | 1200 | **Once** at 10 min. |
| **MegaBoss** | 5.6× | 1500 | 90 | 700 | Every 2 min (then every 1 min after 15 min, every 10 s after 20 min). Red aura particles. |

---

## 🎁 Power-Ups (Chests & Level-Up)

Power-ups can appear as **level-up choices** (one of three cards) or as **chest rewards** when you open a boss chest. All use the same pool of effects below.

| Power-up | Effect |
|----------|--------|
| **Health Insurance** | Restores 30 HP. |
| **Gold Rush** | Increases move speed (+0.5). |
| **Hedge Fund** | +15% damage (multiplicative). |
| **Black Market** | Attract **all** XP orbs on the map to the player instantly (one-time). |
| **Diversified Portfolio** | +1 to XP pickup radius. |
| **Insider Trading** | +50% XP gained from orbs (multiplicative). |
| **Tax Evasion** | +0.5 s invincibility duration after taking damage. |
| **Overclock** | +25% projectile count for all weapons (multiplicative). |
| **Duplicate Projectile** | +100% projectile count (double projectiles per shot, multiplicative). |

---

## 📐 Gameplay & Rules

- **Movement:** Player is clamped to the play area (±95 units); you cannot leave the background.
- **Obstacles:** Office furniture (desks, chairs, walls) is placed randomly; player and enemies cannot walk through them.
- **Foreground sync:** Obstacles, XP orbs, and crypto miner rigs move with the scrolling foreground so they don’t slide.
- **Score (Net Worth):** Equals the number of enemies killed (no time-based score).
- **Level-up:** Collect XP orbs to fill the bar; on level-up the game pauses and you pick one of three cards (new weapon, weapon upgrade, or power-up). A one-time **Refresh choices** option can appear as a fourth card.
- **Chests:** Dropped by bosses. On open, a random reward (weapon / upgrade / power-up from the same pools) is applied, then a short notification is shown.
- **Difficulty:** Spawn rate and enemy mix escalate every 30 s (tier). Enemies get **stronger every 2 minutes** (HP, contact damage, and speed scale up). After **22 min**, regular spawn rate becomes **insane** (very fast).

---

## 💾 Persisted Save (Best Run Only)

- **What is saved:** Only your **best run** is stored: **time survived**, **kills**, and **score** (Net Worth = kills × 50).
- **When it is saved:** When you leave the run (Retry or Menu from the Game Over screen). The run is **only** written to disk if its **score is higher** than the previous best; otherwise nothing is updated.
- **Where it is shown:**  
  - **Main menu:** A line like *Best run: MM:SS | X kills | $score* appears below the buttons if a best run exists.  
  - **Game Over screen:** The same *Best run* line is shown above the Retry / Menu buttons when a best run exists.

Data is stored in **PlayerPrefs** (keys: `MoneySurvivor_BestTime`, `MoneySurvivor_BestKills`, `MoneySurvivor_BestScore`).

---

## 📸 Screenshots & Artwork

A showcase of the AI-generated pixel art and procedural assets.

### The CEO
The player character. A stylized banker in a sharp suit and sunglasses.

### The Opposition
Hordes of enemies representing the corrupted factions of the financial world (Bankmen, Ex-Wife, Children, Bouncers, IRS, CEO, MegaBoss).

### The Corporate Floor
- **Foreground:** Lighter tiled pattern with rectangular “glass” windows (striped blue) revealing the layer below.
- **Underneath:** Blue sky with clouds; both layers use parallax scrolling.

### Splash Screen & Main Menu
Custom splash and menus themed to the game.

---

## 📈 Systems

- **Dynamic scaling:** Spawn rate and difficulty tier increase over time; enemies scale every 2 minutes.
- **Event bus:** Decoupled events for game state, level-up, and UI.
- **Object pooling:** Enemies and particles use pooling for performance.
- **Juicy combat:** Hit flashing, screen shake, XP orbs, particle emitters on hit/death.
- **OnGUI UI:** HUD, **pause menu (Escape)**, level-up cards, game-over screen, and chest reward toast drawn with OnGUI.

---

## 🚀 How to Play (Developer Setup)

1. Clone or download the repository into a Unity project.
2. Open the Unity Editor.
3. In the menu bar, click **MoneySurvivor → Setup Entire Project**.
4. The editor script will generate Prefabs, Sprites, ScriptableObjects, and Scenes.
5. Open **Assets/Scenes/MainMenu.unity** (or **Game.unity**) and press **Play**.

Use **Escape** in-game to open and close the pause menu.

**Input:** The project uses the **legacy Input Manager** only (no new Input System package), so you won’t see the “native platform backends” prompt. Controls: WASD / arrows, Escape (pause), Enter/Space (confirm), gamepad via default axes (Horizontal, Vertical, Submit, Cancel).
