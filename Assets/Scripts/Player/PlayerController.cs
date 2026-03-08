using UnityEngine;

/// <summary>
/// Handles player WASD/arrow-key and gamepad movement via Rigidbody2D.
/// Reads speed from PlayerStats so power-ups are reflected instantly.
/// Uses legacy Input Manager (no new Input System package required).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Vector2     _moveInput;

    [Header("Movement Bounds")]
    public bool   useMovementBounds = false;
    public Vector2 minBounds = new Vector2(-95f, -95f);
    public Vector2 maxBounds = new Vector2( 95f,  95f);

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

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(x, y);
        if (_moveInput.sqrMagnitude > 1f)
            _moveInput.Normalize();

        // Sprite faces LEFT by default (flipX=false).
        if (_sr != null && _moveInput.x != 0f)
            _sr.flipX = (_moveInput.x > 0f);
    }

    private void FixedUpdate()
    {
        if (!_stats.IsAlive) return;

        Vector2 desiredVelocity = _moveInput * _stats.moveSpeed;

        if (!useMovementBounds)
        {
            _rb.linearVelocity = desiredVelocity;
            return;
        }

        Vector2 currentPos = _rb.position;
        Vector2 nextPos    = currentPos + desiredVelocity * Time.fixedDeltaTime;

        nextPos.x = Mathf.Clamp(nextPos.x, minBounds.x, maxBounds.x);
        nextPos.y = Mathf.Clamp(nextPos.y, minBounds.y, maxBounds.y);

        _rb.MovePosition(nextPos);
        Vector2 actualVelocity = (nextPos - currentPos) / Time.fixedDeltaTime;
        _rb.linearVelocity = actualVelocity;
    }
}
