using System.Collections;
using UnityEngine;

/// <summary>
/// Attaches to the Main Camera. Call ScreenShake.Instance.Shake() from anywhere.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    private Vector3 _originalPos;
    private bool    _isShaking;

    private void Awake()
    {
        Instance    = this;
        _originalPos = transform.localPosition;
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        if (_isShaking) StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        _isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = _originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
        _isShaking = false;
    }
}
