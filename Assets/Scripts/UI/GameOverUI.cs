using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Game Over screen using OnGUI — no packages needed.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    public Texture2D splashScreen;
    private bool   _visible;
    private GUIStyle _titleStyle, _statStyle, _btnStyle, _panelStyle;
    private bool   _stylesReady;

    private int  _selectedIndex = 0; // 0: Retry, 1: Menu
    private bool _navigateLeftHandled, _navigateRightHandled;

    private void Awake() => EventBus.OnPlayerDeath += Show;
    private void OnDestroy() => EventBus.OnPlayerDeath -= Show;

    private void Show()
    {
        _selectedIndex = 0;
        _visible       = true;
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!_visible || GameManager.Instance == null) return;

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

        if (_selectedIndex < 0) _selectedIndex = 1;
        if (_selectedIndex > 1) _selectedIndex = 0;

        bool confirm = false;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) confirm = true;
        if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) confirm = true;

        if (confirm)
        {
            if (_selectedIndex == 0)
            {
                _visible = false;
                Time.timeScale = 1f;
                GameManager.Instance.StartGame();
            }
            else
            {
                _visible = false;
                Time.timeScale = 1f;
                GameManager.Instance.ReturnToMainMenu();
            }
        }
    }

    private void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _panelStyle = new GUIStyle(GUI.skin.box);
        _panelStyle.normal.background = MakeTex(new Color(0f, 0f, 0f, 0.93f));

        _titleStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        _titleStyle.normal.textColor = new Color(0.9f, 0.15f, 0.15f);

        _statStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 24, alignment = TextAnchor.MiddleCenter };
        _statStyle.normal.textColor = Color.white;

        _btnStyle = new GUIStyle(GUI.skin.button)
            { fontSize = 18, fontStyle = FontStyle.Bold };
    }

    private void OnGUI()
    {
        if (!_visible || GameManager.Instance == null) return;
        InitStyles();

        float sw = Screen.width, sh = Screen.height;
        float pw = 480f, ph = 360f;
        float px = (sw - pw) / 2f, py = (sh - ph) / 2f;

        // Background (fullscreen)
        if (splashScreen != null)
        {
            GUI.DrawTexture(new Rect(0, 0, sw, sh), splashScreen, ScaleMode.ScaleAndCrop);
            // Very dark tint over splash screen for Game Over clarity
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Panel
        GUI.Box(new Rect(px, py, pw, ph), GUIContent.none, _panelStyle);

        // Title
        GUI.Label(new Rect(px, py + 20, pw, 70f), "BANKRUPT", _titleStyle);

        // Stats
        float t    = GameManager.Instance.TimeSurvived;
        int   mins = (int)(t / 60f);
        int   secs = (int)(t % 60f);

        GUI.Label(new Rect(px, py + 110, pw, 36f), $"Time Survived:  {mins:00}:{secs:00}", _statStyle);
        GUI.Label(new Rect(px, py + 155, pw, 36f), $"Enemies Killed:  {GameManager.Instance.EnemiesKilled}", _statStyle);
        GUI.Label(new Rect(px, py + 200, pw, 36f), $"Level Reached:  {GameManager.Instance.CurrentLevel}", _statStyle);

        // Buttons
        GUI.backgroundColor = _selectedIndex == 0 ? new Color(0.2f, 0.9f, 0.2f) : new Color(0.1f, 0.6f, 0.15f);
        if (GUI.Button(new Rect(px + 40, py + 270, 170f, 52f), "RETRY", _btnStyle))
        {
            _visible = false;
            Time.timeScale = 1f;
            GameManager.Instance.StartGame();
        }

        GUI.backgroundColor = _selectedIndex == 1 ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.35f, 0.35f, 0.35f);
        if (GUI.Button(new Rect(px + 270, py + 270, 170f, 52f), "MENU", _btnStyle))
        {
            _visible = false;
            Time.timeScale = 1f;
            GameManager.Instance.ReturnToMainMenu();
        }

        GUI.backgroundColor = Color.white;
    }

    private static Texture2D MakeTex(Color col)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, col);
        t.Apply();
        return t;
    }
}
