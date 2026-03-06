using UnityEngine;

public class VolatileProjectile : ProjectileBase
{
    private float _baseDamage;

    public override void Initialize(WeaponLevelStats stats, Vector2 direction)
    {
        base.Initialize(stats, direction);
        _baseDamage = stats.damage;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Randomize damage between 50% and 150% of base
        float multiplier = Random.Range(0.5f, 1.5f);
        damage = _baseDamage * multiplier;

        // Apply a visual visual scaling based on damage volatile state
        if (multiplier > 1.25f)
        {
            transform.localScale = Vector3.one * 1.3f; // big hit
        }
        else if (multiplier < 0.75f)
        {
            transform.localScale = Vector3.one * 0.7f; // small hit
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        // Parent handles collision using the mutated `damage` variable
        base.OnTriggerEnter2D(other);
    }
}
