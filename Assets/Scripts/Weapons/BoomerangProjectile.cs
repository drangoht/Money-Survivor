using UnityEngine;

/// <summary>
/// A projectile that travels outward to a target position, waits briefly,
/// and then flies back to the player. Hits enemies along the way.
/// </summary>
public class BoomerangProjectile : ProjectileBase
{
    private Vector2 _startPos;
    private Vector2 _targetPos;
    private Transform _playerTransform;

    private float _outwardDuration;
    private float _timer;
    private bool  _isReturning;

    public void Fire(WeaponLevelStats stats, Vector2 dir, Transform player)
    {
        Initialize(stats, dir);
        _playerTransform = player;
        
        _startPos = transform.position;
        _targetPos = _startPos + dir.normalized * (stats.projectileSpeed * 0.5f);
        _outwardDuration = stats.duration * 0.45f; // Spend 45% of time going out, 10% pause, 45% returning
        _timer = 0f;
        _isReturning = false;
        
        // Disable base Rigidbody velocity, we will move it manually via Lerp
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        // Custom movement instead of base Rigidbody velocity
        _timer += Time.deltaTime;

        // Spin the sprite
        transform.Rotate(0, 0, -720f * Time.deltaTime);

        if (!_isReturning)
        {
            // Going outward
            float t = _timer / _outwardDuration;
            // Ease out
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.position = Vector2.Lerp(_startPos, _targetPos, t);

            if (_timer >= _outwardDuration)
            {
                _isReturning = true;
                _timer = 0f;
            }
        }
        else
        {
            // Pausing and returning
            if (_playerTransform == null) { Destroy(gameObject); return; }

            // Slight pause before return
            if (_timer < _outwardDuration * 0.1f) return;

            float returnTime = _timer - (_outwardDuration * 0.1f);
            float t = returnTime / _outwardDuration;

            // Ease in
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            transform.position = Vector2.Lerp(_targetPos, _playerTransform.position, t);

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
