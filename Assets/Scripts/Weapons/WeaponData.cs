using UnityEngine;

/// <summary>
/// ScriptableObject defining a weapon's per-level stats.
/// Create instances via: Assets > Create > MoneySurvivor > WeaponData
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "MoneySurvivor/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName    = "Coin Toss";
    public string description   = "Throws coins in all directions.";
    public Sprite icon;

    [Header("Per-Level Stats (index 0 = level 1)")]
    public WeaponLevelStats[] levels;
}

[System.Serializable]
public class WeaponLevelStats
{
    public float damage       = 10f;
    public float fireRate     = 1f;   // attacks per second
    public float projectileSpeed = 8f;
    public int   projectileCount = 4;
    public int   pierceCount  = 1;
    public float aoeRadius    = 0f;   // used by AoE weapons
    public float duration     = 0f;   // lifetime in seconds (0 = instant)
}
