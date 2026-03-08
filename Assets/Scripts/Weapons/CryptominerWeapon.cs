using UnityEngine;

/// <summary>
/// Drops a stationary Cryptominer rig at the player's position periodically.
/// At higher levels, it drops multiple rigs in a cluster.
/// </summary>
public class CryptominerWeapon : WeaponBase
{
    [Header("Cryptominer Settings")]
    public GameObject minerPrefab;

    protected override void Activate()
    {
        if (minerPrefab == null || CurrentStats == null) return;

        int count = GetProjectileCount(CurrentStats.projectileCount > 0 ? CurrentStats.projectileCount : 1);
        float radius = CurrentStats.aoeRadius > 0f ? CurrentStats.aoeRadius : 2f;
        float spreadDist = 1.0f; // Distance from player when dropping multiple

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position;
            
            // If spawning multiple, arrange them in a small circle around the player
            if (count > 1)
            {
                float angle = (i * 360f / count) * Mathf.Deg2Rad;
                spawnPos += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spreadDist;
            }

            var rig = Instantiate(minerPrefab, spawnPos, Quaternion.identity);
            
            if (rig.TryGetComponent<LingeringAOE>(out var aoe))
            {
                aoe.Initialize(GetDamage(), CurrentStats.duration, radius);
            }
        }
    }
}
