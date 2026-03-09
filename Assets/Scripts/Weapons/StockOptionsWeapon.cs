using UnityEngine;

public class StockOptionsWeapon : WeaponBase
{
    [Header("Stock Options Settings")]
    public GameObject arrowPrefab;

    protected override void Activate()
    {
        if (arrowPrefab == null || CurrentStats == null) return;

        int count = GetProjectileCount(CurrentStats.projectileCount);

        // Use central registry instead of scene-wide search
        var enemies = EnemyRegistry.Enemies;
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

        Vector2 baseDir = Vector2.right;
        if (closest != null)
        {
            baseDir = (closest.position - transform.position).normalized;
        }

        float spreadAngle = 15f;
        float startAngle = -(spreadAngle * (count - 1)) / 2f;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (i * spreadAngle);
            Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * baseDir;

            // Orient projectile to face movement direction
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var go = ProjectileSpawner.SpawnProjectile(
                arrowPrefab,
                transform.position,
                dir,
                CurrentStats,
                GetDamage());
            if (go != null)
                go.transform.rotation = Quaternion.Euler(0, 0, angleDeg);
        }
    }
}
