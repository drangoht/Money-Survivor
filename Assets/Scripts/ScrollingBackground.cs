using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Tooltip("How fast the texture scrolls relative to player speed.")]
    public float scrollScale = 0.02f;

    [Tooltip("When true, move this transform (and synced orbs/miners) instead of texture offset, so obstacles and ground stay aligned.")]
    public bool moveTransformWithScroll;

    private Renderer  _bgRenderer;
    private Transform _player;
    private Vector2   _currentOffset;
    private Vector3   _lastPlayerPos;

    void Start()
    {
        _bgRenderer = GetComponent<Renderer>();
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            _player = playerGO.transform;
            _lastPlayerPos = _player.position;
        }
    }

    void Update()
    {
        if (_player == null) return;

        Vector3 delta = _player.position - _lastPlayerPos;
        _lastPlayerPos = _player.position;

        if (delta.sqrMagnitude < 0.00001f) return;

        Vector2 scroll = new Vector2(delta.x, delta.y) * scrollScale;

        if (moveTransformWithScroll)
        {
            // Move this transform (foreground quad) so texture and child obstacles scroll together
            transform.position += new Vector3(-scroll.x, -scroll.y, 0f);

            // Sync XP orbs and crypto miners so they don't slide on the ground
            Vector3 worldDelta = new Vector3(-scroll.x, -scroll.y, 0f);
            var orbs = FindObjectsByType<XPOrb>(FindObjectsSortMode.None);
            for (int i = 0; i < orbs.Length; i++)
                if (orbs[i] != null && orbs[i].gameObject.activeInHierarchy)
                    orbs[i].transform.position += worldDelta;
            var miners = FindObjectsByType<LingeringAOE>(FindObjectsSortMode.None);
            for (int i = 0; i < miners.Length; i++)
                if (miners[i] != null && miners[i].gameObject.activeInHierarchy)
                    miners[i].transform.position += worldDelta;
        }
        else if (_bgRenderer != null)
        {
            _currentOffset += scroll;
            _bgRenderer.material.mainTextureOffset = _currentOffset;
        }
    }
}
