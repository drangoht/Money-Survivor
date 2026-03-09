using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Compound Interest: a persistent aura around the player that deals DPS to
/// all enemies inside it. Grows larger and deals more damage each level.
/// </summary>
public class CompoundInterest : WeaponBase
{
    [Header("Compound Interest Settings")]
    public Color auraColor = new Color(0.2f, 1f, 0.4f, 0.3f);

    // Visual aura (optional decal/Circle sprite child)
    private SpriteRenderer _auraSprite;
    private readonly HashSet<EnemyBase> _enemiesInRange = new();

    protected override void Awake()
    {
        base.Awake();

        // Create a child visual circle for the aura
        var child = new GameObject("AuraVisual");
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;

        _auraSprite = child.AddComponent<SpriteRenderer>();
        _auraSprite.sprite        = CreateCircleSprite();
        _auraSprite.color         = auraColor;
        _auraSprite.sortingOrder  = -1;
        UpdateAuraScale();
    }

    protected override void Update()
    {
        base.Update(); // handles fire timer → calls Activate()
        UpdateAuraScale();
    }

    // Activate is called by the fire timer — deals a tick of damage
    protected override void Activate()
    {
        if (CurrentStats == null) return;

        // Base radius: scale from data, but make it clearly larger than the player sprite.
        // At level 1 this is roughly ~1.5× the player sprite width.
        float baseRadius = CurrentStats.aoeRadius > 0f ? CurrentStats.aoeRadius * 1.5f : 1.5f;
        float radius = baseRadius;
        radius += CurrentLevel * 0.02f; // very small growth per level

        // We lookup stats to get the global repel force
        var playerStats = GetComponentInParent<PlayerStats>();
        float extraRepel = playerStats != null ? playerStats.repelForce : 0f;

        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(GetDamage());
                // Repel: reduced so aura doesn’t trivialize positioning
                Vector2 repelDir = (enemy.transform.position - transform.position).normalized;
                enemy.ApplyKnockback(repelDir, 0.08f + extraRepel * 0.1f);
            }
        }
    }

    private void UpdateAuraScale()
    {
        if (_auraSprite == null || CurrentStats == null) return;
        float baseRadius = CurrentStats.aoeRadius > 0f ? CurrentStats.aoeRadius * 1.5f : 1.5f;
        float radius = baseRadius + CurrentLevel * 0.02f;
        _auraSprite.transform.localScale = Vector3.one * radius * 2f;
    }

    // Build a simple filled circle sprite at runtime (no texture asset needed)
    private static Sprite CreateCircleSprite()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float   radius = size / 2f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            float alpha = dist < radius ? 1f : 0f;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
