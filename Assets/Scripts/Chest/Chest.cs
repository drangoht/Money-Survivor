using System.Collections;
using UnityEngine;

/// <summary>
/// Chest: opened by player contact. Gives one random possible reward (new weapon, weapon upgrade, or power-up)
/// using the same rules as level-up (max 3 weapons, max level 10, etc.). Notification is shown via ChestRewardUI.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Chest : MonoBehaviour
{
    [Header("Spawn timing")]
    public static float SpawnInterval = 60f; // seconds between chest spawns

    [Header("Visuals")]
    public GameObject openParticlePrefab;

    [Header("Rewards")]
    [Tooltip("How many rewards to roll and apply when opened.")]
    public int rewardCount = 1;

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

        var levelUp = FindFirstObjectByType<LevelUpManager>();
        if (levelUp == null) { Destroy(gameObject); return; }

        var rewards = new System.Collections.Generic.List<UpgradeOption>();
        int rolls = Mathf.Max(1, rewardCount);
        for (int i = 0; i < rolls; i++)
        {
            var option = levelUp.GetRandomChestReward();
            if (option == null) break;
            rewards.Add(option);
            levelUp.ApplyReward(option);
            EventBus.RaiseRewardApplied(option);
        }

        if (rewards.Count == 0) { Destroy(gameObject); return; }

        EventBus.RaiseChestOpened(rewards);
        StartCoroutine(OpenAnimation());
    }

    private IEnumerator OpenAnimation()
    {
        if (_sr != null)
        {
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.6f * Mathf.Sin(t / 0.4f * Mathf.PI);
                transform.localScale = Vector3.one * scale;
                yield return null;
            }
        }

        if (openParticlePrefab != null)
            Instantiate(openParticlePrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
