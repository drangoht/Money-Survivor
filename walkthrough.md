# Money Survivor: Development Walkthrough

This document records the step-by-step process used to build and refine the game alongside the AI up to its current state.

---

## Phase 1: Foundational Layout

1. **Project Initialization:** Created the barebones Unity project and instantiated `GameSetup.cs`.
2. **Asset Generation:** Set up `GameSetup.cs` to write code-generated primitive textures, then transitioned to using AI-generated pixel-art sprites (Player CEO, Enemies, Coins).
3. **Basic Loop:** Implemented the player controller with a virtual camera follow script, floating enemies that track the player, and barebones OnGUI tracking.

---

## Phase 2: Solving "Bullet Heaven" Complexity

1. **Weapon Instantiation:** Implemented `LevelUpManager`. Initially faced errors attaching `Rigidbody2D` to weapons that weren’t meant to use them (e.g. Bill Whip, orbital-style weapons).
2. **Physics vs. Transforms:** Shield coins were stuck or jittering; fixed by disabling physics simulation where position is driven manually (e.g. orbitals).
3. **Visual Flare:** Bill Whip upgraded to spawn an array of sprites in a sweeping arc with `WhipVisualFader` for staggered fade-out.
4. **Item Scales:** Credit card, shield, and chests rescaled for visibility; chest size and cache invalidation handled in Editor setup.

---

## Phase 3: Menus and Thematic Overhaul

1. **Pause Feature:** Escape toggles `GameState.Paused` and `Time.timeScale` via `GameManager`.
2. **Comprehensive UI:** HUDController shows a run-summary pause menu with base stats and all equipped `WeaponBase` objects.
3. **Cyberpunk Finance Re-skin:** Splash screen, player sprite, and HUD styled to finance/CEO theme; background updated to office/cyberpunk look.
4. **Sprite Caching:** Addressed Unity `.asset` caching by forcing clean rebuilds of sprites and prefabs where needed.

---

## Phase 4: GitHub Ready

1. **Documentation:** README and markdown docs added.
2. **Transparency:** Prompt history, walkthrough, and implementation plan written and linked.
3. **Art Handling:** ReadmeImages folder used for display; game assets kept in main Sprites structure.

---

## Phase 5: Chest System & Expanded Item Pool

1. **Chest Rebalance:** Random `ChestSpawner` deprecated; bosses (IRS, CEO) drop a chest on death. Chest open triggers golden particle burst.
2. **Item Roster:** Cryptominer (stationary AOE rigs), Stock Options (volatile arrows), Insider Trading (XP gain), Tax Evasion (i-frames). Dividend Shield removed.

---

## Phase 6: Background, Obstacles & Bounds

1. **Simpler Background:** Replaced busy background with a flat colour and faint vertical stripes (editor-generated texture).
2. **Tiled Foreground & Parallax:** Foreground became a tiled pattern with rectangular “glass” windows (striped blue) revealing a second layer; underneath layer redesigned as blue sky with clouds (Perlin noise). Both layers scroll for parallax.
3. **Obstacles:** Office furniture (desks, chairs, walls) added as insurmountable obstacles: generated sprites in GameSetup, prefabs with `BoxCollider2D` (non-trigger), random placement in Game scene, parented to the foreground quad so they move with the ground.
4. **Player Bounds:** `PlayerController` gained `useMovementBounds`, `minBounds`, `maxBounds`; position clamped so the player cannot leave the play area. GameSetup enables bounds on the Player prefab and when updating existing prefab.
5. **Enemies vs. Obstacles:** Enemies given a second, non-trigger collider so they are blocked by office obstacles and cannot cross them.
6. **Foreground Scroll Sync:** So obstacles, XP orbs, and crypto miner rigs don’t slide on the ground, `ScrollingBackground` got `moveTransformWithScroll`: when true, the foreground quad’s transform is moved by the scroll delta instead of only the texture offset, and XP orbs and `LingeringAOE` instances are moved by the same delta.

---

## Phase 7: MegaBoss & Polish

1. **MegaBoss:** New boss spawning every 2 minutes: large (scale 2.8), 600 HP, 40 contact damage, 300 XP, drops chest. Uses same enemy pipeline (EnemyData, prefab, EnemySpawner timer).
2. **Boss Aura:** MegaBoss prefab given a child ParticleSystem (“BossAura”): local-space, red-tinted, sphere-shaped aura for visual emphasis.
3. **Bug Fix:** Resolved CS0106 (invalid `private` on “item”) by fixing a missing closing brace in `MakeEnemyPrefab` so `AddBossAuraParticles` is a class-level method, not a local function.
