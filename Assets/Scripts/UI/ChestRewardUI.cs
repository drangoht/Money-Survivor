using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Chest reward popup using OnGUI — no packages needed.
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

    public void Show(PowerUpData reward)
    {
        if (reward == null) return;
        _rewardName = reward.powerUpName;
        _rewardDesc = reward.description;
        _visible    = true;
        _hideTime   = Time.unscaledTime + displayDuration;

        ApplyReward(reward);
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

        // Panel
        var bg = new Texture2D(1, 1);
        bg.SetPixel(0, 0, new Color(0.1f, 0.08f, 0.02f, 0.95f));
        bg.Apply();
        GUI.DrawTexture(new Rect(x, y, w, h), bg);

        // Border line
        GUI.color = new Color(1f, 0.82f, 0f);
        GUI.DrawTexture(new Rect(x, y, w, 3f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Header
        var headerStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 13, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = new Color(1f, 0.82f, 0f);
        GUI.Label(new Rect(x + 10, y + 8, w - 20, 22f), "CHEST OPENED!", headerStyle);

        // Reward name
        var nameStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 18, fontStyle = FontStyle.Bold };
        nameStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 10, y + 30, w - 20, 28f), _rewardName, nameStyle);

        // Description
        var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        GUI.Label(new Rect(x + 10, y + 60, w - 20, 44f), _rewardDesc, descStyle);
    }

    private void ApplyReward(PowerUpData pu)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        switch (pu.effectType)
        {
            case PowerUpEffectType.HealHP:               stats.Heal(pu.value);              break;
            case PowerUpEffectType.IncreaseMaxHP:         stats.IncreaseMaxHP(pu.value);     break;
            case PowerUpEffectType.IncreaseSpeed:         stats.moveSpeed      += pu.value;  break;
            case PowerUpEffectType.IncreaseDamage:        stats.damageMultiplier += pu.value / 100f; break;
            case PowerUpEffectType.IncreasePickupRadius:  stats.pickupRadius   += pu.value;  break;
            case PowerUpEffectType.RepelEnemies:          stats.repelForce     += pu.value;  break;
            case PowerUpEffectType.IncreaseXPGain:        stats.xpMultiplier   += pu.value / 100f; break;
            case PowerUpEffectType.IncreaseIFrames:       stats.invincibilityDuration += pu.value; break;
            case PowerUpEffectType.MagnetAllOrbs:
                foreach (var orb in FindObjectsByType<XPOrb>(FindObjectsSortMode.None))
                    orb.ForceAttract();
                break;
        }
    }
}
