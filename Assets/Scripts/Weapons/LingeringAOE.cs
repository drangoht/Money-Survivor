using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A stationary object that deals tick damage to enemies within its trigger collider over time,
/// then destroys itself after its duration expires.
/// </summary>
public class LingeringAOE : MonoBehaviour
{
    private float _damage;
    private float _duration;
    private float _tickRate = 0.5f; // applies damage twice a second
    private float _tickTimer;
    private float _lifeTimer;

    private readonly HashSet<EnemyBase> _enemiesInZone = new();
    private readonly List<EnemyBase> _damageCache = new();
    private SpriteRenderer _sr;

    public void Initialize(float damage, float duration, float radius)
    {
        _damage = damage;
        _duration = duration;
        _lifeTimer = duration;

        // Set radius of the trigger collider
        var col = GetComponent<CircleCollider2D>();
        if (col != null) col.radius = radius;
        
        // Ensure scale is correct visually if needed (sprite is handled via setup)
    }

    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        // Add a pulsing visual effect to represent processing/mining heat
        gameObject.AddComponent<ScalePulse>();
    }

    private void Update()
    {
        _lifeTimer -= Time.deltaTime;
        
        // Fade out slightly as it approaches end of life
        if (_sr != null && _lifeTimer < 1f)
        {
            Color c = _sr.color;
            c.a = _lifeTimer;
            _sr.color = c;
        }

        if (_lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Apply tick damage
        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0f)
        {
            _tickTimer = _tickRate;
            ApplyDamageTick();
        }
    }

    private void ApplyDamageTick()
    {
        // Clean up dead enemies from hashset
        _enemiesInZone.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy || e.IsDead());

        _damageCache.Clear();
        _damageCache.AddRange(_enemiesInZone);

        foreach (var enemy in _damageCache)
        {
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(_damage);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<EnemyBase>(out var enemy))
        {
            _enemiesInZone.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<EnemyBase>(out var enemy))
        {
            _enemiesInZone.Remove(enemy);
        }
    }
}
