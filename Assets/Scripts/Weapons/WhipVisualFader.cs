using UnityEngine;

/// <summary>
/// Fades out a SpriteRenderer over elapsed duration and then destroys the GameObject.
/// Used for the Bill Whip's visual swipe effect.
/// </summary>
public class WhipVisualFader : MonoBehaviour
{
    public float duration = 0.2f;
    public float delay = 0f;
    private SpriteRenderer _sr;
    private float _timer;
    private Color _startColor;

    void Start()
    {
        // Add spin so the bills look like they are tumbling
        var spin = gameObject.AddComponent<SpinProjectile>();
        spin.speed = Random.Range(360f, 720f);

        _sr = GetComponentInChildren<SpriteRenderer>();
        if (_sr == null)
        {
            Destroy(gameObject);
            return;
        }
        _startColor = _sr.color;
        
        if (delay > 0f)
        {
            _sr.color = new Color(_startColor.r, _startColor.g, _startColor.b, 0f); // hide initially
        }
        
        _timer = duration;
    }

    void Update()
    {
        if (delay > 0f)
        {
            delay -= Time.deltaTime;
            if (delay <= 0f)
            {
                // Delay finished; become visible and start spinning
                _sr.color = _startColor;
            }
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float t = _timer / duration;
        _sr.color = new Color(_startColor.r, _startColor.g, _startColor.b, t);
    }
}
