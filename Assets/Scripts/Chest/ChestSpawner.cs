using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns chests periodically around the player.
/// Attach to a persistent GameObject in the Game scene.
/// </summary>
public class ChestSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject chestPrefab;
    public float spawnInterval = 60f;
    public float spawnRadius   = 6f;

    private float _timer;
    private Transform _playerTransform;

    private void Start()
    {
        _timer = spawnInterval;
        StartCoroutine(FindPlayer());
    }

    private IEnumerator FindPlayer()
    {
        while (_playerTransform == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) _playerTransform = go.transform;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameState.Playing) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = spawnInterval;
            SpawnChest();
        }
    }

    private void SpawnChest()
    {
        if (chestPrefab == null) return;
        float angle   = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist    = Random.Range(2f, spawnRadius);
        Vector2 pos   = (Vector2)_playerTransform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        Instantiate(chestPrefab, pos, Quaternion.identity);
    }
}
