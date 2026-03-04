using UnityEngine;

/// <summary>
/// Makes a GameObject pulse in scale sinusoidally.
/// Great for XP orbs, pickups and UI elements.
/// </summary>
public class ScalePulse : MonoBehaviour
{
    public float speed     = 3f;
    public float amplitude = 0.12f;   // max scale delta
    public float baseScale = 1f;

    private float _offset;

    void Awake() => _offset = Random.Range(0f, Mathf.PI * 2f); // random phase so all orbs don't sync

    void Update()
    {
        float s = baseScale + Mathf.Sin(Time.time * speed + _offset) * amplitude;
        transform.localScale = new Vector3(s, s, 1f);
    }
}
