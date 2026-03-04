using UnityEngine;

/// <summary>
/// Manages XP accumulation and level-up thresholds.
/// Uses an exponential curve so each level requires more XP.
/// </summary>
public class XPManager : MonoBehaviour
{
    [Header("XP Curve")]
    [Tooltip("XP required to reach level 2")]
    public float baseXP = 50f;
    [Tooltip("Multiplier applied per level (e.g. 1.25 = 25% more each level)")]
    public float xpMultiplier = 1.25f;

    public int   CurrentLevel { get; private set; } = 1;
    public float CurrentXP   { get; private set; }
    public float RequiredXP  => Mathf.Floor(baseXP * Mathf.Pow(xpMultiplier, CurrentLevel - 1));

    private PlayerStats _playerStats;

    private void Awake()
    {
        var p = GameObject.FindWithTag("Player");
        if (p) _playerStats = p.GetComponent<PlayerStats>();
    }

    public void AddXP(float amount)
    {
        if (_playerStats == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) _playerStats = p.GetComponent<PlayerStats>();
        }

        float mult = _playerStats != null ? _playerStats.xpMultiplier : 1f;
        CurrentXP += amount * mult;

        EventBus.RaiseXPChanged(CurrentXP, RequiredXP);

        while (CurrentXP >= RequiredXP)
        {
            CurrentXP  -= RequiredXP;
            CurrentLevel++;
            EventBus.RaisePlayerLevelUp(CurrentLevel);
            // Trigger level-up UI
            FindFirstObjectByType<LevelUpManager>()?.TriggerLevelUp();
        }

        // Fire one more XP update after possible level-ups
        EventBus.RaiseXPChanged(CurrentXP, RequiredXP);
    }
}
