using UnityEngine;

/// <summary>
/// Projectile fired by CoinToss and similar weapons.
/// Moves in a straight line, pierces enemies up to pierceCount times, then despawns.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class ProjectileBase : MonoBehaviour, IPoolable
{
    /// <summary>Global count of active projectiles (for soft caps / performance).</summary>
    public static int ActiveCount = 0;

    [Header("Homing")]
    [Tooltip("If true, projectile will continuously steer toward homingTarget.")]
    public bool homing = false;
    public Transform homingTarget;

    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public int   pierceCount;
    [HideInInspector] public float lifetime;
    [HideInInspector] public string poolTag;

    private Rigidbody2D _rb;
    private int         _pierceRemaining;
    private float       _lifeTimer;
    private float       _damageTickTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale   = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        _rb.interpolation = RigidbodyInterpolation2D.None;

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    // IPoolable requirements
    public virtual void OnSpawn() { }
    public virtual void OnDespawn()
    {
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
    }

    /// <summary>Call right after instantiating to set properties.</summary>
    public virtual void Initialize(WeaponLevelStats stats, Vector2 direction)
    {
        ActiveCount++;

        damage      = stats.damage;
        speed       = stats.projectileSpeed;
        pierceCount = stats.pierceCount;
        lifetime    = stats.duration > 0f ? stats.duration : 5f;
        
        _pierceRemaining = pierceCount > 0 ? pierceCount : 999;
        _lifeTimer       = lifetime;

        if (direction != Vector2.zero && _rb != null)
            _rb.linearVelocity = direction.normalized * speed;
    }

    protected virtual void Update()
    {
        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
        {
            Despawn();
            return;
        }

        // Cull far-off projectiles to avoid simulating useless ones off-screen.
        var cam = Camera.main;
        if (cam != null)
        {
            float maxDist = 60f;
            if (Vector2.SqrMagnitude((Vector2)transform.position - (Vector2)cam.transform.position) > maxDist * maxDist)
            {
                Despawn();
                return;
            }
        }

        // Simple homing: continuously aim velocity at target if set.
        if (homing && homingTarget != null && _rb != null)
        {
            Vector2 dir = ((Vector2)homingTarget.position - _rb.position).normalized;
            _rb.linearVelocity = dir * speed;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageEnemy(other.GetComponent<EnemyBase>());
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        // For continuous hit weapons like Orbital Shield
        _damageTickTimer -= Time.deltaTime;
        if (_damageTickTimer <= 0f)
        {
            TryDamageEnemy(other.GetComponent<EnemyBase>());
            _damageTickTimer = 0.5f; // hit twice a second
        }
    }

    private void TryDamageEnemy(EnemyBase enemy)
    {
        if (enemy == null) return;

        enemy.TakeDamage(damage);

        // Repel / Knockback
        if (enemy.TryGetComponent<Rigidbody2D>(out var enemyRb) && _rb != null)
        {
            Vector2 pushDir = (enemy.transform.position - transform.position).normalized;
            enemyRb.AddForce(pushDir * 15f, ForceMode2D.Impulse);
        }

        if (pierceCount < 99) // Infinite pierce weapons shouldn't count down
        {
            _pierceRemaining--;
            if (_pierceRemaining <= 0) Despawn();
        }
    }

    private void Despawn()
    {
        if (ActiveCount > 0) ActiveCount--;

        if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolTag))
            ObjectPool.Instance.Return(poolTag, gameObject);
        else
            Destroy(gameObject);
    }
}
