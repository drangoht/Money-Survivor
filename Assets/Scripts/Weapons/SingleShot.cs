using UnityEngine;

/// <summary>
/// Fires a single projectile aimed at the closest enemy.
/// At higher levels, it can fire multiple projectiles in a slight spread towards them.
/// /// </summary>
public class SingleShot : WeaponBase
{
    [Header("Single Shot Settings")]
    public GameObject projectilePrefab;
    public Color color = new Color(0.8f, 0.8f, 1f);

    protected override void Activate()
    {
        if (projectilePrefab == null || CurrentStats == null) return;

        // Find closest enemy
        var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var e in enemies)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = e.transform;
            }
        }

        Vector2 baseDir = Vector2.right; // fallback
        if (closest != null)
        {
            baseDir = (closest.position - transform.position).normalized;
        }

        int count = CurrentStats.projectileCount;
        float spreadAngle = 15f; // degrees between shots
        
        float startAngle = -(spreadAngle * (count - 1)) / 2f;

        for (int i = 0; i < count; i++)
        {
            // Calculate direction with spread
            float currentAngle = startAngle + (i * spreadAngle);
            Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * baseDir;

            var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = color;

            var proj = go.GetComponent<ProjectileBase>();
            if (proj != null)
            {
                proj.Initialize(CurrentStats, dir);
                proj.damage = GetDamage(); 
            }
        }
    }
}
