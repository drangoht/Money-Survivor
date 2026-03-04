using UnityEngine;

/// <summary>
/// HUD drawn entirely with OnGUI — no packages needed.
/// Displays HP bar, XP bar, level, and elapsed timer.
/// Attach to any persistent GameObject in the Game scene.
/// </summary>
public class HUDController : MonoBehaviour
{
    // Runtime values updated by EventBus
    private float _currentHP  = 100f;
    private float _maxHP      = 100f;
    private float _currentXP  = 0f;
    private float _requiredXP = 50f;
    private int   _level      = 1;

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
        _barBG.normal.background = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f, 0.85f));

        _hpFill = new GUIStyle();
        _hpFill.normal.background = MakeTex(1, 1, new Color(0.85f, 0.1f, 0.1f, 1f));

        _xpFill = new GUIStyle();
        _xpFill.normal.background = MakeTex(1, 1, new Color(1f, 0.82f, 0f, 1f));

        _labelStyle = new GUIStyle(GUI.skin.label);
        _labelStyle.fontSize  = 16;
        _labelStyle.fontStyle = FontStyle.Bold;
        _labelStyle.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.State == GameState.MainMenu) return;
        InitStyles();

        float sw = Screen.width;

        // ── HP Bar ────────────────────────────────────────────────────────────
        float barW = 280f, barH = 22f;
        float barX = 20f,  barY = 20f;

        GUI.Box(new Rect(barX, barY, barW, barH), GUIContent.none, _barBG);
        float hpPct = _maxHP > 0 ? _currentHP / _maxHP : 0f;
        GUI.Box(new Rect(barX, barY, barW * hpPct, barH), GUIContent.none, _hpFill);
        GUI.Label(new Rect(barX, barY, barW, barH),
                  $" ♥ {Mathf.CeilToInt(_currentHP)} / {Mathf.CeilToInt(_maxHP)}", _labelStyle);

        // ── XP Bar ────────────────────────────────────────────────────────────
        barY += barH + 5f;
        float xpBarH = 14f;
        GUI.Box(new Rect(barX, barY, barW, xpBarH), GUIContent.none, _barBG);
        float xpPct = _requiredXP > 0 ? _currentXP / _requiredXP : 0f;
        GUI.Box(new Rect(barX, barY, barW * xpPct, xpBarH), GUIContent.none, _xpFill);

        // ── Level ─────────────────────────────────────────────────────────────
        var lvlStyle = new GUIStyle(_labelStyle) { fontSize = 20 };
        lvlStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(barX, barY + xpBarH + 2f, 120f, 30f), $"Lv {_level}", lvlStyle);

        // ── Timer ─────────────────────────────────────────────────────────────
        float t     = GameManager.Instance.TimeSurvived;
        int   mins  = (int)(t / 60f);
        int   secs  = (int)(t % 60f);
        var   tStyle = new GUIStyle(_labelStyle) { fontSize = 22, alignment = TextAnchor.UpperCenter };
        GUI.Label(new Rect(sw / 2f - 50f, 14f, 100f, 36f), $"{mins:00}:{secs:00}", tStyle);

        // ── Kills & Score ─────────────────────────────────────────────────────
        var kStyle = new GUIStyle(_labelStyle) { fontSize = 16, alignment = TextAnchor.UpperRight };
        int kills  = GameManager.Instance.EnemiesKilled;
        int score  = (kills * 50) + (int)(t * 10f);

        GUI.Label(new Rect(sw - 200f, 16f, 180f, 30f), $"Kills: {kills}", kStyle);
        
        kStyle.normal.textColor = new Color(0.2f, 0.9f, 0.4f); // Neon green score
        GUI.Label(new Rect(sw - 200f, 38f, 180f, 30f), $"SCORE: {score}", kStyle);
    }

    private static Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        for (int i = 0; i < w * h; i++) tex.SetPixel(i % w, i / w, col);
        tex.Apply();
        return tex;
    }
}
