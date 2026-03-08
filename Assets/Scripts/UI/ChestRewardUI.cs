using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Chest reward popup using OnGUI. Shows the label and description of the reward (weapon, weapon upgrade, or power-up).
/// Reward is already applied by Chest via LevelUpManager.ApplyReward.
/// </summary>
public class ChestRewardUI : MonoBehaviour
{
    private bool   _visible;
    private string _rewardName = "";
    private string _rewardDesc = "";
    private float  _hideTime;
    public  float  displayDuration = 3f;

    private void Awake() => EventBus.OnChestOpened += Show;
    private void OnDestroy() => EventBus.OnChestOpened -= Show;

    public void Show(UpgradeOption option)
    {
        if (string.IsNullOrEmpty(option.label)) return;
        _rewardName = option.label;
        _rewardDesc = option.description ?? "";
        _visible    = true;
        _hideTime   = Time.unscaledTime + displayDuration;
    }

    private void Update()
    {
        if (!_visible) return;

        bool dismiss = false;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) dismiss = true;
        if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) dismiss = true;

        if (Time.unscaledTime >= _hideTime || dismiss)
            _visible = false;
    }

    private void OnGUI()
    {
        if (!_visible) return;

        float sw = Screen.width, sh = Screen.height;
        float w = 320f, h = 110f;
        float x = sw - w - 20f, y = sh - h - 20f;

        var bg = new Texture2D(1, 1);
        bg.SetPixel(0, 0, new Color(0.1f, 0.08f, 0.02f, 0.95f));
        bg.Apply();
        GUI.DrawTexture(new Rect(x, y, w, h), bg);

        GUI.color = new Color(1f, 0.82f, 0f);
        GUI.DrawTexture(new Rect(x, y, w, 3f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var headerStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 13, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = new Color(1f, 0.82f, 0f);
        GUI.Label(new Rect(x + 10, y + 8, w - 20, 22f), "CHEST OPENED!", headerStyle);

        var nameStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 18, fontStyle = FontStyle.Bold };
        nameStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 10, y + 30, w - 20, 28f), _rewardName, nameStyle);

        var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        GUI.Label(new Rect(x + 10, y + 60, w - 20, 44f), _rewardDesc, descStyle);
    }
}
