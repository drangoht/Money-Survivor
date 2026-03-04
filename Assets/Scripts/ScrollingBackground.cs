using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Tooltip("How fast the texture scrolls relative to player speed.")]
    public float scrollScale = 0.02f;

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
        if (_bgRenderer == null || _player == null) return;

        // Calculate how much the player moved this frame
        Vector3 delta = _player.position - _lastPlayerPos;
        _lastPlayerPos = _player.position;

        // Only scroll when the player actually moved
        if (delta.sqrMagnitude > 0.00001f)
        {
            // Scroll in the opposite direction of player movement (parallax feel)
            _currentOffset += new Vector2(delta.x, delta.y) * scrollScale;
            _bgRenderer.material.mainTextureOffset = _currentOffset;
        }
    }
}
