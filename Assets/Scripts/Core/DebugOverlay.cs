using UnityEngine;

/// <summary>
/// Minimal debug overlay to inspect live game stats in development builds.
/// Toggle with F3. Does nothing in release builds.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    private bool _visible = true;

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.F3))
        {
            _visible = !_visible;
        }
#endif
    }

    private void OnGUI()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!_visible) return;

        const int width  = 240;
        const int height = 110;

        GUILayout.BeginArea(new Rect(10, 10, width, height), GUI.skin.box);
        GUILayout.Label("DEBUG");

        // Time & game state
        var gm = GameManager.Instance;
        float timeSurvived = gm != null ? gm.TimeSurvived : 0f;
        string state = gm != null ? gm.State.ToString() : "No GameManager";
        GUILayout.Label($"Time: {timeSurvived:0.0}s");
        GUILayout.Label($"State: {state}");

        // Projectile count
        GUILayout.Label($"Projectiles: {ProjectileBase.ActiveCount}");

        // Enemy count (via registry)
        int enemyCount = EnemyRegistry.Enemies != null ? EnemyRegistry.Enemies.Count : 0;
        GUILayout.Label($"Enemies: {enemyCount}");

        // Tier info (optional: try to read active spawner)
        var spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            GUILayout.Label($"Tier: {spawner.CurrentTier}");
        }

        GUILayout.EndArea();
#endif
    }
}

