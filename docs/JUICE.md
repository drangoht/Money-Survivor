# Money Survivor – Juice & Polish

This document describes the "juicy" features added to improve game feel (inspired by survivor-style games like Vampire Survivors and Brotato).

## Light & Visual Effects

- **Player glow**: A soft golden halo sprite behind the player (added in Game Setup). Gives a subtle "hero" presence.
- **Enemy strength differentiation**: Enemies are tinted by difficulty. Higher tier and longer survival time make enemies shift toward a warmer (orange/red) tint so stronger foes are visually distinct at a glance.

## Particles

- **Hit particles**: Existing; spawn on enemy hit (color from `EnemyData.hitParticleColor`).
- **Death particles**: New burst (12–20 particles) when any enemy dies, using the same color as hit particles. Assigned via `EnemyBase.deathParticlePrefab` (GameSetup).
- **Level-up burst**: Golden particle burst at the player when the level-up screen appears (assigned to `LevelUpManager.levelUpBurstPrefab`).
- **Chest open**: Existing golden burst when opening chests.
- **Boss aura**: Existing particle aura on boss prefabs.

## Sound

- **Pitch variation**: All one-shot SFX use a random pitch between 0.92 and 1.08 to avoid repetition.
- **Boss/elite death**: Enemies with XP value ≥ 30 use a heavier, lower-pitched death sound (`CreateBossDieClip`).

## Background Music

- **MusicController**: Attached to the same GameObject as `GameManager` (in Main Menu scene). Plays a procedural 16-second looping BGM (soft pad + light arpeggio).
- **Start**: Music starts when the game starts (OnGameStart → load Game scene).
- **Stop**: Music stops on game over (OnGameOver) or when returning to main menu (StopMusic() in ReturnToMainMenu).

## Applying These Changes

Run **MoneySurvivor → Setup Entire Project** in the Unity menu to:

- Create/update Death and LevelUpBurst particle prefabs.
- Assign `deathParticlePrefab` on all enemy prefabs.
- Add player glow and level-up burst to the Game scene.
- Add `MusicController` to the GameManager object in the Main Menu scene.

Existing prefabs are updated with the new death particle reference when Setup runs.
