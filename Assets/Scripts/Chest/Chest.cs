using System.Collections;
using UnityEngine;

/// <summary>
/// Chest: spawns periodically in the world. Player walks over it to open it
/// and receive a random power-up reward.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Chest : MonoBehaviour
{
    [Header("Rewards")]
    public PowerUpData[] possibleRewards;

    [Header("Spawn timing")]
    public static float SpawnInterval = 60f; // seconds between chest spawns

    [Header("Visuals")]
    public GameObject openParticlePrefab;

    private bool _opened;
    private SpriteRenderer _sr;



    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.8f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_opened || !other.CompareTag("Player")) return;
        Open();
    }

    private void Open()
    {
        if (_opened) return;
        _opened = true;

        // Pick a random reward
        if (possibleRewards == null || possibleRewards.Length == 0) { Destroy(gameObject); return; }
        var reward = possibleRewards[Random.Range(0, possibleRewards.Length)];

        EventBus.RaiseChestOpened(reward);
        StartCoroutine(OpenAnimation(reward));
    }

    private IEnumerator OpenAnimation(PowerUpData reward)
    {
        // Quick bounce animation
        if (_sr != null)
        {
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.6f * Mathf.Sin(t / 0.4f * Mathf.PI); // Bigger bounce
                transform.localScale = Vector3.one * scale;
                yield return null;
            }
        }

        // Spawn explosion particles before destroying
        if (openParticlePrefab != null)
        {
            Instantiate(openParticlePrefab, transform.position, Quaternion.identity);
        }

        // Show the reward UI
        var chestUI = FindFirstObjectByType<ChestRewardUI>();
        chestUI?.Show(reward);

        Destroy(gameObject);
    }
}
