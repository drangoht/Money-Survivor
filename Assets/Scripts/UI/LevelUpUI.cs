using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Level-up card selection UI drawn with OnGUI — no packages needed.
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    public Texture2D splashScreen;
    private bool   _visible;
    private Action<UpgradeOption> _callback;
    private Func<List<UpgradeOption>> _getRefreshedOptions;
    private List<UpgradeOption>   _options = new();
    private bool   _refreshUsed;

    private int  _selectedIndex = 0;
    private bool _navigateLeftHandled, _navigateRightHandled;

    // Cached styles
    private GUIStyle _panelStyle, _titleStyle, _cardStyle, _descStyle, _btnStyle;
    private bool     _stylesReady;

    private void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _panelStyle = new GUIStyle(GUI.skin.box);
        _panelStyle.normal.background = MakeTex(new Color(0f, 0f, 0f, 0.92f));

        _titleStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        _titleStyle.normal.textColor = Color.yellow;

        _cardStyle = new GUIStyle(GUI.skin.box);
        _cardStyle.normal.background = MakeTex(new Color(0.08f, 0.13f, 0.08f, 1f));

        _descStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 14, wordWrap = true, alignment = TextAnchor.UpperCenter };
        _descStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        _btnStyle = new GUIStyle(GUI.skin.button)
            { fontSize = 16, fontStyle = FontStyle.Bold };
    }

    public void Show(List<UpgradeOption> options, Action<UpgradeOption> callback, Func<List<UpgradeOption>> getRefreshedOptions = null)
    {
        _options   = options;
        _callback  = callback;
        _getRefreshedOptions = getRefreshedOptions;
        _refreshUsed = false;
        _selectedIndex = 0;
        _visible   = true;
    }

    private void Update()
    {
        if (!_visible) return;

        float x = Input.GetAxisRaw("Horizontal");

        if (x > 0.5f)
        {
            if (!_navigateRightHandled) { _selectedIndex++; _navigateRightHandled = true; }
        }
        else _navigateRightHandled = false;

        if (x < -0.5f)
        {
            if (!_navigateLeftHandled) { _selectedIndex--; _navigateLeftHandled = true; }
        }
        else _navigateLeftHandled = false;

        int selectableCount = _options.Count + (CanShowRefresh() ? 1 : 0);
        if (_selectedIndex < 0) _selectedIndex = selectableCount - 1;
        if (_selectedIndex >= selectableCount) _selectedIndex = 0;

        if (Input.GetButtonDown("Submit"))
        {
            if (CanShowRefresh() && _selectedIndex == _options.Count)
            {
                _options = _getRefreshedOptions();
                _refreshUsed = true;
                _selectedIndex = 0;
            }
            else if (_options.Count > 0 && _selectedIndex < _options.Count)
            {
                _visible = false;
                _callback?.Invoke(_options[_selectedIndex]);
            }
        }
    }

    private bool CanShowRefresh() => _getRefreshedOptions != null && !_refreshUsed && _options.Count > 0;

    private void OnGUI()
    {
        if (!_visible) return;
        InitStyles();

        float sw = Screen.width, sh = Screen.height;

        // Background image
        if (splashScreen != null)
        {
            GUI.DrawTexture(new Rect(0, 0, sw, sh), splashScreen, ScaleMode.ScaleAndCrop);
        }

        // Dim overlay so cards pop
        GUI.color = new Color(0, 0, 0, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUI.Label(new Rect(sw / 2f - 400f, sh * 0.1f, 800f, 60f), "YOU'VE BEEN PROMOTED!", _titleStyle);

        // Cards + Refresh as 4th selectable (so controller can reach it)
        bool showRefresh = CanShowRefresh();
        int totalSlots = _options.Count + (showRefresh ? 1 : 0);
        float cardW = 240f, cardH = 320f;
        float gap = 20f;
        float totalW = cardW * totalSlots + gap * (totalSlots - 1);
        float startX = (sw - totalW) / 2f;
        float cardY  = sh * 0.22f;

        for (int i = 0; i < _options.Count; i++)
        {
            float cx = startX + i * (cardW + 20f);
            var   opt = _options[i];

            // Card background (highlight if selected)
            GUI.color = i == _selectedIndex ? new Color(1f, 1f, 0.5f) : Color.white;
            GUI.Box(new Rect(cx, cardY, cardW, cardH), GUIContent.none, _cardStyle);
            GUI.color = Color.white;

            // Type badge
            string badge = opt.type == UpgradeType.PowerUp ? "★ POWER-UP" : "⚔ WEAPON";
            Color badgeColor = opt.type == UpgradeType.PowerUp
                ? new Color(1f, 0.84f, 0f)
                : new Color(0.4f, 0.8f, 1f);
            var badgeStyle = new GUIStyle(_descStyle) { fontSize = 12, fontStyle = FontStyle.Bold };
            badgeStyle.normal.textColor = badgeColor;
            GUI.Label(new Rect(cx + 10, cardY + 10, cardW - 20, 20f), badge, badgeStyle);

            // Weapon icon (for weapon options)
            float iconSize = 48f;
            float iconY = cardY + 32f;
            float nameY = cardY + 38f;
            if (opt.type != UpgradeType.PowerUp && opt.data != null && opt.data.icon != null && opt.data.icon.texture != null)
            {
                float iconX = cx + (cardW - iconSize) / 2f;
                var iconRect = new Rect(iconX, iconY, iconSize, iconSize);
                var iconTex = opt.data.icon.texture;
                // Alpha blend so transparent (mauve-stripped) background shows correctly
                GUI.DrawTexture(iconRect, iconTex, ScaleMode.ScaleToFit, true);
                nameY = cardY + 32f + iconSize + 4f;
            }

            // Name
            var nameStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter, wordWrap = true };
            nameStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cx + 10, nameY, cardW - 20, 60f), opt.label, nameStyle);

            // Description
            float descY = nameY + 52f;
            GUI.Label(new Rect(cx + 12, descY, cardW - 24, 160f), opt.description, _descStyle);

            // Select Button
            GUI.backgroundColor = i == _selectedIndex ? new Color(0.2f, 0.9f, 0.2f) : new Color(0.2f, 0.6f, 0.2f);
            if (GUI.Button(new Rect(cx + 20, cardY + 270, cardW - 40, 40f), "SELECT", _btnStyle))
            {
                _visible = false;
                _callback?.Invoke(opt);
            }
            GUI.backgroundColor = Color.white;
        }

        // Refresh slot (4th card, same row) — selectable with controller, no text crop
        if (showRefresh)
        {
            int refreshIdx = _options.Count;
            float cx = startX + refreshIdx * (cardW + gap);
            GUI.color = _selectedIndex == refreshIdx ? new Color(1f, 1f, 0.5f) : Color.white;
            GUI.Box(new Rect(cx, cardY, cardW, cardH), GUIContent.none, _cardStyle);
            GUI.color = Color.white;
            var refreshLabelStyle = new GUIStyle(_descStyle) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            refreshLabelStyle.normal.textColor = new Color(0.4f, 0.8f, 1f);
            GUI.Label(new Rect(cx + 10, cardY + 120f, cardW - 20, 80f), "REFRESH\nCHOICES\n(1x)", refreshLabelStyle);
            GUI.backgroundColor = _selectedIndex == refreshIdx ? new Color(0.2f, 0.7f, 1f) : new Color(0.15f, 0.5f, 0.7f);
            if (GUI.Button(new Rect(cx + 20, cardY + 270, cardW - 40, 40f), "REFRESH", _btnStyle))
            {
                _options = _getRefreshedOptions();
                _refreshUsed = true;
                _selectedIndex = 0;
            }
            GUI.backgroundColor = Color.white;
        }
    }

    private static Texture2D MakeTex(Color col)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, col);
        t.Apply();
        return t;
    }
}
