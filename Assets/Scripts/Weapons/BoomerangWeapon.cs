using UnityEngine;

/// <summary>
/// Fires one or more BoomerangProjectiles (Credit Cards) outward.
/// </summary>
public class BoomerangWeapon : WeaponBase
{
    public GameObject boomerangPrefab;

    protected override void Activate()
    {
        if (boomerangPrefab == null) return;

        var stats = CurrentStats;
        int count = stats.projectileCount;
        
        // Spread projectiles evenly in a circle around the player
        float angleStep = 360f / count;
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            var go = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);
            var bProj = go.GetComponent<BoomerangProjectile>();
            if (bProj != null)
            {
                bProj.Fire(stats, dir, transform);
            }
        }
    }
}
