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

        var option = levelUp.GetRandomChestReward();
        if (option == null) { Destroy(gameObject); return; }
        levelUp.ApplyReward(option);
        EventBus.RaiseChestOpened(option);
        StartCoroutine(OpenAnimation(option));
    }

    private IEnumerator OpenAnimation(UpgradeOption option)
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

        var chestUI = FindFirstObjectByType<ChestRewardUI>(FindObjectsInactive.Include);
        if (chestUI != null)
        {
            chestUI.gameObject.SetActive(true);
            chestUI.Show(option);
        }

        Destroy(gameObject);
    }
}
