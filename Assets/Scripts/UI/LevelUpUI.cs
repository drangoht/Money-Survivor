using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Level-up card selection UI drawn with OnGUI — no packages needed.
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    public Texture2D splashScreen;
    private bool   _visible;
    private Action<UpgradeOption> _callback;
    private List<UpgradeOption>   _options = new();

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

    public void Show(List<UpgradeOption> options, Action<UpgradeOption> callback)
    {
        _options  = options;
        _callback = callback;
        _selectedIndex = 0;
        _visible  = true;
    }

    private void Update()
    {
        if (!_visible) return;

        float x = 0f;
        if (Gamepad.current != null)
            x = Gamepad.current.leftStick.x.ReadValue() + Gamepad.current.dpad.x.ReadValue();
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        }

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

        if (_selectedIndex < 0) _selectedIndex = _options.Count - 1;
        if (_selectedIndex >= _options.Count) _selectedIndex = 0;

        bool confirm = false;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) confirm = true;
        if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) confirm = true;

        if (confirm && _options.Count > 0)
        {
            _visible = false;
            _callback?.Invoke(_options[_selectedIndex]);
        }
    }

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

        // Cards
        float cardW = 240f, cardH = 320f;
        float totalW = cardW * _options.Count + 20f * (_options.Count - 1);
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

            // Name
            var nameStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter, wordWrap = true };
            nameStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cx + 10, cardY + 38, cardW - 20, 60f), opt.label, nameStyle);

            // Description
            GUI.Label(new Rect(cx + 12, cardY + 105, cardW - 24, 160f), opt.description, _descStyle);

            // Select Button
            GUI.backgroundColor = i == _selectedIndex ? new Color(0.2f, 0.9f, 0.2f) : new Color(0.2f, 0.6f, 0.2f);
            if (GUI.Button(new Rect(cx + 20, cardY + 260, cardW - 40, 40f), "SELECT", _btnStyle))
            {
                _visible = false;
                _callback?.Invoke(opt);
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
