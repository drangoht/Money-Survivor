using UnityEngine;

/// <summary>
/// Base enemy behaviour: move toward the player, deal contact damage, die and drop XP.
/// Attach an EnemyData ScriptableObject in the Inspector.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyBase : MonoBehaviour, IPoolable
{
    [Header("Data")]
    public EnemyData data;
    public GameObject xpOrbPrefab; // assign in Inspector or via GameSetup
    public GameObject hitParticlePrefab; // assign via GameSetup
    public GameObject chestPrefab; // assigned globally via GameSetup

    // Runtime
    private float        _currentHP;
    private int          _difficultyTier; // set by spawner
    private float        _timeStrengthMult = 1f; // scales with elapsed time (set by spawner)
    private Rigidbody2D  _rb;
    private Transform    _playerTransform;
    private SpriteRenderer _sr;
    private bool         _isDead;

    private float        _knockbackTimer;
    private Vector2      _knockbackVelocity;

    // Pool tag (set by spawner so we can return to the correct pool)
    [HideInInspector] public string poolTag;

    // ── IPoolable ────────────────────────────────────────────────────────────

    public void OnSpawn()
    {
        _isDead = false;
        float hpMult = Mathf.Pow(data.hpScaleFactor, _difficultyTier) * _timeStrengthMult;
        _currentHP   = data.hp * hpMult;

        if (_sr != null) _sr.color = data.bodyColor;
    }

    public void OnDespawn() { }

    // ── Unity lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale   = 0f;
        _rb.freezeRotation = true;

        _sr = GetComponentInChildren<SpriteRenderer>();

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        // Fallback initialization if OnSpawn wasn't called (e.g., initial spawns)
        if (_currentHP <= 0f && data != null)
        {
            float hpMult = Mathf.Pow(data.hpScaleFactor, _difficultyTier) * _timeStrengthMult;
            _currentHP   = data.hp * hpMult;
            if (_sr != null) _sr.color = data.bodyColor;
        }
    }

    private void Update()
    {
        if (_isDead) return;

        if (_knockbackTimer > 0f)
        {
            // Apply knockback velocity instead of normal AI movement
            _rb.linearVelocity = _knockbackVelocity;
            _knockbackTimer -= Time.deltaTime;
            return;
        }

        // Lazy player lookup
        if (_playerTransform == null)
        {
            var ps = FindFirstObjectByType<PlayerStats>();
            if (ps == null) return;
            _playerTransform = ps.transform;
        }

        float speedMult = Mathf.Pow(data.speedScaleFactor, _difficultyTier);
        Vector2 dir = ((Vector2)_playerTransform.position - _rb.position).normalized;
        _rb.linearVelocity = dir * (data.moveSpeed * speedMult * Mathf.Sqrt(_timeStrengthMult));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_isDead) return;
        var stats = other.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.TakeDamage(data.contactDamage * _timeStrengthMult);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void SetDifficultyTier(int tier) => _difficultyTier = tier;

    /// <summary>Scale HP and contact damage; steps up every 2 minutes. Call after SetDifficultyTier when spawning.</summary>
    public void SetTimeStrength(float timeSurvived)
    {
        int twoMinuteSteps = Mathf.FloorToInt(timeSurvived / 120f); // 0–2 min: 0, 2–4 min: 1, etc.
        _timeStrengthMult = 1f + twoMinuteSteps * 0.25f; // +25% per 2 minutes
    }

    public bool IsDead() => _isDead;

    public void ApplyKnockback(Vector2 direction, float force, float duration = 0.25f)
    {
        if (_isDead) return;
        _knockbackTimer = duration;
        _knockbackVelocity = direction.normalized * force;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        EventBus.RaiseEnemyHit(transform.position);
        _currentHP -= amount;

        // White hit flash
        if (_sr != null) StartCoroutine(DamageFlash());
        
        // Spawn colored hit particles based on EnemyData
        if (hitParticlePrefab != null)
        {
            var parts = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            var main = parts.GetComponent<ParticleSystem>().main;
            main.startColor = data != null ? data.hitParticleColor : Color.white;
        }

        if (_currentHP <= 0f) Die();
    }

    private void Die()
    {
        _isDead = true;
        EventBus.RaiseEnemyKilled(transform.position, data.xpValue);

        if (xpOrbPrefab != null)
        {
            var orbGO = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
            if (orbGO.TryGetComponent<XPOrb>(out var orbComponent))
            {
                orbComponent.SetXP(data.xpValue);
            }
        }

        if (data != null && data.isBoss && chestPrefab != null)
        {
            var chestGO = Instantiate(chestPrefab, transform.position, Quaternion.identity);
            // MegaBoss chest: roll 3 rewards and show them in a single notification.
            if (data.enemyName == "MegaBoss" && chestGO.TryGetComponent<Chest>(out var chest))
                chest.rewardCount = 3;
        }

        if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolTag))
            ObjectPool.Instance.Return(poolTag, gameObject);
        else
            Destroy(gameObject);
    }

    // White flash on hit (more readable on coloured sprites)
    private System.Collections.IEnumerator DamageFlash()
    {
        if (_sr == null) yield break;
        var original = _sr.color;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.07f);
        _sr.color = original;
    }
}
