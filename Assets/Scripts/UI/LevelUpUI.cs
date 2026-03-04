using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Level-up card selection UI drawn with OnGUI — no packages needed.
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    private bool   _visible;
    private Action<UpgradeOption> _callback;
    private List<UpgradeOption>   _options = new();

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
        _visible  = true;
    }

    private void OnGUI()
    {
        if (!_visible) return;
        InitStyles();

        float sw = Screen.width, sh = Screen.height;

        // Dim overlay
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUI.Label(new Rect(sw / 2f - 250f, sh * 0.1f, 500f, 60f), "LEVEL UP!", _titleStyle);

        // Cards
        float cardW = 240f, cardH = 320f;
        float totalW = cardW * _options.Count + 20f * (_options.Count - 1);
        float startX = (sw - totalW) / 2f;
        float cardY  = sh * 0.22f;

        for (int i = 0; i < _options.Count; i++)
        {
            float cx = startX + i * (cardW + 20f);
            var   opt = _options[i];

            // Card background
            GUI.Box(new Rect(cx, cardY, cardW, cardH), GUIContent.none, _cardStyle);

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

            // Choose button
            GUI.backgroundColor = new Color(0.1f, 0.55f, 0.15f);
            if (GUI.Button(new Rect(cx + 20, cardY + cardH - 60, cardW - 40, 44f), "CHOOSE", _btnStyle))
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
