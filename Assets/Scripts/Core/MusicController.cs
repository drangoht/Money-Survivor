using UnityEngine;

/// <summary>
/// Procedural looping background music. Attach to same GameObject as GameManager (persists across scenes).
/// Starts on game start, stops on game over or return to menu.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    public static MusicController Instance { get; private set; }

    private const int SampleRate = 44100;
    private AudioSource _source;
    private AudioClip _loopClip;
    private bool _playing;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = true;
        _source.spatialBlend = 0f;
        _source.volume = 0.25f;
        _source.priority = 0; // Lower than SFX

        _loopClip = CreateLoopClip();
        _source.clip = _loopClip;
    }

    private void OnEnable()
    {
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnGameOver -= OnGameOver;
    }

    private void OnGameStart()
    {
        _playing = true;
        if (_source != null && _loopClip != null) _source.Play();
    }

    private void OnGameOver()
    {
        StopMusic();
    }

    /// <summary>Call when returning to main menu so BGM stops.</summary>
    public void StopMusic()
    {
        _playing = false;
        if (_source != null && _source.isPlaying) _source.Stop();
    }

    /// <summary>Simple procedural BGM: slow pad + light arpeggio (survivor-style tension).</summary>
    private static AudioClip CreateLoopClip()
    {
        float loopSeconds = 16f;
        int len = (int)(SampleRate * loopSeconds);
        var data = new float[len];

        // Root note ~55 Hz (A1), minor feel
        float baseFreq = 55f;
        float[] chord = { 1f, 1.2f, 1.5f }; // minor-ish

        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            float sample = 0f;

            // Soft pad (low amplitude)
            for (int c = 0; c < chord.Length; c++)
                sample += Mathf.Sin(2f * Mathf.PI * baseFreq * chord[c] * t) * 0.08f;

            // Light arpeggio every 2 seconds
            float arpPhase = (t * 0.5f) % 1f;
            float arpFreq = baseFreq * chord[Mathf.FloorToInt(arpPhase * 3) % 3] * 2f;
            sample += Mathf.Sin(2f * Mathf.PI * arpFreq * t) * (0.04f * (1f - arpPhase));

            data[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        var clip = AudioClip.Create("BGM_Loop", len, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
