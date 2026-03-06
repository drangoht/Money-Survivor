using UnityEngine;
using UnityEngine.InputSystem;

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

        // Read from Gamepad
        if (Gamepad.current != null)
        {
            _moveInput = Gamepad.current.leftStick.ReadValue();
        }
        else
        {
            _moveInput = Vector2.zero;
        }

        // Read from Keyboard and blend
        if (Keyboard.current != null)
        {
            float x = 0f, y = 0f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;

            if (x != 0f || y != 0f)
                _moveInput = new Vector2(x, y).normalized;
        }

        // Sprite faces LEFT by default (flipX=false).
        // Moving right → flipX=true  → mirrors sprite → faces right.
        // Moving left  → flipX=false → natural pose  → faces left.
        if (_sr != null && _moveInput.x != 0f)
            _sr.flipX = (_moveInput.x > 0f);
    }

    private void FixedUpdate()
    {
        if (!_stats.IsAlive) return;
        _rb.linearVelocity = _moveInput * _stats.moveSpeed;
    }
}
