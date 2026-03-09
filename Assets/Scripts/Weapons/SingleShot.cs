using System.Collections.Generic;
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

        // Get current enemies from registry once
        var enemyList = new List<EnemyBase>(EnemyRegistry.Enemies);

        // Fallback direction if no enemies exist
        Vector2 baseDir = Vector2.right;
        if (enemyList.Count > 0)
        {
            // Aim roughly at closest for fallback direction
            float minDist = float.MaxValue;
            foreach (var e in enemyList)
            {
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    baseDir = (e.transform.position - transform.position).normalized;
                }
            }
        }

        int count = GetProjectileCount(CurrentStats.projectileCount);
        float spreadAngle = 15f; // degrees between shots
        
        float startAngle = -(spreadAngle * (count - 1)) / 2f;

        for (int i = 0; i < count; i++)
        {
            // Choose a target for this projectile if available (different bullets can follow different enemies)
            Transform target = null;
            if (enemyList.Count > 0)
            {
                float best = float.MaxValue;
                int bestIndex = -1;
                for (int ei = 0; ei < enemyList.Count; ei++)
                {
                    var e = enemyList[ei];
                    if (e == null) continue;
                    float d = Vector2.Distance(transform.position, e.transform.position);
                    if (d < best)
                    {
                        best = d;
                        bestIndex = ei;
                    }
                }
                if (bestIndex >= 0)
                {
                    target = enemyList[bestIndex].transform;
                    enemyList.RemoveAt(bestIndex); // next projectile can pick another enemy
                }
            }

            // Calculate direction with spread
            float currentAngle = startAngle + (i * spreadAngle);
            Vector2 baseDirection = baseDir;
            if (target != null)
                baseDirection = (target.position - transform.position).normalized;

            Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * baseDirection;

            ProjectileSpawner.SpawnProjectile(
                projectilePrefab,
                transform.position,
                dir,
                CurrentStats,
                GetDamage(),
                color,
                target,
                target != null);
        }
    }
}
