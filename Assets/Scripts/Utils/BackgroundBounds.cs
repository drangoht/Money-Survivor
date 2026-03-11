using UnityEngine;

/// <summary>
/// At runtime, reads the world-space bounds of the background quad
/// and applies matching movement/camera limits so the player and
/// camera can never leave the visible floor area (both in Editor
/// Play mode and in builds).
/// </summary>
public class BackgroundBounds : MonoBehaviour
{
    [Tooltip("Optional padding inside the background edges.")]
    public float padding = 1f;

    [Tooltip("Extra inset for the PLAYER only, so their body never visually reaches the edge of the floor.")]
    public float playerPadding = 3f;

    private void Start()
    {
        var rend = GetComponent<Renderer>();
        if (rend == null) return;

        Bounds b = rend.bounds;
        Vector2 bgMin = new Vector2(b.min.x + padding, b.min.y + padding);
        Vector2 bgMax = new Vector2(b.max.x - padding, b.max.y - padding);

        // Configure player movement bounds slightly INSIDE the background area,
        // so the player sprite never visually leaves the floor.
        Vector2 playerMin = new Vector2(bgMin.x + playerPadding, bgMin.y + playerPadding);
        Vector2 playerMax = new Vector2(bgMax.x - playerPadding, bgMax.y - playerPadding);
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.useMovementBounds = true;
                pc.minBounds = playerMin;
                pc.maxBounds = playerMax;
            }
        }

        // Configure camera follow bounds so the CAMERA VIEW never leaves the background.
        // We shrink the usable area by the camera's half-extents in world units.
        var cam = Camera.main;
        if (cam != null)
        {
            var cf = cam.GetComponent<CameraFollow>();
            if (cf != null)
            {
                float vertExtent  = cam.orthographicSize;
                float horizExtent = vertExtent * cam.aspect;

                Vector2 camMin = new Vector2(bgMin.x + horizExtent, bgMin.y + vertExtent);
                Vector2 camMax = new Vector2(bgMax.x - horizExtent, bgMax.y - vertExtent);

                cf.useBounds = true;
                cf.minBounds = camMin;
                cf.maxBounds = camMax;
            }
        }
    }
}

