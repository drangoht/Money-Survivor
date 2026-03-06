using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemies around the player in escalating waves.
/// Attach to a persistent GameObject in the Game scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject bankmanPrefab;
    public GameObject exWifePrefab;
    public GameObject childrenPrefab;
    public GameObject irsPrefab;       // boss 1
    public GameObject bouncerPrefab;   // tank
    public GameObject ceoPrefab;       // endgame boss

    [Header("Spawn Settings")]
    [Tooltip("How far off-screen enemies spawn (world units from player)")]
    public float spawnRadius = 14f;

    [Tooltip("Base seconds between spawns")]
    public float baseSpawnInterval = 1.2f;

    [Tooltip("Every this many seconds, increase difficulty tier")]
    public float tierInterval = 30f;

    [Tooltip("Auditor boss spawns every N seconds")]
    public float bossInterval = 180f; // 3 mins

    [Tooltip("CEO boss spawns at this absolute time")]
    public float ceoSpawnTime = 600f; // 10 mins

    private float _spawnTimer;
    private float _tierTimer;
    private float _bossTimer;
    private bool  _ceoSpawned;
    private int   _tier;

    private Transform _playerTransform;

    // Wave composition: probability weights per tier
    //  [tier][0]=Bankman, [1]=LoanShark, [2]=TaxCollector, [3]=Bouncer
    private static readonly float[,] SpawnWeights = {
        { 1.00f, 0.00f, 0.00f, 0.00f },  // tier 0 (0-30s)
        { 0.70f, 0.30f, 0.00f, 0.00f },  // tier 1 (30-60s)
        { 0.40f, 0.45f, 0.15f, 0.00f },  // tier 2 (60-90s)
        { 0.20f, 0.40f, 0.30f, 0.10f },  // tier 3 (90-120s)
        { 0.10f, 0.20f, 0.40f, 0.30f },  // tier 4+ (120s+)
    };

    private void Start()
    {
        _spawnTimer = baseSpawnInterval;
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

        // Difficulty tier escalation
        _tierTimer += Time.deltaTime;
        if (_tierTimer >= tierInterval)
        {
            _tierTimer = 0f;
            _tier++;
            // Also shrink spawn interval (faster spawns)
            baseSpawnInterval = Mathf.Max(0.3f, baseSpawnInterval * 0.9f);
        }

        // Boss 1 timer (IRS)
        _bossTimer += Time.deltaTime;
        if (_bossTimer >= bossInterval)
        {
            _bossTimer = 0f;
            SpawnEnemy(irsPrefab);
        }

        // Boss 2 timer (CEO)
        if (!_ceoSpawned && GameManager.Instance != null && GameManager.Instance.TimeSurvived >= ceoSpawnTime)
        {
            _ceoSpawned = true;
            SpawnEnemy(ceoPrefab);
        }

        // Regular spawns
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = baseSpawnInterval;
            SpawnRegularEnemy();
        }
    }

    private void SpawnRegularEnemy()
    {
        int clampedTier = Mathf.Min(_tier, 4);
        float r = Random.value;
        float w0 = SpawnWeights[clampedTier, 0];
        float w1 = SpawnWeights[clampedTier, 1];
        float w2 = SpawnWeights[clampedTier, 2];

        if (r < w0)                SpawnEnemy(bankmanPrefab);
        else if (r < w0 + w1)      SpawnEnemy(exWifePrefab);
        else if (r < w0 + w1 + w2) SpawnEnemy(childrenPrefab);
        else                       SpawnEnemy(bouncerPrefab);
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        Vector2 spawnPos = GetSpawnPosition();
        Instantiate(prefab, spawnPos, Quaternion.identity);

        // Set difficulty tier on spawned enemy
        var enemy = prefab.GetComponent<EnemyBase>();
        if (enemy != null) enemy.SetDifficultyTier(_tier);
    }

    private Vector2 GetSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return (Vector2)_playerTransform.position + dir * spawnRadius;
    }
}
