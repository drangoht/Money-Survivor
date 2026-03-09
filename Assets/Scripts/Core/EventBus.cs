using System;
using UnityEngine;

/// <summary>
/// Static event hub for decoupled communication between all game systems.
/// </summary>
public static class EventBus
{
    // Player events
    public static event Action OnPlayerDeath;
    public static event Action<int> OnPlayerLevelUp;          // param: new level
    public static event Action<float, float> OnPlayerHPChanged; // current, max
    public static event Action<float, float> OnXPChanged;     // current, required

    // Enemy events
    public static event Action<Vector3, int> OnEnemyKilled;   // position, xpValue

    // Chest events (rewards are the chosen upgrade options: weapon, weapon upgrade, or power-up)
    public static event Action<System.Collections.Generic.List<UpgradeOption>> OnChestOpened;

    // Sound / gameplay events (for SFX)
    public static event Action<Vector3> OnWeaponFired;       // position (e.g. player)
    public static event Action<Vector3> OnEnemyHit;         // hit position
    public static event Action OnLevelUpScreenShown;        // when level-up card UI is displayed
    public static event Action<UpgradeOption> OnRewardApplied; // after applying a reward (weapon / upgrade / power-up)

    // Game state events
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    public static void RaisePlayerDeath()         => OnPlayerDeath?.Invoke();
    public static void RaisePlayerLevelUp(int lvl)=> OnPlayerLevelUp?.Invoke(lvl);
    public static void RaisePlayerHPChanged(float cur, float max) => OnPlayerHPChanged?.Invoke(cur, max);
    public static void RaiseXPChanged(float cur, float req)       => OnXPChanged?.Invoke(cur, req);
    public static void RaiseEnemyKilled(Vector3 pos, int xp)      => OnEnemyKilled?.Invoke(pos, xp);
    public static void RaiseChestOpened(System.Collections.Generic.List<UpgradeOption> data) => OnChestOpened?.Invoke(data);
    public static void RaiseWeaponFired(Vector3 pos)            => OnWeaponFired?.Invoke(pos);
    public static void RaiseEnemyHit(Vector3 pos)              => OnEnemyHit?.Invoke(pos);
    public static void RaiseLevelUpScreenShown()               => OnLevelUpScreenShown?.Invoke();
    public static void RaiseRewardApplied(UpgradeOption data)   => OnRewardApplied?.Invoke(data);
    public static void RaiseGameStart()           => OnGameStart?.Invoke();
    public static void RaiseGameOver()            => OnGameOver?.Invoke();
    public static void RaiseGamePaused()          => OnGamePaused?.Invoke();
    public static void RaiseGameResumed()         => OnGameResumed?.Invoke();

    /// <summary>Clear all listeners — call on scene unload to avoid stale references.</summary>
    public static void ClearAll()
    {
        OnPlayerDeath   = null;
        OnPlayerLevelUp = null;
        OnPlayerHPChanged = null;
        OnXPChanged     = null;
        OnEnemyKilled      = null;
        OnChestOpened      = null;
        OnWeaponFired      = null;
        OnEnemyHit         = null;
        OnLevelUpScreenShown = null;
        OnRewardApplied    = null;
        OnGameStart     = null;
        OnGameOver      = null;
        OnGamePaused    = null;
        OnGameResumed   = null;
    }
}
