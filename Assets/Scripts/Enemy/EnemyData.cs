using UnityEngine;

/// <summary>
/// ScriptableObject defining a single enemy type's stats and visuals.
/// Create instances via: Assets > Create > MoneySurvivor > EnemyData
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "MoneySurvivor/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Bankman";
    public Color  bodyColor = Color.blue;

    [Header("Stats")]
    public float hp            = 30f;
    public float moveSpeed     = 1.5f;
    public float contactDamage = 5f;
    public int   xpValue       = 10;

    [Header("Scaling")]
    [Tooltip("HP multiplier applied per difficulty tier (every 30s)")]
    public float hpScaleFactor     = 1.1f;
    [Tooltip("Speed multiplier applied per difficulty tier")]
    public float speedScaleFactor  = 1.05f;
}
