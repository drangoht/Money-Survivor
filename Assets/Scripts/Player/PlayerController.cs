using UnityEngine;

/// <summary>
/// Handles player WASD/arrow-key movement via Rigidbody2D.
/// Reads speed from PlayerStats so power-ups are reflected instantly.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Vector2     _moveInput;

    // Sprite flipping
    private SpriteRenderer _sr;

    private void Awake()
    {
        _rb    = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _sr    = GetComponentInChildren<SpriteRenderer>();

        _rb.gravityScale  = 0f;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!_stats.IsAlive) return;
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameState.Playing) return;

        _moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // Flip sprite based on horizontal direction
        if (_sr != null && _moveInput.x != 0f)
            _sr.flipX = _moveInput.x < 0f;
    }

    private void FixedUpdate()
    {
        if (!_stats.IsAlive) return;
        _rb.linearVelocity = _moveInput * _stats.moveSpeed;
    }
}
