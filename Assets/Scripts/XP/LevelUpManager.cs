using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// When a level-up occurs, selects 3 random upgrade options from available
/// weapons and power-ups, presents them via the LevelUpUI, and applies the choice.
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    [Header("All available weapon prefabs (with WeaponBase)")]
    public List<GameObject> weaponPrefabs;

    [Header("All available power-up definitions")]
    public List<PowerUpData> powerUps;

    private LevelUpUI _ui;
    private PlayerStats _playerStats;

    // Track which weapons the player already has
    private readonly List<WeaponBase> _equippedWeapons = new();

    private void Awake()
    {
        _ui          = FindFirstObjectByType<LevelUpUI>(FindObjectsInactive.Include);
        _playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public void TriggerLevelUp()
    {
        var options = BuildOptions(3);
        if (options.Count == 0) { GameManager.Instance?.ResumeGame(); return; }

        GameManager.Instance?.EnterLevelUp();
        _ui?.Show(options, ApplyChoice);
    }

    // ── Option building ──────────────────────────────────────────────────────

    private List<UpgradeOption> BuildOptions(int count)
    {
        var candidates = new List<UpgradeOption>();

        // New weapons the player doesn't have yet
        var player = GameObject.FindWithTag("Player");
        if (player != null) _equippedWeapons.Clear();
        _equippedWeapons.AddRange(player != null ? player.GetComponentsInChildren<WeaponBase>() : System.Array.Empty<WeaponBase>());

        foreach (var prefab in weaponPrefabs)
        {
            var wb = prefab.GetComponent<WeaponBase>();
            if (wb == null || wb.data == null) continue;

            // Not yet equipped → offer as new weapon
            bool alreadyEquipped = _equippedWeapons.Any(e => e.data == wb.data);
            if (!alreadyEquipped)
            {
                candidates.Add(new UpgradeOption
                {
                    type       = UpgradeType.NewWeapon,
                    weaponPrefab = prefab,
                    data       = wb.data,
                    label      = wb.data.weaponName,
                    description = wb.data.description,
                });
                continue;
            }

            // Already equipped → offer upgrade if not maxed
            var equipped = _equippedWeapons.FirstOrDefault(e => e.data == wb.data);
            if (equipped != null && !equipped.IsMaxLevel)
            {
                candidates.Add(new UpgradeOption
                {
                    type       = UpgradeType.WeaponUpgrade,
                    weaponBase = equipped,
                    data       = wb.data,
                    label      = $"{wb.data.weaponName} (Lv {equipped.CurrentLevel + 2})",
                    description = wb.data.description,
                });
            }
        }

        // Power-ups
        foreach (var pu in powerUps)
            candidates.Add(new UpgradeOption { type = UpgradeType.PowerUp, powerUp = pu, label = pu.powerUpName, description = pu.description });

        // Shuffle and take N
        candidates = candidates.OrderBy(_ => Random.value).ToList();
        return candidates.Take(count).ToList();
    }

    // ── Applying choices ─────────────────────────────────────────────────────

    private void ApplyChoice(UpgradeOption option)
    {
        var player = GameObject.FindWithTag("Player");

        switch (option.type)
        {
            case UpgradeType.NewWeapon when player != null:
                // Instantiate the prefab as a child to keep all serialized fields (like projectile references)
                var spawnedW = Instantiate(option.weaponPrefab, player.transform);
                var wb       = spawnedW.GetComponent<WeaponBase>();
                if (wb != null) wb.data = option.data;
                break;

            case UpgradeType.WeaponUpgrade:
                option.weaponBase?.Upgrade();
                break;

            case UpgradeType.PowerUp:
                ApplyPowerUp(option.powerUp, player);
                break;
        }

        GameManager.Instance?.ResumeGame();
    }

    private void ApplyPowerUp(PowerUpData pu, GameObject player)
    {
        if (pu == null || player == null) return;
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        switch (pu.effectType)
        {
            case PowerUpEffectType.HealHP:            stats.Heal(pu.value);                break;
            case PowerUpEffectType.IncreaseMaxHP:     stats.IncreaseMaxHP(pu.value);       break;
            case PowerUpEffectType.IncreaseSpeed:     stats.moveSpeed      += pu.value;    break;
            case PowerUpEffectType.IncreaseDamage:    stats.damageMultiplier += pu.value / 100f; break;
            case PowerUpEffectType.IncreasePickupRadius: stats.pickupRadius += pu.value;   break;
            case PowerUpEffectType.MagnetAllOrbs:
                foreach (var orb in FindObjectsByType<XPOrb>(FindObjectsSortMode.None))
                    orb.ForceAttract();
                break;
        }
    }
}

// ── Data structures ──────────────────────────────────────────────────────────

public enum UpgradeType { NewWeapon, WeaponUpgrade, PowerUp }

public class UpgradeOption
{
    public UpgradeType  type;
    public string       label;
    public string       description;
    // Weapon
    public GameObject   weaponPrefab;
    public WeaponBase   weaponBase;
    public WeaponData   data;
    // PowerUp
    public PowerUpData  powerUp;
}
