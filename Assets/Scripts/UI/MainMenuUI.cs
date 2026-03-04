using UnityEngine;

/// <summary>
/// Main Menu UI using OnGUI — no packages needed.
/// Loads in the MainMenu scene.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Texture2D splashScreen;
    private GUIStyle _titleStyle, _subStyle, _btnStyle, _bgStyle;
    private bool     _stylesReady;

    private void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _bgStyle = new GUIStyle();
        _bgStyle.normal.background = MakeTex(new Color(0.03f, 0.08f, 0.03f, 1f));

        _titleStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 62, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        _titleStyle.normal.textColor = new Color(1f, 0.82f, 0f);

        _subStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        _subStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

        _btnStyle = new GUIStyle(GUI.skin.button)
            { fontSize = 22, fontStyle = FontStyle.Bold };
    }

    private void OnGUI()
    {
        InitStyles();

        float sw = Screen.width, sh = Screen.height;

        // Background
        if (splashScreen != null)
        {
            GUI.DrawTexture(new Rect(0, 0, sw, sh), splashScreen, ScaleMode.ScaleAndCrop);
            // Add a slight dark tint so text remains readable
            GUI.color = new Color(0, 0, 0, 0.4f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        else
        {
            GUI.Box(new Rect(0, 0, sw, sh), GUIContent.none, _bgStyle);
        }

        // Gold accent bar
        GUI.color = new Color(1f, 0.82f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(sw / 2f - 260f, sh / 2f - 120f, 520f, 3f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(sw / 2f - 260f, sh / 2f + 60f,  520f, 3f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUI.Label(new Rect(sw / 2f - 320f, sh / 2f - 200f, 640f, 90f), "MONEY SURVIVOR", _titleStyle);

        // Subtitle
        GUI.Label(new Rect(sw / 2f - 280f, sh / 2f - 106f, 560f, 36f),
                  "Survive the financial apocalypse", _subStyle);

        // Play button
        GUI.backgroundColor = new Color(0.1f, 0.65f, 0.15f);
        if (GUI.Button(new Rect(sw / 2f - 130f, sh / 2f - 20f, 260f, 60f), "PLAY", _btnStyle))
        {
            GameManager.Instance?.StartGame();
        }

        // Quit button
        GUI.backgroundColor = new Color(0.5f, 0.1f, 0.1f);
        if (GUI.Button(new Rect(sw / 2f - 130f, sh / 2f + 55f, 260f, 50f), "QUIT", _btnStyle))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        GUI.backgroundColor = Color.white;

        // Footer
        var footerStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.LowerCenter };
        footerStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
        GUI.Label(new Rect(0, sh - 30f, sw, 24f), "WASD to move  |  Defeat enemies  |  Collect XP  |  Level up!", footerStyle);
    }

    private static Texture2D MakeTex(Color col)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, col);
        t.Apply();
        return t;
    }
}
