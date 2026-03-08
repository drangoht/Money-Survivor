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
    /// <summary>Multiplier applied to weapon projectile count (e.g. 1.25 = +25% projectiles).</summary>
    public float projectileCountMultiplier = 1f;

    [Header("Runtime")]
    public float CurrentHP { get; private set; }
    public bool  IsAlive   { get; private set; } = true;
    public bool  IsInvincible { get; private set; }

    private float _invincibilityTimer;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();

        CurrentHP = maxHP;
        EventBus.RaisePlayerHPChanged(CurrentHP, maxHP);
    }

    private void Update()
    {
        if (IsInvincible)
        {
            _invincibilityTimer -= Time.deltaTime;

            if (_sr != null)
            {
                _sr.enabled = Mathf.PingPong(Time.time * 15f, 1f) > 0.5f;
            }

            if (_invincibilityTimer <= 0f) 
            {
                IsInvincible = false;
                if (_sr != null) _sr.enabled = true;
            }
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
