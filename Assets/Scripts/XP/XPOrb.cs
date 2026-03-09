using UnityEngine;

/// <summary>
/// XP Orb: floats on the ground after enemy death.
/// Once within player's pickup radius, magnetically flies toward the player.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class XPOrb : MonoBehaviour, IPoolable
{
    [Header("Settings")]
    public int   xpValue      = 10;
    public float magnetSpeed  = 8f;
    public Color orbColor     = new Color(1f, 0.9f, 0.1f);

    private Transform    _playerTransform;
    private PlayerStats  _playerStats;
    private XPManager    _xpManager;
    private bool         _isBeingAttracted;
    private SpriteRenderer _sr;
    public string poolTag;

    // ── IPoolable ────────────────────────────────────────────────────────────
    public void OnSpawn()
    {
        _isBeingAttracted = false;
        SetXP(xpValue); // Refresh visual state just in case
    }

    public void OnDespawn() { }

    // ── Unity lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.3f;
    }

    private void Start()
    {
        FindReferences();
        SetXP(xpValue); // Apply initial visual state
    }

    public void SetXP(int amount)
    {
        xpValue = amount;
        
        // Define dynamic sizes and colors based on XP payload
        float scaleMultiplier;
        
        if (xpValue <= 5)
        {
            // Small (e.g., standard enemies)
            scaleMultiplier = 0.4f;
            orbColor = new Color(0.2f, 0.9f, 0.2f); // Neon Green
        }
        else if (xpValue <= 15)
        {
            // Medium
            scaleMultiplier = 0.6f;
            orbColor = new Color(0.2f, 0.8f, 1f);   // Electric Cyan
        }
        else
        {
            // Large (e.g., Bosses like IRS)
            scaleMultiplier = 0.8f;
            orbColor = new Color(0.9f, 0.3f, 0.9f); // Neon Purple
        }

        transform.localScale = Vector3.one * scaleMultiplier;
        if (_sr != null) _sr.color = orbColor;
    }

    private void FindReferences()
    {
        if (_playerStats == null || _playerTransform == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerStats     = player.GetComponent<PlayerStats>();
            }
        }

        if (_xpManager == null)
        {
            _xpManager = Object.FindFirstObjectByType<XPManager>();
        }
    }

    private void Update()
    {
        if (_playerTransform == null) { FindReferences(); return; }

        float dist = Vector2.Distance(transform.position, _playerTransform.position);

        // Enter magnet range
        float radius = _playerStats != null ? _playerStats.pickupRadius : 2.5f;
        if (dist <= radius) _isBeingAttracted = true;

        if (_isBeingAttracted)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                _playerTransform.position,
                magnetSpeed * Time.deltaTime
            );

            if (dist < 0.3f) Collect();
        }
    }

    private void Collect()
    {
        _xpManager?.AddXP(xpValue);

        if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolTag))
            ObjectPool.Instance.Return(poolTag, gameObject);
        else
            Destroy(gameObject);
    }

    /// <summary>Call this to immediately attract all nearby orbs (Black Market power-up).</summary>
    public void ForceAttract() => _isBeingAttracted = true;
}
