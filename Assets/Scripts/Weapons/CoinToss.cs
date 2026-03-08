using UnityEngine;

/// <summary>
/// Coin Toss: fires N coins outward in evenly-spaced directions.
/// At higher levels: more coins, more pierce, more damage.
/// </summary>
public class CoinToss : WeaponBase
{
    [Header("Coin Toss Settings")]
    public GameObject coinPrefab;
    public Color coinColor = new Color(1f, 0.84f, 0f);

    protected override void Activate()
    {
        if (coinPrefab == null || CurrentStats == null) return;

        int   count     = GetProjectileCount(CurrentStats.projectileCount);
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Direct instantiate — no pool setup required
            var coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

            var sr = coin.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = coinColor;

            var proj = coin.GetComponent<ProjectileBase>();
            if (proj != null)
            {
                // Override damage cleanly to include PlayerStats modifier
                var stats = CurrentStats;
                proj.Initialize(stats, dir);
                proj.damage = GetDamage(); 
            }
        }
    }
}
