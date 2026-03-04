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

    private static void GenerateSprites()
    {
        _circle = GetOrCreateSprite(SpritesPath + "/Circle.png", CreateCircleTex(128));
        _square = GetOrCreateSprite(SpritesPath + "/Square.png",  CreateSolidTex(32, Color.white));
        Log($"  Sprites ready");
    }

    private static Sprite GetOrCreateSprite(string path, Texture2D generated)
    {
        string texPath = path;
        string spritePath = path.Replace(".png", "_Sprite.asset");

        // 1. Ensure Texture exists
        if (!File.Exists(texPath))
        {
            File.WriteAllBytes(texPath, generated.EncodeToPNG());
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite; // Still good for UI rendering
                importer.spritePixelsPerUnit = 100f;
                importer.filterMode = FilterMode.Bilinear;
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

    private static EnemyData  _bankman, _loanShark, _taxCollector, _auditor, _bouncer, _ceo;
    private static WeaponData _coinData, _whipData,  _auraData, _cardData, _dividendData, _singleShotData;
    private static PowerUpData _healPU, _speedPU, _damagePU, _magnetPU, _radiusPU;

    private static void CreateScriptableObjects()
    {
        _bankman      = MakeEnemy("Bankman",      new Color(.2f,.4f,.9f),  30f, 1.5f,  5f, 10);
        _loanShark    = MakeEnemy("LoanShark",    new Color(.7f,.1f,.1f),  15f, 3.0f,  8f, 15);
        _taxCollector = MakeEnemy("TaxCollector", new Color(.5f, 0f,.5f), 100f, 0.8f, 12f, 25);
        _auditor      = MakeEnemy("Auditor",      new Color(.8f,.1f, 0f), 500f, 1.2f, 20f, 100, 1.05f, 1.02f);
        _bouncer      = MakeEnemy("Bouncer",      new Color(.3f,.3f,.3f), 150f, 0.6f, 15f, 40);
        _ceo          = MakeEnemy("CEO",          new Color(1f,.9f,.1f), 2500f, 2.5f, 30f, 500, 1.02f, 1.01f);

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

        _dividendData = MakeWeapon("DividendShieldData", "Dividend Shield",
            "Coins rapidly orbit and damage enemies near you.",
            new WeaponLevelStats[]
            {
                new() { damage=8,  fireRate=90f,  aoeRadius=1.5f, projectileCount=1 },
                new() { damage=12, fireRate=120f, aoeRadius=1.8f, projectileCount=2 },
                new() { damage=18, fireRate=150f, aoeRadius=2.0f, projectileCount=3 },
                new() { damage=25, fireRate=180f, aoeRadius=2.2f, projectileCount=4 },
                new() { damage=40, fireRate=210f, aoeRadius=2.5f, projectileCount=6 },
            });

        _healPU   = MakePU("HealthInsurance","Health Insurance","Restores 30 HP.",
                        PowerUpEffectType.HealHP,30f,new Color(.9f,.2f,.2f));
        _speedPU  = MakePU("GoldRush","Gold Rush","Increase move speed.",
                        PowerUpEffectType.IncreaseSpeed,0.5f,new Color(1f,.84f,0f));
        _damagePU = MakePU("HedgeFund","Hedge Fund","+15% damage.",
                        PowerUpEffectType.IncreaseDamage,15f,new Color(.2f,.8f,.3f));
        _magnetPU = MakePU("BlackMarket","Black Market","Attract all XP orbs instantly.",
                        PowerUpEffectType.MagnetAllOrbs,0f,new Color(.15f,.15f,.15f));
        _radiusPU = MakePU("Diversified","Diversified Portfolio","Bigger XP pickup radius.",
                        PowerUpEffectType.IncreasePickupRadius,1f,new Color(.3f,.5f,1f));

        Log($"  All ScriptableObjects created");
    }

    private static EnemyData MakeEnemy(string id, Color col, float hp, float spd,
        float dmg, int xp, float hpS = 1.1f, float sS = 1.05f)
    {
        string path = EnemySOPath + "/" + id + ".asset";
        var ex = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (ex != null) { Log($"  Enemy exists: {id}"); return ex; }
        var d = ScriptableObject.CreateInstance<EnemyData>();
        d.enemyName = id; d.bodyColor = col; d.hp = hp; d.moveSpeed = spd;
        d.contactDamage = dmg; d.xpValue = xp; d.hpScaleFactor = hpS; d.speedScaleFactor = sS;
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
    private static GameObject _cardPrefab, _dividendPrefab;
    private static GameObject _p1, _p2, _p3, _p4, _p5, _p6; // enemy prefabs

    private static void CreatePrefabs()
    {
        _coinPrefab     = MakeProjectile("Coin.prefab", _circle, new Color(1f,.84f,0f), typeof(ProjectileBase));
        _cardPrefab     = MakeProjectile("Card.prefab", _square, new Color(.2f,.8f,.9f), typeof(BoomerangProjectile));
        _dividendPrefab = MakeProjectile("Dividend.prefab", _circle, new Color(1f,.6f,0f), typeof(ProjectileBase));
        _orbPrefab      = MakeOrb();
        _chestPrefab    = MakeChest();
        _playerPrefab   = MakePlayer();
        
        _p1 = MakeEnemy("Bankman",      _bankman,      0.7f);
        _p2 = MakeEnemy("LoanShark",    _loanShark,    0.55f);
        _p3 = MakeEnemy("TaxCollector", _taxCollector, 0.9f);
        _p4 = MakeEnemy("Auditor",      _auditor,      1.5f);
        _p5 = MakeEnemy("Bouncer",      _bouncer,      1.2f);
        _p6 = MakeEnemy("CEO",          _ceo,          2.0f);
        
        Log("  All prefabs created");
    }

    private static GameObject MakePlayer()
    {
        string path = PrefabsPath + "/Player.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) { Log("  Player exists"); return ex; }

        var root = new GameObject("Player");
        root.tag = "Player";
        Sprite2D(root, _circle, new Color(.1f,.9f,.3f), 2, .8f);

        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var col = root.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        root.AddComponent<PlayerStats>();
        root.AddComponent<PlayerController>();

        var sw = root.AddComponent<SingleShot>();
        sw.data = _singleShotData;
        sw.projectilePrefab = _coinPrefab;
        sw.color = new Color(0.8f, 0.8f, 1f);

        return Save(root, path);
    }

    private static GameObject MakeProjectile(string fileName, Sprite sprite, Color color, Type componentType)
    {
        string path = PrefabsPath + "/" + fileName;
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject(fileName.Replace(".prefab",""));
        root.tag = "Projectile";
        Sprite2D(root, sprite, color, 1, .32f);
        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.15f;
        root.AddComponent(componentType);
        return Save(root, path);
    }

    private static GameObject MakeOrb()
    {
        string path = PrefabsPath + "/XPOrb.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject("XPOrb");
        root.tag = "XPOrb";
        Sprite2D(root, _circle, new Color(1f,.95f,.1f), 1, .28f);
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.25f;
        root.AddComponent<XPOrb>();
        return Save(root, path);
    }

    private static GameObject MakeChest()
    {
        string path = PrefabsPath + "/Chest.prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject("Chest");
        var sr = Sprite2D(root, _square, new Color(.65f,.4f,.1f), 1, .75f);
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.7f;
        var c = root.AddComponent<Chest>();
        c.possibleRewards = new[] { _healPU, _speedPU, _damagePU, _magnetPU, _radiusPU };
        return Save(root, path);
    }

    private static GameObject MakeEnemy(string name, EnemyData data, float scale)
    {
        string path = EnemyPath + "/" + name + ".prefab";
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject(name);
        root.tag = "Enemy";
        Sprite2D(root, _circle, data.bodyColor, 1, scale);
        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.4f;
        var eb = root.AddComponent<EnemyBase>();
        eb.data = data; eb.poolTag = name;
        eb.xpOrbPrefab = _orbPrefab;
        return Save(root, path);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SCENES
    // ══════════════════════════════════════════════════════════════════════════

    private static void CreateMainMenuScene()
    {
        string path = ScenesPath + "/MainMenu.unity";
        if (File.Exists(path)) { Log("  MainMenu scene exists"); return; }

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
        new GameObject("_UI").AddComponent<MainMenuUI>();

        bool ok = EditorSceneManager.SaveScene(scene, path);
        Log(ok ? "  MainMenu scene saved" : "  WARNING: MainMenu scene save returned false");
    }

    private static void CreateGameScene()
    {
        string path = ScenesPath + "/Game.unity";
        if (File.Exists(path)) { Log("  Game scene exists"); return; }

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

        // (Background explicitly removed — using camera solid color instead)

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
        sp.loanSharkPrefab    = _p2;
        sp.taxCollectorPrefab = _p3;
        sp.auditorPrefab      = _p4;
        sp.bouncerPrefab      = _p5;
        sp.ceoPrefab          = _p6;

        var cs = sys.AddComponent<ChestSpawner>();
        cs.chestPrefab = _chestPrefab;

        sys.AddComponent<XPManager>();

        var lm = sys.AddComponent<LevelUpManager>();
        lm.powerUps      = new List<PowerUpData> { _healPU, _speedPU, _damagePU, _magnetPU, _radiusPU };
        lm.weaponPrefabs = new List<GameObject>
        {
            CreateWeaponPrefabWrapper("SingleShot", typeof(SingleShot), _singleShotData, _coinPrefab),
            CreateWeaponPrefabWrapper("CoinToss", typeof(CoinToss), _coinData, _coinPrefab),
            CreateWeaponPrefabWrapper("BillWhip", typeof(BillWhip), _whipData, null),
            CreateWeaponPrefabWrapper("CompoundInterest", typeof(CompoundInterest), _auraData, null),
            CreateWeaponPrefabWrapper("BoomerangCard", typeof(BoomerangWeapon), _cardData, _cardPrefab),
            CreateWeaponPrefabWrapper("DividendShield", typeof(OrbitalWeapon), _dividendData, _dividendPrefab)
        };

        // UI (all OnGUI-based)
        var ui = new GameObject("_UI");
        ui.AddComponent<HUDController>();
        ui.AddComponent<ChestRewardUI>();
        ui.AddComponent<GameOverUI>();
        ui.AddComponent<LevelUpUI>();

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
        var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (ex != null) return ex;

        var root = new GameObject(name + "_Wrapper");
        var w = root.AddComponent(weaponType) as WeaponBase;
        w.data = data;
        
        // Wire up specific fields
        if (w is CoinToss ct) ct.coinPrefab = projectile;
        if (w is SingleShot ss) { ss.projectilePrefab = projectile; ss.color = new Color(0.8f, 0.8f, 1f); }
        if (w is BoomerangWeapon bw) bw.boomerangPrefab = projectile;
        if (w is OrbitalWeapon ow) ow.orbitalPrefab = projectile;

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
