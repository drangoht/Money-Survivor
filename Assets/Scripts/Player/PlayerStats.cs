using UnityEngine;

/// <summary>
/// All modifiable stats for the player. Other systems read from here.
/// Power-ups and weapon upgrades can modify these values directly.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHP          = 100f;
    public float moveSpeed      = 5f;
    public float damageMultiplier = 1f;
    public float pickupRadius   = 2.5f;
    public float xpMultiplier   = 1f;
    public float invincibilityDuration = 0.8f;
    public float repelForce     = 0f;

    [Header("Runtime")]
    public float CurrentHP { get; private set; }
    public bool  IsAlive   { get; private set; } = true;
    public bool  IsInvincible { get; private set; }

    private float _invincibilityTimer;

    private void Awake()
    {
        CurrentHP = maxHP;
        EventBus.RaisePlayerHPChanged(CurrentHP, maxHP);
    }

    private void Update()
    {
        if (IsInvincible)
        {
            _invincibilityTimer -= Time.deltaTime;
            if (_invincibilityTimer <= 0f) IsInvincible = false;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || IsInvincible) return;

        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        EventBus.RaisePlayerHPChanged(CurrentHP, maxHP);

        // Flash invincibility
        IsInvincible      = true;
        _invincibilityTimer = invincibilityDuration;

        if (CurrentHP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        EventBus.RaisePlayerHPChanged(CurrentHP, maxHP);
    }

    public void IncreaseMaxHP(float amount)
    {
        maxHP     += amount;
        CurrentHP += amount; // also heal by the same amount
        EventBus.RaisePlayerHPChanged(CurrentHP, maxHP);
    }

    private void Die()
    {
        IsAlive = false;
        EventBus.RaisePlayerDeath();
    }
}
