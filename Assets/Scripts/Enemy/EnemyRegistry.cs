using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry of live enemies in the scene.
/// Enemies register/unregister themselves so other systems (weapons, AI)
/// can query them without expensive scene-wide searches.
/// </summary>
public static class EnemyRegistry
{
    private static readonly List<EnemyBase> _enemies = new List<EnemyBase>();

    /// <summary>Snapshot-style read-only view of all currently-registered enemies.</summary>
    public static IReadOnlyList<EnemyBase> Enemies => _enemies;

    public static void Register(EnemyBase enemy)
    {
        if (enemy == null) return;
        if (_enemies.Contains(enemy)) return;
        _enemies.Add(enemy);
    }

    public static void Unregister(EnemyBase enemy)
    {
        if (enemy == null) return;
        _enemies.Remove(enemy);
    }

    /// <summary>
    /// Fills the provided list with up to maxCount closest enemies to origin.
    /// The result list is cleared first.
    /// </summary>
    public static void GetClosest(Vector2 origin, int maxCount, List<EnemyBase> result)
    {
        result.Clear();
        if (_enemies.Count == 0 || maxCount <= 0) return;

        // Simple O(n log n) sort; list is usually small.
        var temp = ListPool<EnemyBase>.Get();
        temp.AddRange(_enemies);
        temp.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            float da = ((Vector2)a.transform.position - origin).sqrMagnitude;
            float db = ((Vector2)b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        });

        for (int i = 0; i < temp.Count && result.Count < maxCount; i++)
        {
            if (temp[i] != null) result.Add(temp[i]);
        }

        ListPool<EnemyBase>.Release(temp);
    }

    /// <summary>
    /// Utility list pool so we avoid allocating temporary lists every frame.
    /// </summary>
    private static class ListPool<T>
    {
        private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

        public static List<T> Get()
        {
            return Pool.Count > 0 ? Pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            Pool.Push(list);
        }
    }
}

