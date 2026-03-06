using UnityEngine;

public enum PowerUpEffectType
{
    HealHP,
    IncreaseMaxHP,
    IncreaseSpeed,
    IncreaseDamage,
    IncreasePickupRadius,
    MagnetAllOrbs,
    ExtraLevelUpChoice,
    RepelEnemies,
    IncreaseXPGain,
    IncreaseIFrames
}

/// <summary>
/// ScriptableObject defining a chest power-up.
/// Create instances via: Assets > Create > MoneySurvivor > PowerUpData
/// </summary>
[CreateAssetMenu(fileName = "NewPowerUp", menuName = "MoneySurvivor/PowerUpData")]
public class PowerUpData : ScriptableObject
{
    [Header("Identity")]
    public string powerUpName = "Gold Rush";
    public string description = "Increase move speed.";
    public Sprite icon;
    public Color  cardColor   = new Color(1f, 0.84f, 0f); // gold

    [Header("Effect")]
    public PowerUpEffectType effectType;
    public float value = 10f; // meaning depends on effectType
}
