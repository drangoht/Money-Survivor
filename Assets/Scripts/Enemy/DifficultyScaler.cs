using UnityEngine;

/// <summary>
/// Central place for difficulty / strength scaling over time.
/// Converts time survived into strength multipliers so that the curve
/// is easy to tweak and test.
/// </summary>
public static class DifficultyScaler
{
    /// <summary>
    /// Returns a multiplier for enemy HP/contact damage based on time survived.
    /// Steps up every 3 minutes (+25% per 3 minutes).
    /// 0–3 min: 1.0, 3–6: 1.25, 6–9: 1.5, etc.
    /// </summary>
    public static float GetTimeStrengthMultiplier(float timeSurvivedSeconds)
    {
        if (timeSurvivedSeconds <= 0f) return 1f;

        const float stepSeconds = 180f;   // 3 minutes
        const float stepIncrease = 0.25f; // +25% per step

        int steps = Mathf.FloorToInt(timeSurvivedSeconds / stepSeconds);
        return 1f + steps * stepIncrease;
    }
}

