using UnityEngine;

/// <summary>
/// Small helper/factory to create and initialize projectiles in a consistent way.
/// Centralizes Instantiate vs pool usage, basic setup, and optional coloring/homing.
/// </summary>
public static class ProjectileSpawner
{
    public static GameObject SpawnProjectile(
        GameObject prefab,
        Vector3 position,
        Vector2 direction,
        WeaponLevelStats stats,
        float damageOverride,
        Color? tint = null,
        Transform homingTarget = null,
        bool homing = false)
    {
        if (prefab == null || stats == null) return null;

        var go = Object.Instantiate(prefab, position, Quaternion.identity);

        // Optional color tint
        if (tint.HasValue)
        {
            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = tint.Value;
        }

        // Basic projectile setup
        if (go.TryGetComponent<ProjectileBase>(out var proj))
        {
            proj.Initialize(stats, direction);
            if (damageOverride > 0f) proj.damage = damageOverride;
            proj.homing = homing && homingTarget != null;
            proj.homingTarget = homingTarget;
        }

        return go;
    }
}

