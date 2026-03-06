using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Renders the in-game HUD (HP, XP, timer, kills) using OnGUI.
/// Also handles the Pause Menu overlay using the new Unity Input System.
/// </summary>
public class HUDController : MonoBehaviour
{
    public Texture2D splashScreen;
    // Runtime values updated by EventBus
    private float _currentHP  = 100f;
    private float _maxHP      = 100f;
    private float _currentXP  = 0f;
    private float _requiredXP = 10f;
    private int   _level      = 1;

    private bool _isPaused;
    
    // Pause menu navigation
    private int  _selectedIndex = 0; // 0: Resume, 1: Main Menu
    private bool _navigateDownHandled, _navigateUpHandled;

    // Cached styles
    private GUIStyle _barBG, _hpFill, _xpFill, _labelStyle;
    private bool     _stylesReady;

    private void OnEnable()
    {
        EventBus.OnPlayerHPChanged += OnHPChanged;
        EventBus.OnXPChanged       += OnXPChanged;
        EventBus.OnPlayerLevelUp   += OnLevelUp;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerHPChanged -= OnHPChanged;
        EventBus.OnXPChanged       -= OnXPChanged;
        EventBus.OnPlayerLevelUp   -= OnLevelUp;
    }

    private void OnHPChanged(float cur, float max) { _currentHP = cur; _maxHP = max; }
    private void OnXPChanged(float cur, float req)  { _currentXP = cur; _requiredXP = req; }
    private void OnLevelUp(int lvl)                 { _level = lvl; }

    private void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _barBG = new GUIStyle(GUI.skin.box);
        _barBG.normal.background = MakeTex(1, 1, new Color(0.05f, 0.08f, 0.12f, 0.95f)); // Dark navy/metallic background

        _hpFill = new GUIStyle();
        _hpFill.normal.background = MakeTex(1, 1, new Color(1f, 0.1f, 0.5f, 1f)); // Neon Pink/Magenta HP

        _xpFill = new GUIStyle();
        _xpFill.normal.background = MakeTex(1, 1, new Color(0f, 0.8f, 1f, 1f));   // Electric cyan XP

        _labelStyle = new GUIStyle(GUI.skin.label);
        _labelStyle.fontSize  = 18;
        _labelStyle.fontStyle = FontStyle.Bold;
        _labelStyle.normal.textColor = Color.white;
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State == GameState.GameOver) return;

        bool togglePause = false;
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame) togglePause = true;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) togglePause = true;

        if (togglePause)
        {
            if (GameManager.Instance.State == GameState.Playing)
            {
                _selectedIndex = 0;
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.State == GameState.Paused)
                GameManager.Instance.ResumeGame();
        }

        // Handle pause menu navigation
        if (GameManager.Instance.State == GameState.Paused)
        {
            float y = 0f;
            if (Gamepad.current != null)
                y = Gamepad.current.leftStick.y.ReadValue() + Gamepad.current.dpad.y.ReadValue();
            
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            }

            if (y > 0.5f)
            {
                if (!_navigateUpHandled) { _selectedIndex--; _navigateUpHandled = true; }
            }
            else _navigateUpHandled = false;

            if (y < -0.5f)
            {
                if (!_navigateDownHandled) { _selectedIndex++; _navigateDownHandled = true; }
            }
            else _navigateDownHandled = false;

            if (_selectedIndex < 0) _selectedIndex = 1;
            if (_selectedIndex > 1) _selectedIndex = 0;

            bool confirm = false;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) confirm = true;
            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) confirm = true;

            if (confirm)
            {
                if (_selectedIndex == 0) GameManager.Instance.ResumeGame();
                else GameManager.Instance.ReturnToMainMenu();
            }
        }
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.State == GameState.MainMenu) return;
        InitStyles();

        // Draw HUD if playing OR paused (so it stays visible under the pause menu)
        DrawHUD();

        if (GameManager.Instance.State == GameState.Paused)
        {
            DrawPauseMenu();
        }
    }

    private void DrawHUD()
    {
        float sw = Screen.width;

        // ── HP Bar (Stylized) ──────────────────────────────────────────────────
        float barW = 320f, barH = 26f;
        float barX = 30f,  barY = 30f;

        // Border shadow/glow
        GUI.color = new Color(1f, 0.1f, 0.5f, 0.3f);
        GUI.DrawTexture(new Rect(barX - 2, barY - 2, barW + 4, barH + 4), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Background & Fill
        GUI.Box(new Rect(barX, barY, barW, barH), GUIContent.none, _barBG);
        float hpPct = _maxHP > 0 ? _currentHP / _maxHP : 0f;
        GUI.Box(new Rect(barX, barY, barW * hpPct, barH), GUIContent.none, _hpFill);
        
        // HP Text overlay
        var hpTxtStyle = new GUIStyle(_labelStyle) { alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(barX, barY, barW, barH), $"COMPANY HEALTH : {Mathf.CeilToInt(_currentHP)} / {Mathf.CeilToInt(_maxHP)}", hpTxtStyle);

        // ── XP Bar (Stylized) ──────────────────────────────────────────────────
        barY += barH + 12f;
        float xpBarH = 16f;

        // Border shadow/glow
        GUI.color = new Color(0f, 0.8f, 1f, 0.3f);
        GUI.DrawTexture(new Rect(barX - 2, barY - 2, barW + 4, xpBarH + 4), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Box(new Rect(barX, barY, barW, xpBarH), GUIContent.none, _barBG);
        float xpPct = _requiredXP > 0 ? _currentXP / _requiredXP : 0f;
        GUI.Box(new Rect(barX, barY, barW * xpPct, xpBarH), GUIContent.none, _xpFill);

        // ── Level Badge ───────────────────────────────────────────────────────
        var lvlStyle = new GUIStyle(_labelStyle) { fontSize = 24, fontStyle = FontStyle.Bold };
        lvlStyle.normal.textColor = new Color(0f, 0.8f, 1f); // match XP bar
        GUI.Label(new Rect(barX, barY + xpBarH + 8f, 200f, 40f), $"AUTHORITY LVL <color=white>{_level}</color>", lvlStyle);

        // ── Top Center Timer Box ─────────────────────────────────────────────
        float t     = GameManager.Instance.TimeSurvived;
        int   mins  = (int)(t / 60f);
        int   secs  = (int)(t % 60f);
        
        float tw = 160f, th = 40f;
        float tx = (sw - tw) / 2f;
        GUI.Box(new Rect(tx, 20f, tw, th), GUIContent.none, _barBG); // Timer bg
        
        var tStyle = new GUIStyle(_labelStyle) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
        tStyle.normal.textColor = new Color(1f, 0.8f, 0f); // Gold timer
        GUI.Label(new Rect(tx, 20f, tw, th), $"{mins:00}:{secs:00}", tStyle);

        // ── Top Right Score & Kills ───────────────────────────────────────────
        int kills  = GameManager.Instance.EnemiesKilled;
        int score  = kills * 50;
        
        var kStyle = new GUIStyle(_labelStyle) { fontSize = 22, alignment = TextAnchor.MiddleRight };
        string netWorthText = $"NET WORTH: ${score}";
        Vector2 textSize = kStyle.CalcSize(new GUIContent(netWorthText));

        float scoreW = Mathf.Max(240f, textSize.x + 30f);
        float scoreH = 65f;
        float scoreX = sw - scoreW - 30f;
        
        // Score Box
        GUI.color = new Color(0.2f, 0.9f, 0.4f, 0.2f);
        GUI.DrawTexture(new Rect(scoreX - 2, 28f, scoreW + 4, scoreH + 4), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Box(new Rect(scoreX, 30f, scoreW, scoreH), GUIContent.none, _barBG);

        kStyle.fontSize = 18;
        kStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(scoreX, 35f, scoreW - 15f, 25f), $"ACQUISITIONS: <color=yellow>{kills}</color>", kStyle);
        
        kStyle.fontSize = 22;
        kStyle.normal.textColor = new Color(0.2f, 1f, 0.4f); // Neon green score
        GUI.Label(new Rect(scoreX, 60f, scoreW - 15f, 30f), netWorthText, kStyle);
    }

    private void DrawPauseMenu()
    {
        float sw = Screen.width;
        float sh = Screen.height;

        // Dark overlay / Splash Screen
        if (splashScreen != null)
        {
            GUI.DrawTexture(new Rect(0, 0, sw, sh), splashScreen, ScaleMode.ScaleAndCrop);
            // Ultra-dark tint so menu stats are perfectly readable
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        else
        {
            GUI.Box(new Rect(0, 0, sw, sh), GUIContent.none, _barBG);
            GUI.Box(new Rect(0, 0, sw, sh), GUIContent.none, _barBG); // double for darkness
        }

        float panelW = 600f;
        float panelH = 500f;
        float startX = (sw - panelW) / 2f;
        float startY = (sh - panelH) / 2f;

        GUI.Box(new Rect(startX, startY, panelW, panelH), GUIContent.none, _barBG);

        var titleStyle = new GUIStyle(_labelStyle) { fontSize = 32, alignment = TextAnchor.UpperCenter };
        titleStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(startX, startY + 20f, panelW, 40f), "- PAUSED -", titleStyle);

        // Fetch Player and Stats
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        var weapons = player.GetComponentsInChildren<WeaponBase>();

        // Player Stats column
        float colX = startX + 40f;
        float colY = startY + 80f;

        var headerStyle = new GUIStyle(_labelStyle) { fontSize = 22 };
        headerStyle.normal.textColor = new Color(0.3f, 0.8f, 1f);

        GUI.Label(new Rect(colX, colY, 200f, 30f), "PLAYER STATS", headerStyle);
        colY += 40f;

        if (stats != null)
        {
            GUI.Label(new Rect(colX, colY, 250f, 25f), $"HP: {Mathf.CeilToInt(stats.CurrentHP)} / {Mathf.CeilToInt(stats.maxHP)}", _labelStyle);
            colY += 25f;
            GUI.Label(new Rect(colX, colY, 250f, 25f), $"Speed: {stats.moveSpeed:F1}", _labelStyle);
            colY += 25f;
            GUI.Label(new Rect(colX, colY, 250f, 25f), $"Damage Mult: {stats.damageMultiplier:F1}x", _labelStyle);
            colY += 25f;
            GUI.Label(new Rect(colX, colY, 250f, 25f), $"Pickup Radius: {stats.pickupRadius:F1}m", _labelStyle);
            colY += 25f;
            GUI.Label(new Rect(colX, colY, 250f, 25f), $"XP Multiplier: {stats.xpMultiplier:F1}x", _labelStyle);
        }

        // Weapons column
        colX = startX + 300f;
        colY = startY + 80f;
        
        GUI.Label(new Rect(colX, colY, 200f, 30f), "EQUIPMENT", headerStyle);
        colY += 40f;

        if (weapons.Length == 0)
        {
            GUI.Label(new Rect(colX, colY, 250f, 25f), "No weapons equipped", _labelStyle);
        }
        else
        {
            foreach (var w in weapons)
            {
                if (w.data == null) continue;
                GUI.Label(new Rect(colX, colY, 250f, 25f), $"{w.data.weaponName} (Lv {w.CurrentLevel + 1})", _labelStyle);
                colY += 25f;
            }
        }

        // Buttons
        float btnW = 200f, btnH = 50f;
        float btnY = startY + panelH - 80f;

        var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };

        GUI.backgroundColor = _selectedIndex == 0 ? new Color(0.2f, 0.9f, 0.2f) : Color.white;
        if (GUI.Button(new Rect(startX + 60f, btnY, btnW, btnH), "RESUME", btnStyle))
        {
            GameManager.Instance.ResumeGame();
        }

        GUI.backgroundColor = _selectedIndex == 1 ? new Color(0.9f, 0.2f, 0.2f) : Color.white;
        if (GUI.Button(new Rect(startX + panelW - btnW - 60f, btnY, btnW, btnH), "MAIN MENU", btnStyle))
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        GUI.backgroundColor = Color.white;
    }

    private static Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        for (int i = 0; i < w * h; i++) tex.SetPixel(i % w, i / w, col);
        tex.Apply();
        return tex;
    }
}
