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

    private int   _activationCount;
    private float _showWhipTime;
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
        _showWhipTime = 0.15f; 

        // Hit all enemies in arc
        var hits = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (var hit in hits)
        {
            Vector2 toEnemy = (hit.transform.position - transform.position).normalized;
            float dot = Vector2.Dot(toEnemy, Quaternion.Euler(0, 0, _whipFacing) * Vector2.right);
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;

            if (Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) <= halfAngle)
            {
                if (hit.TryGetComponent<EnemyBase>(out var enemy))
                {
                    enemy.TakeDamage(GetDamage());
                }
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (_showWhipTime > 0f) _showWhipTime -= Time.deltaTime;
    }

    private void OnGUI()
    {
        if (_showWhipTime <= 0f || Camera.main == null) return;

        // Draw a flash of color to represent the whip swipe
        Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        float   screenRange     = _whipRange * (Screen.height / (Camera.main.orthographicSize * 2f)); // approximate screen pixels
        
        // Offset box to left or right depending on facing
        float xOffset = _whipFacing > 0f ? 0 : -screenRange;

        Rect rect = new Rect(
            playerScreenPos.x + xOffset,
            Screen.height - playerScreenPos.y - (screenRange * 0.5f), // Invert Y
            screenRange,
            screenRange
        );

        GUI.color = new Color(0.1f, 0.8f, 0.2f, 0.5f); // transparent green
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void OnDrawGizmosSelected()
    {
        if (CurrentStats == null) return;
        float range = CurrentStats.aoeRadius > 0f ? CurrentStats.aoeRadius : 2.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
