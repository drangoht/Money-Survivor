using UnityEngine;

/// <summary>
/// Adds a subtle vertical bob and flips the sprite to face the movement direction.
/// Uses raw input axis instead of velocity for reliable directional detection.
/// </summary>
public class SpriteBobber : MonoBehaviour
{
    public float bobAmount = 0.05f;
    public float bobSpeed  = 6f;

    private SpriteRenderer _sr;
    private Rigidbody2D    _rb;
    private Vector3        _baseLocalPos;
    private float          _bobTimer;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
        _rb = GetComponentInParent<Rigidbody2D>();
        _baseLocalPos = transform.localPosition;
    }

    void Update()
    {
        // --- Bob only; directional flip is handled by PlayerController ---
        float speed = _rb != null ? _rb.linearVelocity.magnitude : 0f;
        float effectiveSpeed  = speed > 0.1f ? bobSpeed * 2f    : bobSpeed;
        float effectiveAmount = speed > 0.1f ? bobAmount * 1.5f : bobAmount * 0.5f;

        _bobTimer += Time.deltaTime * effectiveSpeed;
        transform.localPosition = _baseLocalPos + new Vector3(0f, Mathf.Sin(_bobTimer) * effectiveAmount, 0f);
    }
}
