using UnityEngine;

/// <summary>
/// Smooth camera follow that tracks the player, clamped to world bounds (optional).
/// Attach to Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    [Tooltip("Set automatically if left empty")]
    public string targetTag = "Player";

    [Header("Follow Settings")]
    public float smoothSpeed = 6f;
    public Vector3 offset    = new Vector3(0f, 0f, -10f);

    [Header("Optional World Bounds")]
    public bool useBounds;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private void LateUpdate()
    {
        if (target == null)
        {
            var go = GameObject.FindWithTag(targetTag);
            if (go != null) target = go.transform;
            return;
        }

        Vector3 desired = target.position + offset;

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
