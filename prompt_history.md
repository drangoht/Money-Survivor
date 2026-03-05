# Money Survivor: Prompt History Backup

This document serves as a backup of the prompt history and user requests used to build **Money Survivor**. This can be fed into future AI-assisted development sessions to provide full context on how the game evolved.

## Initial Setup & Game Foundation
* **Prompt:** Create a "vampire survivor" style game in Unity called "Money-Survivor" using only standard Unity components and OnGUI for the UI. Do not use any external packages (no Unity UI, no Cinemachine). Generate all prefabs, scenes, and scriptable objects automatically from a single editor script.

### 1. Fixing Weapons & Core Gameplay
* **Prompt:** "i don't see coin toss and credit card weapon"
* **Prompt:** "credit card is not visible neither than shield, and whip display a square instead of a cool arc circle animated with effect"
* **Prompt:** "Assets\Editor\GameSetup.cs(761,80): error CS0103: The name '_booPrefab' does not exist in the current context"
* **Prompt:** "no credit card visible, and the coin orbiting for shield doesn't orbit around but spin"
* **Prompt:** "for the shield the cin doesn't orbit they stay at the same place and no whip visible"
* **Prompt:** "when getting shield: Can't remove Rigidbody2D because ProjectileBase (Script) depends on it... So raise the size of credit card it is very small. And i can't see the whip"
* **Prompt:** "MissingComponentException: There is no 'SpriteRenderer' attached to the "Card(Clone)" game object... when getting whip"
* **Prompt:** "the whip is not very visible make it more visible may by raise it or add some animation"
* **Prompt:** "add point to score only when killing enemies and not over the time, make the chest more large (half size of the player size)"
* **Prompt:** "the shield is again to small raise it size and redesign it"
* **Prompt:** "i think the chest is not well instanciated i see the old version at small size"
* **Prompt:** "you raise the size of dividend shield and not the chest found on the map revert it, and apply my request on the chest"
* **Prompt:** "Continue"

### 2. UI Overhaul & Menus
* **Prompt:** "add pause menu with stats about actually equipped object, speed, numbers of bullets, all stats related to this run, we can also go back to main menu from here"

### 3. Visual & Aesthetic Redesign (Cyberpunk Finance)
* **Prompt:** "add a splash screen with image inspired by sprite player, with some bills and coin. Redesign all the menus this way, with design part related to this gaming theme."
* **Prompt:** "redesign player sprite with inspiration from the cool character on splash screen background"
* **Prompt:** "the sprite did nt change on game"
* **Prompt:** "ok now redesign in game overlay (progress bar life and xp, score) inspired by this new design. redeisgn game background to represent financial company office too"
* **Prompt:** "the background did not change"

### 4. Git & Markdown Documentation
* **Prompt:** "create readme.MD for github, explaining how this game was made using Google gravity and a documentation of this game with screenshot based on assets"
* **Prompt:** "for readme use asset with transparent background inside of purple"
* **Prompt:** "copy the assert for readme with transparent backgrouund, it does not work now"
* **Prompt:** "write in markdown files all prompt history, walkthroughs and implementation plans"
* **Prompt:** "updtae readme.MD wtih link to prompt_history.md, implementation_plan.ms and walkthrough.md"
* **Prompt:** "so for now each time i prompt update the prompt_history.md file too"

### 5. Advanced Enemy Roster (Planned)
* **Prompt:** "Add different type of enemies with different sprites: Ex-wife, children, inspecteur des impots, fisc... Each type has is particular life point and animations and specific particule when hitted and die"
* **Prompt:** "save the implementation plan, so we will continue tomorrow"
