using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool. Attach this to a persistent GameObject and call
/// ObjectPool.Instance.Get() / Return() from anywhere.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
    }

    [SerializeField] private List<Pool> pools = new();

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabLookup   = new Dictionary<string, GameObject>();

        foreach (var pool in pools)
        {
            var queue = new Queue<GameObject>();
            var parent = new GameObject($"Pool_{pool.tag}").transform;
            parent.SetParent(transform);

            for (int i = 0; i < pool.initialSize; i++)
            {
                var obj = Instantiate(pool.prefab, parent);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            poolDictionary[pool.tag] = queue;
            prefabLookup[pool.tag]   = pool.prefab;
        }
    }

    public GameObject Get(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] Tag not found: {tag}");
            return null;
        }

        var queue = poolDictionary[tag];
        GameObject obj;

        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            // Grow pool dynamically if exhausted
            obj = Instantiate(prefabLookup[tag]);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawn();

        return obj;
    }

    public void Return(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Destroy(obj);
            return;
        }

        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnDespawn();

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}

/// <summary>Optional interface for pool-aware objects.</summary>
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}
