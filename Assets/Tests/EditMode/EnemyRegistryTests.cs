// Wrap in UNITY_INCLUDE_TESTS so this file is only compiled when the Unity Test Framework is present.
#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class EnemyRegistryTests
{
    [TearDown]
    public void TearDown()
    {
        // Ensure registry is cleared between tests by disabling all created enemies.
        var enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            Object.DestroyImmediate(e.gameObject);
        }
    }

    [Test]
    public void GetClosest_ReturnsEnemiesOrderedByDistance()
    {
        // Arrange: three enemies at different positions
        var e1 = new GameObject("E1").AddComponent<EnemyBase>();
        e1.transform.position = new Vector3(5f, 0f, 0f);

        var e2 = new GameObject("E2").AddComponent<EnemyBase>();
        e2.transform.position = new Vector3(2f, 0f, 0f);

        var e3 = new GameObject("E3").AddComponent<EnemyBase>();
        e3.transform.position = new Vector3(10f, 0f, 0f);

        var origin = Vector2.zero;
        var results = new List<EnemyBase>();

        // Act
        EnemyRegistry.GetClosest(origin, 3, results);

        // Assert
        Assert.AreEqual(3, results.Count);
        Assert.AreSame(e2, results[0]); // distance 2
        Assert.AreSame(e1, results[1]); // distance 5
        Assert.AreSame(e3, results[2]); // distance 10
    }
}
#endif


