using UnityEngine;

/// <summary>
/// Abstract base class for all weapons. Handles fire-rate timing.
/// Child classes implement Activate() with the actual attack logic.
/// Weapons are components added to the Player GameObject.
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Definition")]
    public WeaponData data;

    public int  CurrentLevel { get; private set; } = 0; // 0-indexed → level 1
    public int  MaxLevel     => data != null ? data.levels.Length : 1;
    public bool IsMaxLevel   => CurrentLevel >= MaxLevel - 1;

    protected WeaponLevelStats CurrentStats =>
        data != null ? data.levels[Mathf.Min(CurrentLevel, data.levels.Length - 1)] : null;

    protected PlayerStats _playerStats;
    private float _fireTimer;

    protected virtual void Awake()
    {
        _playerStats = GetComponentInParent<PlayerStats>();
        if (data != null && data.levels.Length > 0)
            _fireTimer = 1f / data.levels[0].fireRate; // start ready after one interval
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameState.Playing) return;
        if (_playerStats != null && !_playerStats.IsAlive) return;
        if (CurrentStats == null) return;

        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            _fireTimer = 1f / CurrentStats.fireRate;
            Activate();
        }
    }

    /// <summary>Override in child classes to implement the attack.</summary>
    protected abstract void Activate();

    /// <summary>Called when the player selects this weapon's upgrade card.</summary>
    public virtual void Upgrade()
    {
        if (!IsMaxLevel)
        {
            CurrentLevel++;
            Debug.Log($"[{data.weaponName}] upgraded to level {CurrentLevel + 1}");
        }
    }

    protected float GetDamage()
    {
        float mult = _playerStats != null ? _playerStats.damageMultiplier : 1f;
        return CurrentStats.damage * mult;
    }
}
