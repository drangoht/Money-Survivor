#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// One-click project setup — MoneySurvivor → Setup Entire Project
/// Requires NO external packages. Uses only UnityEngine core.
/// </summary>
public static class GameSetup
{
    private const string PrefabsPath   = "Assets/Prefabs";
    private const string EnemyPath     = "Assets/Prefabs/Enemies";
    private const string ScenesPath    = "Assets/Scenes";
    private const string SOPath        = "Assets/ScriptableObjects";
    private const string EnemySOPath   = "Assets/ScriptableObjects/Enemies";
    private const string WeaponSOPath  = "Assets/ScriptableObjects/Weapons";
    private const string PowerUpSOPath = "Assets/ScriptableObjects/PowerUps";
    private const string SpritesPath   = "Assets/Sprites";

    // ── Entry point ──────────────────────────────────────────────────────────

    [MenuItem("MoneySurvivor/Setup Entire Project")]
    public static void SetupProject()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Setup Error", "You cannot run the project setup while the game is playing. Please stop Play Mode and try again.", "OK");
            return;
        }

        try
        {
            Log("=== Money Survivor Setup START ===");

            Step("Creating folders",      EnsureFolders);
            AssetDatabase.Refresh();  // CRITICAL: flush folder creation before saving assets

            Step("Generating sprites",    GenerateSprites);
            AssetDatabase.Refresh();

            Step("Creating ScriptableObjects", CreateScriptableObjects);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Step("Creating prefabs",      CreatePrefabs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Step("Creating MainMenu scene", CreateMainMenuScene);
            Step("Creating Game scene",     CreateGameScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Step("Adding scenes to Build Settings", AddScenesToBuildSettings);

            Log("=== Money Survivor Setup COMPLETE ===");
            EditorUtility.DisplayDialog("Money Survivor",
                "Setup complete!\n\nOpen Assets/Scenes/Game.unity and press Play.",
                "Let's go!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameSetup] FAILED: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Setup Failed",
                $"An error occurred:\n{e.Message}\n\nSee Console for details.", "OK");
        }
    }

    private static void Step(string name, Action action)
    {
        Log($"Step: {name}");
        action();
        Log($"Step done: {name}");
    }

    private static void Log(string msg) => Debug.Log($"[GameSetup] {msg}");

    // ══════════════════════════════════════════════════════════════════════════
    // FOLDERS
    // ══════════════════════════════════════════════════════════════════════════

    private static void EnsureFolders()
    {
        // Order matters: parent must exist before child
        string[] dirs =
        {
            ScenesPath, SpritesPath, PrefabsPath, EnemyPath,
            SOPath, EnemySOPath, WeaponSOPath, PowerUpSOPath,
        };

        foreach (var dir in dirs)
        {
            if (AssetDatabase.IsValidFolder(dir)) { Log($"  Folder exists: {dir}"); continue; }

            string parent = dir.Substring(0, dir.LastIndexOf('/'));
            string child  = dir.Substring(dir.LastIndexOf('/') + 1);

            string guid = AssetDatabase.CreateFolder(parent, child);
            if (string.IsNullOrEmpty(guid))
                throw new Exception($"Failed to create folder: {dir}");

            Log($"  Created folder: {dir}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SPRITES
    // ══════════════════════════════════════════════════════════════════════════

    private static Sprite _circle, _square;
    private static Sprite _playerSprite, _enemySprite, _coinSprite, _bgSpriteFront, _bgSpriteBack;
    private static Sprite _xpOrbSprite, _boomerangSprite, _shieldSprite, _splashSprite, _chestSprite;
    private static Sprite _exWifeSprite, _childrenSprite, _irsSprite, _cryptoSprite, _stockSprite;

    private static void GenerateSprites()
    {
        _circle = GetOrCreateSprite(SpritesPath + "/Circle.png", CreateCircleTex(128));
        _square = GetOrCreateSprite(SpritesPath + "/Square.png",  CreateSolidTex(32, Color.white));

        // Parallax backgrounds: front (tiled with holes) and far (soft gradient).
        _bgSpriteFront = GetOrCreateSprite(
            SpritesPath + "/scrolling_background.png",
            CreateTiledForegroundBackgroundTex(1024, 1024),
            forceRegenerate: true);
        _bgSpriteBack = GetOrCreateSprite(
            SpritesPath + "/scrolling_background_far.png",
            CreateFarParallaxBackgroundTex(1024, 1024),
            forceRegenerate: true);
        
        _playerSprite    = LoadSingleSprite(SpritesPath + "/player_sprite.png",         "player");
        _enemySprite     = LoadSingleSprite(SpritesPath + "/enemy_sprite.png",          "enemy");
        _exWifeSprite    = LoadSingleSprite(SpritesPath + "/ex_wife_sprite.png",        "ex_wife");
        _childrenSprite  = LoadSingleSprite(SpritesPath + "/children_sprite.png",       "children");
        _irsSprite       = LoadSingleSprite(SpritesPath + "/irs_sprite.png",            "irs");
        _coinSprite      = LoadSingleSprite(SpritesPath + "/coin_sprite.png",           "coin");
        _xpOrbSprite     = LoadSingleSprite(SpritesPath + "/xp_orb_sprite.png",        "xporb");
        _boomerangSprite = LoadSingleSprite(SpritesPath + "/boomerang_sprite.png",      "boomerang");
        _shieldSprite    = LoadSingleSprite(SpritesPath + "/shield_sprite.png",         "shield");
        _splashSprite    = LoadSingleSprite(SpritesPath + "/splash_screen.png",         "splash");
        _chestSprite     = LoadSingleSprite(SpritesPath + "/chest_sprite_1772809376512.png", "chest");
        _cryptoSprite    = LoadSingleSprite(SpritesPath + "/cryptominer_sprite_1772827344346.png", "cryptominer_V2");
        _stockSprite     = LoadSingleSprite(SpritesPath + "/stock_options_sprite_1772827497492.png", "stock_options_V2");

        Log($"  Sprites ready");
    }

    // Load single PNG as one Sprite (strips background)
    private static Sprite LoadSingleSprite(string path, string baseName)
    {
        if (!File.Exists(path)) return _square;
        string assetPath = SpritesPath + "/" + baseName + "_Sprite.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existing != null) return existing;

        Texture2D tex = LoadTexFromDisk(path);
        StripBackground(tex);

        string texPath = SpritesPath + "/" + baseName + "_Tex.asset";
        AssetDatabase.CreateAsset(tex, texPath);
        Sprite sp = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f), 250f);
        sp.name = baseName;
        AssetDatabase.CreateAsset(sp, assetPath);
        AssetDatabase.SaveAssets();
        return sp;
    }

    // Load PNG and slice into cols×rows grid of frames, reading from top-left in image order
    private static Sprite[] LoadSpriteSheetFrames(string path, int cols, int rows, string baseName)
    {
        if (!File.Exists(path)) return new[] { _square };

        Texture2D full = LoadTexFromDisk(path);
        int fw = full.width  / cols;
        int fh = full.height / rows;
        int totalFrames = cols * rows;
        Log($"  Slicing {baseName}: {full.width}x{full.height} → {cols}x{rows} grid, frame={fw}x{fh}, total={totalFrames} frames");
        StripBackground(full);

        var frames = new Sprite[totalFrames];
        int frameIdx = 0;

        // Read top-row first (in image coords = highest Y in Unity coords)
        for (int row = rows - 1; row >= 0; row--)
        {
            for (int col = 0; col < cols; col++)
            {
                string assetPath = SpritesPath + "/" + baseName + "_f" + frameIdx + "_Sprite.asset";
                var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (existing != null) { frames[frameIdx++] = existing; continue; }

                int x = col * fw;
                int y = row * fh; // Unity y=0 is bottom, so row 0 = bottom of image

                Texture2D frameTex = new Texture2D(fw, fh, TextureFormat.RGBA32, false);
                frameTex.SetPixels(full.GetPixels(x, y, fw, fh));
                frameTex.filterMode = FilterMode.Point;
                frameTex.wrapMode = TextureWrapMode.Clamp;
                frameTex.Apply();

                string texPath = SpritesPath + "/" + baseName + "_f" + frameIdx + "_Tex.asset";
                AssetDatabase.CreateAsset(frameTex, texPath);

                // PPU=128: 320px frame → 2.5 Unity units. Good readable size.
                Sprite sp = Sprite.Create(frameTex, new Rect(0, 0, fw, fh), new Vector2(0.5f, 0.5f), 128f);
                sp.name = baseName + "_" + frameIdx;
                AssetDatabase.CreateAsset(sp, assetPath);
                frames[frameIdx++] = sp;
            }
        }

        AssetDatabase.SaveAssets();
        return frames;
    }

    private static Texture2D LoadTexFromDisk(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(bytes);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    // Color-key background removal: iterative BFS flood fill from all 4 corners
    private static void StripBackground(Texture2D tex)
    {
        int w = tex.width, h = tex.height;
        Color[] pixels = tex.GetPixels();
        bool[] visited = new bool[w * h];

        // Use a pixel slightly inset from the top-left as the background key
        // (AI generated images sometimes have a 1-pixel colored border)
        Color key = pixels[(h - 5) * w + 5]; 

        float threshold = 0.22f; // carefully tuned to avoid eating dark neon sprites

        var queue = new System.Collections.Generic.Queue<int>();

        System.Action<int> enqueue = (idx) => {
            if (idx < 0 || idx >= pixels.Length || visited[idx]) return;
            Color c = pixels[idx];
            float dist = Mathf.Abs(c.r - key.r)
                       + Mathf.Abs(c.g - key.g)
                       + Mathf.Abs(c.b - key.b);
            if (dist > threshold) return;
            visited[idx] = true;
            queue.Enqueue(idx);
        };

        // Seed from ALL border pixels (more robust than just 4 corners)
        for (int bx = 0; bx < w; bx++) { enqueue(bx); enqueue((h-1)*w + bx); }   // top & bottom rows
        for (int by = 0; by < h; by++) { enqueue(by * w); enqueue(by * w + w-1); } // left & right cols

        while (queue.Count > 0)
        {
            int idx = queue.Dequeue();
            pixels[idx] = Color.clear;
            int x = idx % w, y = idx / w;
            if (x > 0)   enqueue(idx - 1);
            if (x < w-1) enqueue(idx + 1);
            if (y > 0)   enqueue(idx - w);
            if (y < h-1) enqueue(idx + w);
        }

        // --- Hard edge cleanup pass (pixel art: no semi-transparency) ---
        // Clear any background-adjacent pixels that are close to the background color.
        float edgeThreshold = 0.45f;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (visited[i]) continue;
            int ex = i % w, ey = i / w;
            bool nearCleared =
                (ex > 0   && visited[i - 1]) ||
                (ex < w-1 && visited[i + 1]) ||
                (ey > 0   && visited[i - w]) ||
                (ey < h-1 && visited[i + w]);
            if (!nearCleared) continue;

            float dist = Mathf.Abs(pixels[i].r - key.r)
                       + Mathf.Abs(pixels[i].g - key.g)
                       + Mathf.Abs(pixels[i].b - key.b);
            if (dist < edgeThreshold)
                pixels[i] = Color.clear; // hard cut — no blending for pixel art
        }

        // --- Global magenta removal pass (catches any remaining #FF00FF background pixels) ---
        Color magenta = new Color(1f, 0f, 1f);
        for (int i = 0; i < pixels.Length; i++)
        {
            float magentaDist = Mathf.Abs(pixels[i].r - magenta.r)
                              + Mathf.Abs(pixels[i].g - magenta.g)
                              + Mathf.Abs(pixels[i].b - magenta.b);
            if (magentaDist < 0.4f)
                pixels[i] = Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
    }

    private static Sprite GetOrCreateSprite(string path, Texture2D generated, bool forceRegenerate = false)
    {
        string texPath = path;
        string spritePath = path.Replace(".png", "_Sprite.asset");

        // 1. Ensure Texture exists
        if (!File.Exists(texPath) || forceRegenerate)
        {
            File.WriteAllBytes(texPath, generated.EncodeToPNG());
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite; // Still good for UI rendering
                importer.spritePixelsPerUnit = 100f;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }

        // 2. Load the Texture
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (tex == null) throw new Exception($"Could not load texture at {texPath}");

        // 3. Ensure a standalone Sprite Asset exists pointing to it
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(sprite, spritePath);
            AssetDatabase.SaveAssets();
        }

        return sprite;
    }

    private static Texture2D CreateCircleTex(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            tex.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), center) <= radius
                ? Color.white : Color.clear);
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateTiledForegroundBackgroundTex(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Lighter base tiles.
        Color baseCol    = new Color(0.22f, 0.30f, 0.45f);       // ~#384C73
        Color borderCol  = new Color(0.26f, 0.36f, 0.52f);       // ~#435C85
        // Glass window fill and stripe colors with alpha so the far background is clearly visible.
        Color glassCol        = new Color(0.78f, 0.88f, 1.0f, 0.32f); // light blue, softer opacity
        Color glassStripeCol  = new Color(0.82f, 0.92f, 1.0f, 0.55f); // brighter stripes
        Color glassFrameCol   = new Color(0.70f, 0.82f, 1.0f, 0.75f); // solid frame around window

        const int tileSize        = 128;
        const int borderThickness = 2;
        const int windowMargin    = 12;  // inset from tile borders (smaller margin → larger window)
        const int windowStride    = 3;   // every Nth tile gets a window
        const int windowFrameThickness = 3;
        const int stripeWidth     = 3;
        const int stripeSpacing   = 10;  // distance between stripes inside window

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int tileX = x / tileSize;
                int tileY = y / tileSize;
                int lx    = x % tileSize;
                int ly    = y % tileSize;

                bool onVerticalBorder   = lx < borderThickness;
                bool onHorizontalBorder = ly < borderThickness;
                bool isBorder           = onVerticalBorder || onHorizontalBorder;

                // Decide whether this tile should have a glass "window".
                bool hasWindow = (tileX % windowStride == 1) && (tileY % windowStride == 1);
                bool insideWindow = lx >= windowMargin && lx < tileSize - windowMargin
                                  && ly >= windowMargin && ly < tileSize - windowMargin;

                if (hasWindow && insideWindow)
                {
                    int innerWidth  = tileSize - 2 * windowMargin;
                    int innerHeight = tileSize - 2 * windowMargin;
                    int wx = lx - windowMargin;
                    int wy = ly - windowMargin;

                    bool isFrame =
                        wx < windowFrameThickness ||
                        wy < windowFrameThickness ||
                        wx >= innerWidth  - windowFrameThickness ||
                        wy >= innerHeight - windowFrameThickness;

                    if (isFrame)
                    {
                        // Solid glass frame to outline the window.
                        tex.SetPixel(x, y, glassFrameCol);
                    }
                    else
                    {
                        // Interior: vertical stripes to suggest a glass panel.
                        bool isStripe = (wx % stripeSpacing) < stripeWidth;
                        tex.SetPixel(x, y, isStripe ? glassStripeCol : glassCol);
                    }
                }
                else
                {
                    tex.SetPixel(x, y, isBorder ? borderCol : baseCol);
                }
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Repeat;
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateFarParallaxBackgroundTex(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Blue sky with bright clouds for the far parallax layer.
        Color skyTopCol    = new Color(0.28f, 0.55f, 0.95f); // ~#4890F2, deep blue
        Color skyBottomCol = new Color(0.70f, 0.85f, 1.00f); // ~#B3D9FF, light near-horizon blue
        Color cloudCol     = new Color(0.97f, 0.98f, 1.00f); // almost white clouds

        // Perlin noise parameters to generate soft cloud patches across the sky.
        float noiseScale     = 0.010f;
        float cloudThreshold = 0.48f;
        float cloudIntensity = 0.80f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Vertical sky gradient that loops cleanly when tiled (top and bottom match).
                float vNorm = (float)y / (height - 1);
                float vLoop = Mathf.Abs(vNorm * 2f - 1f); // 0 at middle, 1 at top & bottom
                Color baseColGrad = Color.Lerp(skyBottomCol, skyTopCol, vLoop);

                // Cloud pattern overlay using Perlin noise.
                float n = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
                if (n > cloudThreshold)
                {
                    float cloudT = (n - cloudThreshold) / (1f - cloudThreshold);
                    cloudT = Mathf.Clamp01(cloudT);
                    // Blend towards cloud color to form soft, bright patches.
                    baseColGrad = Color.Lerp(baseColGrad, cloudCol, cloudT * cloudIntensity);
                }

                Color c = baseColGrad;
                tex.SetPixel(x, y, c);
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Repeat;
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateSolidTex(int size, Color col)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int i = 0; i < size * size; i++) tex.SetPixel(i % size, i / size, col);
        tex.Apply();
        return tex;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SCRIPTABLE OBJECTS
    // ══════════════════════════════════════════════════════════════════════════

    private static EnemyData  _bankman, _exWife, _children, _irs, _bouncer, _ceo;
    private static WeaponData _coinData, _whipData,  _auraData, _cardData, _singleShotData, _cryptoData, _stockData;
    private static PowerUpData _healPU, _speedPU, _damagePU, _magnetPU, _radiusPU, _repelPU, _insiderPU, _taxPU;

    private static void CreateScriptableObjects()
    {
        _bankman  = MakeEnemy("Bankman",  new Color(.2f,.4f,.9f),  30f, 1.5f,  5f, 10,  1.1f, 1.05f, new Color(.2f,.4f,.9f));
        _exWife   = MakeEnemy("ExWife",   new Color(.8f,.2f,.8f),  60f, 1.2f, 15f, 25,  1.15f,1.02f, new Color(.8f,.2f,.8f));
        _children = MakeEnemy("Children", new Color(1f, 1f, 0f),   15f, 2.5f,  3f, 5,   1.05f,1.08f, new Color(1f, 1f, 0f));
        _irs      = MakeEnemy("IRS",      new Color(.1f,.8f,.2f), 250f, 0.9f, 25f, 50,  1.2f, 1.01f, new Color(.1f,.8f,.2f));
        _bouncer  = MakeEnemy("Bouncer",  new Color(.3f,.3f,.3f), 150f, 0.6f, 15f, 40,  1.1f, 1.02f, new Color(.3f,.3f,.3f));
        _ceo      = MakeEnemy("CEO",      new Color(1f,.9f,.1f), 2500f, 2.5f, 30f, 500, 1.02f,1.01f, new Color(1f,.9f,.1f));

        _singleShotData = MakeWeapon("SingleShotData", "Aimed Bullet",
            "Fires a projectile directly at the nearest enemy.",
            new WeaponLevelStats[]
            {
                new() { damage=15, fireRate=0.8f, projectileSpeed=15, projectileCount=1, pierceCount=1, duration=3f },
                new() { damage=18, fireRate=0.9f, projectileSpeed=15, projectileCount=2, pierceCount=1, duration=3f },
                new() { damage=22, fireRate=1.0f, projectileSpeed=16, projectileCount=2, pierceCount=2, duration=3f },
                new() { damage=28, fireRate=1.1f, projectileSpeed=16, projectileCount=3, pierceCount=2, duration=3f },
                new() { damage=35, fireRate=1.2f, projectileSpeed=17, projectileCount=4, pierceCount=3, duration=3f },
            });

        _coinData = MakeWeapon("CoinTossData", "Coin Toss",
            "Overkill: Throws gold coins in all directions.",
            new WeaponLevelStats[]
            {
                new() { damage=10, fireRate=1f,   projectileSpeed=8, projectileCount=4, pierceCount=1, duration=3f },
                new() { damage=14, fireRate=1.1f, projectileSpeed=8, projectileCount=6, pierceCount=1, duration=3f },
                new() { damage=18, fireRate=1.2f, projectileSpeed=9, projectileCount=6, pierceCount=2, duration=3f },
                new() { damage=24, fireRate=1.3f, projectileSpeed=9, projectileCount=8, pierceCount=2, duration=3.5f},
                new() { damage=32, fireRate=1.5f, projectileSpeed=10,projectileCount=8, pierceCount=3, duration=4f },
            });

        _whipData = MakeWeapon("BillWhipData", "Bill Whip",
            "Sweeps an arc of bills, hitting all nearby enemies.",
            new WeaponLevelStats[]
            {
                new() { damage=20, fireRate=0.8f, aoeRadius=2.5f },
                new() { damage=28, fireRate=0.9f, aoeRadius=3.0f },
                new() { damage=36, fireRate=1.0f, aoeRadius=3.5f },
                new() { damage=46, fireRate=1.1f, aoeRadius=4.0f },
                new() { damage=60, fireRate=1.2f, aoeRadius=5.0f },
            });

        _auraData = MakeWeapon("CompoundInterestData", "Compound Interest",
            "A growing aura that deals constant damage to nearby enemies.",
            new WeaponLevelStats[]
            {
                new() { damage=5,  fireRate=2f, aoeRadius=2.0f },
                new() { damage=7,  fireRate=2f, aoeRadius=2.5f },
                new() { damage=10, fireRate=2f, aoeRadius=3.0f },
                new() { damage=14, fireRate=2f, aoeRadius=3.5f },
                new() { damage=20, fireRate=2f, aoeRadius=4.5f },
            });

        _cardData = MakeWeapon("CreditCardData", "Credit Card",
            "Throws a piercing credit card that boomerangs back to you.",
            new WeaponLevelStats[]
            {
                new() { damage=15, fireRate=1.5f, projectileSpeed=12, projectileCount=1, pierceCount=3, duration=2f },
                new() { damage=20, fireRate=1.4f, projectileSpeed=14, projectileCount=2, pierceCount=4, duration=2f },
                new() { damage=25, fireRate=1.3f, projectileSpeed=14, projectileCount=3, pierceCount=4, duration=2f },
                new() { damage=35, fireRate=1.2f, projectileSpeed=16, projectileCount=3, pierceCount=99, duration=2.5f },
                new() { damage=50, fireRate=1.0f, projectileSpeed=18, projectileCount=4, pierceCount=99, duration=2.5f },
            });

        string cryptoPath = WeaponSOPath + "/CryptominerData.asset";
        if (AssetDatabase.LoadAssetAtPath<WeaponData>(cryptoPath) != null) AssetDatabase.DeleteAsset(cryptoPath);
        _cryptoData = MakeWeapon("CryptominerData", "Cryptominer",
            "Drops a stationary mining rig that burns nearby enemies.",
            new WeaponLevelStats[]
            {
                new() { damage=15, fireRate=0.25f, aoeRadius=2.5f, projectileCount=1, duration=3f }, // 1 every 4.0 sec
                new() { damage=25, fireRate=0.28f, aoeRadius=2.8f, projectileCount=1, duration=3.5f },// 1 every 3.5 sec
                new() { damage=40, fireRate=0.33f, aoeRadius=3.2f, projectileCount=2, duration=4f }, // 1 every 3.0 sec
                new() { damage=65, fireRate=0.40f, aoeRadius=3.6f, projectileCount=2, duration=4.5f },// 1 every 2.5 sec
                new() { damage=100, fireRate=0.50f, aoeRadius=4.0f, projectileCount=3, duration=5f },// 1 every 2.0 sec
            });

        string stockPath = WeaponSOPath + "/StockOptionsData.asset";
        if (AssetDatabase.LoadAssetAtPath<WeaponData>(stockPath) != null) AssetDatabase.DeleteAsset(stockPath);
        _stockData = MakeWeapon("StockOptionsData", "Stock Options",
            "Shoots volatile market arrows. Damage randomizes significantly per hit.",
            new WeaponLevelStats[]
            {
                new() { damage=18, fireRate=1.2f, projectileSpeed=15, projectileCount=2, pierceCount=2, duration=2f },
                new() { damage=24, fireRate=1.1f, projectileSpeed=16, projectileCount=3, pierceCount=2, duration=2f },
                new() { damage=32, fireRate=1.0f, projectileSpeed=17, projectileCount=3, pierceCount=3, duration=2f },
                new() { damage=45, fireRate=0.9f, projectileSpeed=18, projectileCount=4, pierceCount=3, duration=2.5f },
                new() { damage=60, fireRate=0.8f, projectileSpeed=20, projectileCount=5, pierceCount=4, duration=2.5f },
            });

        _repelPU  = MakePU("RestrainingOrder", "Restraining Order", "Weapons push enemies further away.",
                        PowerUpEffectType.RepelEnemies, 2f, new Color(0.9f, 0.4f, 0.8f));
        _insiderPU = MakePU("InsiderTrading", "Insider Trading", "Gain 50% more XP from all orbs collected.",
                        PowerUpEffectType.IncreaseXPGain, 50f, new Color(0.1f, 0.9f, 0.3f));
        _taxPU = MakePU("TaxEvasion", "Tax Evasion", "Increases your invincibility time after taking damage.",
                        PowerUpEffectType.IncreaseIFrames, 0.5f, new Color(0.5f, 0.5f, 0.9f));

        Log($"  All ScriptableObjects created");
    }

    private static EnemyData MakeEnemy(string id, Color col, float hp, float spd,
        float dmg, int xp, float hpS, float sS, Color hitColor, bool isBoss = false)
    {
        string path = EnemySOPath + "/" + id + ".asset";
        var ex = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (ex != null) { Log($"  Enemy exists: {id}"); return ex; }
        var d = ScriptableObject.CreateInstance<EnemyData>();
        d.enemyName = id; d.isBoss = isBoss; d.bodyColor = col; d.hp = hp; d.moveSpeed = spd;
        d.contactDamage = dmg; d.xpValue = xp; d.hpScaleFactor = hpS; d.speedScaleFactor = sS;
        d.hitParticleColor = hitColor;
        AssetDatabase.CreateAsset(d, path);
        Log($"  Enemy created: {id}");
        return d;
    }

    private static WeaponData MakeWeapon(string id, string label, string desc, WeaponLevelStats[] levels)
    {
        string path = WeaponSOPath + "/" + id + ".asset";
        var ex = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
        if (ex != null) { Log($"  Weapon exists: {id}"); return ex; }
        var d = ScriptableObject.CreateInstance<WeaponData>();
        d.weaponName = label; d.description = desc; d.levels = levels;
        AssetDatabase.CreateAsset(d, path);
        Log($"  Weapon created: {id}");
        return d;
    }

    private static PowerUpData MakePU(string id, string label, string desc,
        PowerUpEffectType eff, float val, Color col)
    {
        string path = PowerUpSOPath + "/" + id + ".asset";
        var ex = AssetDatabase.LoadAssetAtPath<PowerUpData>(path);
        if (ex != null) { Log($"  PowerUp exists: {id}"); return ex; }
        var d = ScriptableObject.CreateInstance<PowerUpData>();
        d.powerUpName = label; d.description = desc; d.effectType = eff; d.value = val; d.cardColor = col;
        AssetDatabase.CreateAsset(d, path);
        Log($"  PowerUp created: {id}");
        return d;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PREFABS
    // ══════════════════════════════════════════════════════════════════════════

    private static GameObject _playerPrefab, _coinPrefab, _orbPrefab, _chestPrefab;
    private static GameObject _cardPrefab, _cryptoPrefab, _stockPrefab, _hitParticlesPrefab;
    private static GameObject _p1, _p2, _p3, _p4, _p5, _p6; // enemy prefabs

    private static void CreatePrefabs()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath + "/Cryptominer.prefab") != null) AssetDatabase.DeleteAsset(PrefabsPath + "/Cryptominer.prefab");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath + "/StockOptionsProjectile.prefab") != null) AssetDatabase.DeleteAsset(PrefabsPath + "/StockOptionsProjectile.prefab");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath + "/Cryptominer.prefab") != null) AssetDatabase.DeleteAsset(PrefabsPath + "/Cryptominer.prefab");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath + "/StockOptionsProjectile.prefab") != null) AssetDatabase.DeleteAsset(PrefabsPath + "/StockOptionsProjectile.prefab");

        _coinPrefab         = MakeProjectile("Coin.prefab", _coinSprite, Color.white, typeof(ProjectileBase));
        _cardPrefab         = MakeProjectile("Card.prefab", _square, new Color(.2f,.8f,.9f), typeof(BoomerangProjectile), 0.6f); // Larger card
        _cryptoPrefab       = MakeLingeringAOEPrefab("Cryptominer.prefab", _cryptoSprite, 0.40f);
        _stockPrefab        = MakeProjectile("StockOptionsProjectile.prefab", _stockSprite, Color.white, typeof(VolatileProjectile), 0.25f);
        _orbPrefab          = MakeOrb();
        _chestPrefab        = MakeChest();
        _hitParticlesPrefab = MakeHitParticles();
        _playerPrefab       = MakePlayer();
        
        _p1 = MakeEnemyPrefab("Bankman",  _bankman,  _enemySprite,   0.7f, 0.03f, 5f);
        _p2 = MakeEnemyPrefab("ExWife",   _exWife,   _exWifeSprite,  0.8f, 0.04f, 4f);
        _p3 = MakeEnemyPrefab("Children", _children, _childrenSprite,0.5f, 0.06f, 8f);
        _p4 = MakeEnemyPrefab("IRS",      _irs,      _irsSprite,     0.9f, 0.02f, 3f);
        _p5 = MakeEnemyPrefab("Bouncer",  _bouncer,  _enemySprite,   1.2f, 0.03f, 4f);
        _p6 = MakeEnemyPrefab("CEO",      _ceo,      _enemySprite,   2.0f, 0.03f, 4f);

        // Update the SOs with the boss flag
        _irs.isBoss = true;
        _ceo.isBoss = true;
        UnityEditor.EditorUtility.SetDirty(_irs);
        UnityEditor.EditorUtility.SetDirty(_ceo);
        
        Log("  All prefabs created");
    }

    private static GameObject MakePlayer()
    {
        string path = PrefabsPath + "/Player.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null)
        {
            // Ensure movement bounds are configured even for an existing prefab.
            var existingController = ex.GetComponent<PlayerController>();
            if (existingController != null)
            {
                existingController.useMovementBounds = true;
                existingController.minBounds = new Vector2(-95f, -95f);
                existingController.maxBounds = new Vector2( 95f,  95f);
            }
            Log("  Player exists (movement bounds updated)");
            return ex;
        }

        var root = new GameObject("Player");
        root.tag = "Player";
        var spriteChild = Sprite2D(root, _playerSprite, Color.white, 2, 1f);
        // Bobbing animation + directional flip (uses Input.GetAxisRaw internally)
        var bobber = spriteChild.gameObject.AddComponent<SpriteBobber>();
        bobber.bobAmount = 0.04f;
        bobber.bobSpeed  = 6f;

        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var col = root.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        root.AddComponent<PlayerStats>();
        var controller = root.AddComponent<PlayerController>();
        controller.useMovementBounds = true;
        controller.minBounds = new Vector2(-95f, -95f);
        controller.maxBounds = new Vector2( 95f,  95f);

        var sw = root.AddComponent<SingleShot>();
        sw.data = _singleShotData;
        sw.projectilePrefab = _coinPrefab;
        sw.color = new Color(0.8f, 0.8f, 1f);

        return Save(root, path);
    }

    private static GameObject MakeProjectile(string fileName, Sprite sprite, Color color, Type componentType, float scale = 0.32f)
    {
        string path = PrefabsPath + "/" + fileName;
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject(fileName.Replace(".prefab",""));
        root.tag = "Projectile";
        var spriteChild = Sprite2D(root, sprite, color, 1, scale);
        spriteChild.gameObject.AddComponent<SpinProjectile>(); // Spin all projectiles
        
        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.15f;
        root.AddComponent(componentType);
        return Save(root, path);
    }

    private static GameObject MakeLingeringAOEPrefab(string fileName, Sprite sprite, float scale = 1.5f)
    {
        string path = PrefabsPath + "/" + fileName;
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject(fileName.Replace(".prefab", ""));
        root.tag = "Projectile";
        var spriteChild = Sprite2D(root, sprite, Color.white, -1, scale); // draw slightly behind
        
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 2.0f;
        root.AddComponent<LingeringAOE>();
        return Save(root, path);
    }

    private static GameObject MakeOrb()
    {
        string path = PrefabsPath + "/XPOrb.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject("XPOrb");
        root.tag = "XPOrb";
        var spriteChild = Sprite2D(root, _xpOrbSprite, Color.white, 1, 1f); // Use standard scale, sprite controls size
        spriteChild.gameObject.AddComponent<ScalePulse>(); // Add pulsing animation
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.25f;
        root.AddComponent<XPOrb>();
        return Save(root, path);
    }

    private static GameObject MakeChest()
    {
        string path = PrefabsPath + "/Chest.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) 
        {
            AssetDatabase.DeleteAsset(path); // Force overwrite
            AssetDatabase.Refresh();
        }

        var root = new GameObject("Chest");
        var sr = Sprite2D(root, _chestSprite, Color.white, 1, 3.5f); // Use custom high-quality sprite and double the size
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 2.0f;
        var c = root.AddComponent<Chest>();
        c.possibleRewards = new[] { _healPU, _speedPU, _damagePU, _magnetPU, _radiusPU, _repelPU, _insiderPU, _taxPU };
        c.openParticlePrefab = MakeChestOpenParticles(); // Assign particle effect
        return Save(root, path);
    }

    private static GameObject MakeEnemyPrefab(string name, EnemyData data, Sprite sprite, float scale, float bobAmt, float bobSpd)
    {
        string path = EnemyPath + "/" + name + ".prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject(name);
        root.tag = "Enemy";
        var spriteChild = Sprite2D(root, sprite, Color.white, 1, 1f);
        
        // Only use bodyColor tint if we're using the generic recycled enemy sprite
        if (sprite == _enemySprite) spriteChild.color = Color.Lerp(Color.white, data.bodyColor, 0.2f);
        
        var bobber = spriteChild.gameObject.AddComponent<SpriteBobber>();
        bobber.bobAmount = bobAmt;
        bobber.bobSpeed  = bobSpd;
        spriteChild.gameObject.AddComponent<HitFlash>(); // Add white hit flash component
        
        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.4f;
        var eb = root.AddComponent<EnemyBase>();
        eb.data = data; eb.poolTag = name;
        eb.xpOrbPrefab = _orbPrefab;
        eb.hitParticlePrefab = _hitParticlesPrefab; // Assign the particle prefab
        return Save(root, path);
    }

    private static GameObject MakeHitParticles()
    {
        string path = PrefabsPath + "/HitParticles.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject("HitParticles");
        var ps = root.AddComponent<ParticleSystem>();
        var psr = root.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Sprites/Default"));
        
        var main = ps.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.25f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy; // auto-destroy when done

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 4, 7) }); // 4 to 7 particles burst

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, 0f); // Shrink to nothing

        return Save(root, path);
    }

    private static GameObject MakeChestOpenParticles()
    {
        string path = PrefabsPath + "/ChestOpenParticles.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject("ChestOpenParticles");
        var ps = root.AddComponent<ParticleSystem>();
        var psr = root.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Sprites/Default"));
        
        var main = ps.main;
        main.duration = 1.0f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 12f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startColor = new Color(1f, 0.9f, 0.2f); // Gold / Treasure color
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 30, 40) }); // Massive burst of golden particles!

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.5f;
        // Shoot upwards
        shape.rotation = new Vector3(-90f, 0f, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(4f, 8f);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f,0.9f,0.2f), 0f), new GradientColorKey(new Color(1f,0.6f,0f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, 0f); // Shrink to nothing

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 1.5f;

        return Save(root, path);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SCENES
    // ══════════════════════════════════════════════════════════════════════════

    private static void CreateMainMenuScene()
    {
        string path = ScenesPath + "/MainMenu.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cam = new GameObject("Main Camera");
        cam.transform.position = new Vector3(0, 0, -10f); // Fix Z-depth for 2D
        cam.tag = "MainCamera";
        var c = cam.AddComponent<Camera>();
        c.orthographic = true; c.orthographicSize = 7f;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color(.03f,.08f,.03f);
        cam.AddComponent<AudioListener>();

        // GameManager
        new GameObject("_GameManager").AddComponent<GameManager>();

        // UI
        var ui = new GameObject("_UI").AddComponent<MainMenuUI>();
        if (_splashSprite != null) ui.splashScreen = _splashSprite.texture;

        bool ok = EditorSceneManager.SaveScene(scene, path);
        Log(ok ? "  MainMenu scene saved" : "  WARNING: MainMenu scene save returned false");
    }

    private static void CreateGameScene()
    {
        string path = ScenesPath + "/Game.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.transform.position = new Vector3(0, 0, -10f); // Fix Z-depth for 2D
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.05f,.09f,.05f);
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<CameraFollow>();
        camGO.AddComponent<ScreenShake>();

        // Background – two-layer parallax:
        // Far layer: soft dark gradient.
        var bgFar = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgFar.name = "BackgroundFar";
        bgFar.transform.position = new Vector3(0, 0, 55f); // furthest back
        bgFar.transform.localScale = new Vector3(220f, 220f, 1f);
        GameObject.DestroyImmediate(bgFar.GetComponent<MeshCollider>());

        var bgFarMat = new Material(Shader.Find("Unlit/Texture")) { name = "BackgroundFarMaterial" };
        if (_bgSpriteBack != null)
        {
            bgFarMat.mainTexture = _bgSpriteBack.texture;
            bgFarMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
            bgFarMat.mainTextureScale = new Vector2(40f, 40f);
        }
        var bgFarRenderer = bgFar.GetComponent<Renderer>();
        bgFarRenderer.sharedMaterial = bgFarMat;
        bgFarRenderer.sortingLayerName = "Default";
        bgFarRenderer.sortingOrder = -2000; // always behind everything else
        var farScroll = bgFar.AddComponent<ScrollingBackground>();
        farScroll.scrollScale = 0.01f;

        // Near layer: lighter tiled pattern with transparent holes to reveal the far layer.
        var bgNear = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgNear.name = "BackgroundNear";
        bgNear.transform.position = new Vector3(0, 0, 50f);
        bgNear.transform.localScale = new Vector3(200f, 200f, 1f);
        GameObject.DestroyImmediate(bgNear.GetComponent<MeshCollider>());

        var bgNearMat = new Material(Shader.Find("Unlit/Transparent")) { name = "BackgroundNearMaterial" };
        if (_bgSpriteFront != null)
        {
            bgNearMat.mainTexture = _bgSpriteFront.texture;
            bgNearMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
            bgNearMat.mainTextureScale = new Vector2(50f, 50f);
        }
        var bgNearRenderer = bgNear.GetComponent<Renderer>();
        bgNearRenderer.sharedMaterial = bgNearMat;
        bgNearRenderer.sortingLayerName = "Default";
        bgNearRenderer.sortingOrder = -1500; // in front of far layer, still behind gameplay sprites
        var nearScroll = bgNear.AddComponent<ScrollingBackground>();
        nearScroll.scrollScale = 0.025f;

        // Player
        if (_playerPrefab == null)
            throw new Exception("Player prefab is null! Check prefab creation step.");

        var player = (GameObject)PrefabUtility.InstantiatePrefab(_playerPrefab);
        player.transform.position = Vector3.zero;
        Log("  Player instantiated");

        // Systems
        var sys = new GameObject("_Systems");
        sys.AddComponent<ObjectPool>();

        var sp = sys.AddComponent<EnemySpawner>();
        sp.bankmanPrefab      = _p1;
        sp.exWifePrefab       = _p2;
        sp.childrenPrefab     = _p3;
        sp.irsPrefab          = _p4;
        sp.bouncerPrefab      = _p5;
        sp.ceoPrefab          = _p6;

        var cs = sys.AddComponent<ChestSpawner>();
        cs.chestPrefab = _chestPrefab;

        sys.AddComponent<XPManager>();

        var lm = sys.AddComponent<LevelUpManager>();
        lm.powerUps      = new List<PowerUpData> { _healPU, _speedPU, _damagePU, _magnetPU, _radiusPU, _insiderPU, _taxPU };
        lm.weaponPrefabs = new List<GameObject>
        {
            CreateWeaponPrefabWrapper("SingleShot", typeof(SingleShot), _singleShotData, _coinPrefab),
            CreateWeaponPrefabWrapper("CoinToss", typeof(CoinToss), _coinData, _coinPrefab),
            CreateWeaponPrefabWrapper("BillWhip", typeof(BillWhip), _whipData, _cardPrefab), // Assigned boomerang prefab to whip
            CreateWeaponPrefabWrapper("CompoundInterest", typeof(CompoundInterest), _auraData, null),
            CreateWeaponPrefabWrapper("BoomerangCard", typeof(BoomerangWeapon), _cardData, _cardPrefab),
            CreateWeaponPrefabWrapper("Cryptominer", typeof(CryptominerWeapon), _cryptoData, _cryptoPrefab),
            CreateWeaponPrefabWrapper("StockOptions", typeof(StockOptionsWeapon), _stockData, _stockPrefab)
        };

        // UI (all OnGUI-based)
        var ui = new GameObject("_UI");
        var hud = ui.AddComponent<HUDController>();
        var cru = ui.AddComponent<ChestRewardUI>();
        var gou = ui.AddComponent<GameOverUI>();
        var luu = ui.AddComponent<LevelUpUI>();

        Texture2D tex = _splashSprite != null ? _splashSprite.texture : null;
        hud.splashScreen = tex;
        gou.splashScreen = tex;
        luu.splashScreen = tex;

        bool ok = EditorSceneManager.SaveScene(scene, path);
        Log(ok ? "  Game scene saved" : "  WARNING: Game scene save returned false");
    }

    private static void AddScenesToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(ScenesPath + "/MainMenu.unity", true),
            new EditorBuildSettingsScene(ScenesPath + "/Game.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
        Log("  Added MainMenu and Game to Build Settings");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    private static GameObject CreateWeaponPrefabWrapper(string name, Type weaponType, WeaponData data, GameObject projectile)
    {
        string path = PrefabsPath + "/" + name + "_Wrapper.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) AssetDatabase.DeleteAsset(path);

        var root = new GameObject(name + "_Wrapper");
        var w = root.AddComponent(weaponType) as WeaponBase;
        w.data = data;
        // Wire up specific fields
        if (w is CoinToss ct) ct.coinPrefab = projectile;
        if (w is SingleShot ss) { ss.projectilePrefab = projectile; ss.color = new Color(0.8f, 0.8f, 1f); }
        if (w is BoomerangWeapon bw) bw.boomerangPrefab = projectile;
        if (w is CryptominerWeapon cw) cw.minerPrefab = projectile;
        if (w is StockOptionsWeapon sw) sw.arrowPrefab = projectile;
        if (w is BillWhip whip) whip.whipPrefab = projectile;

        return Save(root, path);
    }

    private static SpriteRenderer Sprite2D(GameObject parent, Sprite sprite,
        Color color, int order, float scale)
    {
        var child = new GameObject("Sprite");
        child.transform.SetParent(parent.transform);
        child.transform.localScale = Vector3.one * scale;
        var sr = child.AddComponent<SpriteRenderer>();
        sr.sprite = sprite; sr.color = color; sr.sortingOrder = order;
        return sr;
    }

    private static GameObject Save(GameObject go, string path)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        if (prefab == null) throw new Exception($"Failed to save prefab at {path}");
        Log($"  Saved prefab: {path}");
        return prefab;
    }
}
#endif
