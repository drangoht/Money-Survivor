using UnityEngine;

/// <summary>
/// Cycles sprite frames at a configurable FPS.
/// Only animates (walks) when the object is actually moving.
/// Attach to the same GameObject as the SpriteRenderer.
/// </summary>
public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 8f;

    [Tooltip("Minimum speed (units/s) before walking animation plays.")]
    public float moveThreshold = 0.05f;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private float _timer;
    private int _frame;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();

        // Look for Rigidbody2D on parent (root GameObject)
        _rb = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0 || _sr == null) return;

        // Check if moving
        bool isMoving = true;
        if (_rb != null)
            isMoving = _rb.linearVelocity.magnitude > moveThreshold;

        if (!isMoving)
        {
            // Show idle frame (frame 0) and reset timer
            _sr.sprite = frames[0];
            _frame = 0;
            _timer = 0f;
            return;
        }

        // Advance animation
        _timer += Time.deltaTime;
        if (_timer >= 1f / fps)
        {
            _timer = 0f;
            _frame = (_frame + 1) % frames.Length;
            _sr.sprite = frames[_frame];
        }
    }
}
