# Money Survivor: Development Walkthrough

This document records the step-by-step process used to build and refine the game alongside the AI up to its current state.

## Phase 1: Foundational Layout
1.  **Project Initialization:** Created the barebones Unity project and immediately instantiated `GameSetup.cs`.
2.  **Asset Generation:** Set up `GameSetup.cs` to write completely code-generated primitive textures, and then transitioned to using AI-generated pixel-art sprites (Player CEO, Enemies, Coins).
3.  **Basic Loop:** Implemented the player controller with a virtual camera follow script, floating enemies that track the player, and barebones OnGUI tracking.

## Phase 2: Solving "Bullet Heaven" Complexity
1.  **Weapon Instantiation:** Implemented `LevelUpManager`. Initially faced errors attaching `Rigidbody2D` to weapons that weren't meant to use them (like the sweeping Bill Whip and the Orbiting Dividend Shield).
2.  **Physics vs. Transforms:** 
    *   *Issue:* Shield coins were stuck in place or jittering. 
    *   *Fix:* Disabled `rb.simulated = false` on instantiation for specific orbital weapons so `Update()` trigonometric loops weren't fighting Unity's physics engine.
3.  **Visual Flare:** The Whip originally spawned just a single ugly square. Upgraded it to spawn an array of boomerang sprites in a sweeping 180-degree arc using `WhipVisualFader` coroutines to stagger fading out.
4.  **Item Scales:** Re-scaled the credit card, dividend shield, and physical chests to be more visible. Adjusted the chest from 1.5x up to 2.5x manually through the Editor setup forcing cache invalidations.

## Phase 3: Menus and Thematic Overhaul
1.  **Pause Feature:** Added the ability to hook the `Escape` key inside the `GameManager` to toggle a `GameState.Paused` state, effectively stopping `Time.timeScale`.
2.  **Comprehensive UI:** Rewrote the `HUDController.cs` to display a detailed run-summary pause menu showing base stats (speed, pickup radius, HP) and iterating over all uniquely equipped `WeaponBase` objects the player collected.
3.  **Cyberpunk Finance Re-skin:** 
    *   Prompted the AI to generate a cool "Banker/CEO" Splash Screen using `generate_image`.
    *   Updated the `GameSetup.cs` parser to load this Image Texture into variables dynamically injected into `MainMenuUI`, `GameOverUI`, etc.
    *   Updated the player sprite to a standing pixel-art replica of the new splash screen artwork.
    *   Regenerated the procedural gray grid background into a seamless Cyberpunk Server Floor office tileset.
    *   Rewrote the `HUDController` primitive boxes to utilize faux-neon lighting shadows by rendering duplicated slightly larger rectangles with low alphas across the health and XP bars.
4.  **Sprite Caching Bugs:** Ran into several issues where the `GameSetup.Editor` wasn't properly updating the textures because of Unity's `.asset` caching pipeline. We utilized PowerShell commands inside the AI agent to brutally `Remove-Item` both `_Sprite.asset` and `_Tex.asset` hidden files alongside the prefabs folder to force pure, clean rebuilds.

## Phase 4: GitHub Ready
1.  **Documentation:** Wrote a dynamic `README.md` natively in the directory.
2.  **Transparency Pass:** The game relies on stripping `#FF00FF` magenta natively. This broke the README images. The AI wrote a PowerShell script to inject into `Assets/Sprites/*` to cleanly calculate the pixel alpha transparency for the GitHub markdown views.
3.  **Git Isolation:** Transparency broke the `GameSetup.cs` file parser! Fixed by reverting the original files, duplicating them into a `ReadmeImages/` folder purely for GitHub display, stripping the clones, and committing standard magenta sprites back into the game structure.
