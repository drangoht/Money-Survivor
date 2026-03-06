using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns projectiles that orbit around the player.
/// Continuously active; the count and radius are dictated by stats.
/// </summary>
public class OrbitalWeapon : WeaponBase
{
    public GameObject orbitalPrefab;

    private readonly List<GameObject> _orbitals = new();
    private float _currentAngle;

    protected override void Activate()
    {
        // Orbital weapons don't "fire" periodically, they are always active.
        // We handle logic in Update.
    }

    protected override void Update()
    {
        base.Update();
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        UpdateOrbitals();

        var stats = CurrentStats;
        if (stats == null) return;

        // Rotate the entire formation
        _currentAngle += 360f / stats.fireRate * Time.deltaTime; // fireRate acts as rotation speed here
        _currentAngle %= 360f;

        float angleStep = 360f / _orbitals.Count;

        for (int i = 0; i < _orbitals.Count; i++)
        {
            var orb = _orbitals[i];
            if (orb == null) continue;

            float angle = (_currentAngle + (i * angleStep)) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * stats.aoeRadius;
            
            orb.transform.position = (Vector2)transform.position + offset;
            orb.transform.Rotate(0, 0, 360f * Time.deltaTime); // Spin the projectile itself
        }
    }

    private void UpdateOrbitals()
    {
        var stats = CurrentStats;
        if (stats == null || orbitalPrefab == null) return;

        int targetCount = stats.projectileCount > 0 ? stats.projectileCount : 1;

        // Add missing
        while (_orbitals.Count < targetCount)
        {
            var go = Instantiate(orbitalPrefab, transform.position, Quaternion.identity);
            // Disable Rigidbody physics so it doesn't fight our manual transform orbiting
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            var proj = go.GetComponent<ProjectileBase>();
            if (proj != null)
            {
                // Give orbitals infinite lifetime so they don't despawn
                stats.duration = 9999f; 
                proj.Initialize(stats, Vector2.zero);
            }
            _orbitals.Add(go);
        }

        // Remove excess (if we somehow downgraded, or just cleanup)
        while (_orbitals.Count > targetCount)
        {
            var last = _orbitals[_orbitals.Count - 1];
            if (last != null) Destroy(last);
            _orbitals.RemoveAt(_orbitals.Count - 1);
        }
        
        // Update stats on existing
        foreach(var o in _orbitals)
        {
            if (o == null) continue;
            var proj = o.GetComponent<ProjectileBase>();
            if (proj != null) proj.Initialize(stats, Vector2.zero);
        }
    }

    private void OnDestroy()
    {
        // Cleanup visuals if the weapon is removed
        foreach (var o in _orbitals)
        {
            if (o != null) Destroy(o);
        }
    }
}
