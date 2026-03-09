// Wrapped so this only compiles when the Unity Test Framework is present.
#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public class DifficultyScalerTests
{
    [Test]
    public void TimeStrengthMultiplier_Is1_AtStart()
    {
        Assert.AreEqual(1f, DifficultyScaler.GetTimeStrengthMultiplier(0f));
        Assert.AreEqual(1f, DifficultyScaler.GetTimeStrengthMultiplier(10f));
    }

    [Test]
    public void TimeStrengthMultiplier_IncreasesEvery3Minutes_By25Percent()
    {
        // Just under 3 minutes → still 1.0
        Assert.AreEqual(1f, DifficultyScaler.GetTimeStrengthMultiplier(179f));

        // At 3 minutes → 1.25
        Assert.AreEqual(1.25f, DifficultyScaler.GetTimeStrengthMultiplier(180f));

        // At 6 minutes → 1.5
        Assert.AreEqual(1.5f, DifficultyScaler.GetTimeStrengthMultiplier(360f));

        // At 9 minutes → 1.75
        Assert.AreEqual(1.75f, DifficultyScaler.GetTimeStrengthMultiplier(540f));
    }
}
#endif

