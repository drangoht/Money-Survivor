using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bill Whip: sweeps an arc of bills around the player, damaging all enemies caught in it.
/// Uses OverlapCircle + angle filtering for the arc hit detection.
/// </summary>
public class BillWhip : WeaponBase
{
    [Header("Bill Whip Settings")]
    public float arcAngle = 120f;  // overridden by level stats via aoeRadius trick
    public GameObject whipPrefab;  // assigned by GameSetup (boomerang/bill sprite)

    private int   _activationCount;
    private float _whipFacing;
    private float _whipRange;

    protected override void Activate()
    {
        if (CurrentStats == null) return;

        // Use aoeRadius as whip range, angle scales with level
        float range = CurrentStats.aoeRadius > 0f ? CurrentStats.aoeRadius : 2.5f;
        float angle = arcAngle + (CurrentLevel * 20f); // wider per level

        // Alternate left / right each attack
        _whipFacing = (_activationCount % 2 == 0) ? 90f : -90f;
        _activationCount++;

        // Set up visual timer
        _whipRange = range;

        // Hit all enemies in arc and spawn visuals
        var hits = Physics2D.OverlapCircleAll(transform.position, range);
        Vector2 facingDir = Quaternion.Euler(0, 0, _whipFacing) * Vector2.right;
        float halfAngle = angle * 0.5f * Mathf.Deg2Rad;

        foreach (var hit in hits)
        {
            Vector2 toEnemy = (hit.transform.position - transform.position).normalized;
            float dot = Vector2.Dot(toEnemy, facingDir);

            if (Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) <= halfAngle)
            {
                if (hit.TryGetComponent<EnemyBase>(out var enemy))
                {
                    enemy.TakeDamage(GetDamage());
                }
            }
        }

        // Spawn visual arc of spinning bills (sweeping motion)
        if (whipPrefab != null)
        {
            int visualCount = 7 + CurrentLevel * 2; // spawn a few more for density
            float visualAngleStep = angle / (visualCount - 1);
            float startVisualAngle = _whipFacing - (angle / 2f);
            
            float sweepDuration = 0.2f; // total animation time
            float delayStep = sweepDuration / visualCount;

            for (int i = 0; i < visualCount; i++)
            {
                float a = (startVisualAngle + (i * visualAngleStep)) * Mathf.Deg2Rad;
                Vector2 arcPos = (Vector2)transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (range * 0.8f);
                var go = Instantiate(whipPrefab, arcPos, Quaternion.identity);
                
                // Increase scale to make it much more visible
                go.transform.localScale = Vector3.one * 1.5f;
                
                // Disable projectile logic since these are just visuals
                var pb = go.GetComponent<ProjectileBase>();
                if (pb != null) pb.enabled = false;
                var rb = go.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;
                var col = go.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                // Add a simple script to fade and destroy
                var fader = go.AddComponent<WhipVisualFader>();
                
                // Determine direction of sweep based on facing
                int orderIndex = _whipFacing > 0f ? i : (visualCount - 1 - i);
                
                fader.delay = orderIndex * delayStep;
                fader.duration = 0.35f; // stay alive slightly longer
            }
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    private void OnDrawGizmosSelected()
    {
        if (CurrentStats == null) return;
        float range = CurrentStats.aoeRadius > 0f ? CurrentStats.aoeRadius : 2.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
