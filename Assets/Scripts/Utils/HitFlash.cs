using System.Collections;
using UnityEngine;

/// <summary>
/// Flashes the SpriteRenderer white briefly on hit.
/// Call Flash() from EnemyBase.TakeDamage.
/// </summary>
public class HitFlash : MonoBehaviour
{
    [Tooltip("Duration of the white flash in seconds.")]
    public float flashDuration = 0.08f;

    private SpriteRenderer _sr;
    private Color          _originalColor;
    private Coroutine      _flashCoroutine;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
        if (_sr != null) _originalColor = _sr.color;
    }

    public void Flash()
    {
        if (_sr == null) return;
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(DoFlash());
    }

    private IEnumerator DoFlash()
    {
        _sr.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        _sr.color = _originalColor;
        _flashCoroutine = null;
    }
}
