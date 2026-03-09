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

    [Header("Juice (optional)")]
    [Tooltip("Particle burst spawned at player when level-up screen appears.")]
    public GameObject levelUpBurstPrefab;

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
        EventBus.RaiseLevelUpScreenShown();
        if (levelUpBurstPrefab != null && _playerStats != null)
        {
            var burst = Object.Instantiate(levelUpBurstPrefab, _playerStats.transform.position, Quaternion.identity);
            Object.Destroy(burst, 1.5f);
        }
        _ui?.Show(options, ApplyChoice, () => BuildOptions(3));
    }

    // ── Option building ──────────────────────────────────────────────────────

    /// <summary>Builds the full list of possible rewards (new weapons, weapon upgrades, power-ups).</summary>
    public List<UpgradeOption> BuildCandidates()
    {
        var candidates = new List<UpgradeOption>();

        var player = GameObject.FindWithTag("Player");
        if (player != null) _equippedWeapons.Clear();
        _equippedWeapons.AddRange(player != null ? player.GetComponentsInChildren<WeaponBase>() : System.Array.Empty<WeaponBase>());

        bool atMaxWeapons = _equippedWeapons.Count >= 3;

        foreach (var prefab in weaponPrefabs)
        {
            if (prefab == null) continue;
            var wb = prefab.GetComponent<WeaponBase>();
            if (wb == null || wb.data == null) continue;

            bool alreadyEquipped = _equippedWeapons.Any(e => e.data == wb.data);
            if (!alreadyEquipped)
            {
                if (!atMaxWeapons)
                {
                    candidates.Add(new UpgradeOption
                    {
                        type       = UpgradeType.NewWeapon,
                        weaponPrefab = prefab,
                        data       = wb.data,
                        label      = wb.data.weaponName,
                        description = wb.data.description,
                    });
                }
                continue;
            }

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

        foreach (var pu in powerUps)
        {
            if (pu == null) continue;
            candidates.Add(new UpgradeOption { type = UpgradeType.PowerUp, powerUp = pu, label = pu.powerUpName, description = pu.description });
        }

        return candidates.OrderBy(_ => Random.value).ToList();
    }

    private List<UpgradeOption> BuildOptions(int count)
    {
        var candidates = BuildCandidates();
        return candidates.Take(count).ToList();
    }

    /// <summary>Returns one random possible reward for chests (weapon, weapon upgrade, or power-up). Null if none available.</summary>
    public UpgradeOption GetRandomChestReward()
    {
        var candidates = BuildCandidates();
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    /// <summary>Applies a reward (used by chest and level-up). Does not resume game.</summary>
    public void ApplyReward(UpgradeOption option)
    {
        var player = GameObject.FindWithTag("Player");

        switch (option.type)
        {
            case UpgradeType.NewWeapon when player != null:
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
    }

    // ── Applying choices ─────────────────────────────────────────────────────

    private void ApplyChoice(UpgradeOption option)
    {
        ApplyReward(option);
        EventBus.RaiseRewardApplied(option);
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
            case PowerUpEffectType.RepelEnemies:      stats.repelForce     += pu.value;    break;
            case PowerUpEffectType.IncreaseXPGain:    stats.xpMultiplier   += pu.value / 100f; break;
            case PowerUpEffectType.IncreaseIFrames:   stats.invincibilityDuration += pu.value; break;
            case PowerUpEffectType.IncreaseProjectileCount: stats.projectileCountMultiplier += pu.value / 100f; break;
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
