using UnityEngine;

/// <summary>
/// Plays procedural sound effects for weapons, hits, enemies, level-up, chest, and rewards.
/// Subscribes to EventBus and uses generated one-shots (no audio files required).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    private const int SampleRate = 44100;
    private AudioSource _source;
    private AudioClip _clipShoot;
    private AudioClip _clipHit;
    private AudioClip _clipEnemyDie;
    private AudioClip _clipPowerUp;
    private AudioClip _clipWeaponGet;
    private AudioClip _clipWeaponUpgrade;
    private AudioClip _clipLevelUpScreen;
    private AudioClip _clipChestOpen;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f; // 2D
        _source.volume = 0.4f;

        _clipShoot         = CreateShootClip();
        _clipHit           = CreateHitClip();
        _clipEnemyDie      = CreateEnemyDieClip();
        _clipPowerUp       = CreatePowerUpClip();
        _clipWeaponGet     = CreateWeaponGetClip();
        _clipWeaponUpgrade = CreateWeaponUpgradeClip();
        _clipLevelUpScreen = CreateLevelUpScreenClip();
        _clipChestOpen     = CreateChestOpenClip();
    }

    private void OnEnable()
    {
        EventBus.OnWeaponFired        += OnWeaponFired;
        EventBus.OnEnemyHit           += OnEnemyHit;
        EventBus.OnEnemyKilled        += OnEnemyKilled;
        EventBus.OnRewardApplied      += OnRewardApplied;
        EventBus.OnLevelUpScreenShown += OnLevelUpScreenShown;
        EventBus.OnChestOpened        += OnChestOpened;
    }

    private void OnDisable()
    {
        EventBus.OnWeaponFired        -= OnWeaponFired;
        EventBus.OnEnemyHit           -= OnEnemyHit;
        EventBus.OnEnemyKilled        -= OnEnemyKilled;
        EventBus.OnRewardApplied      -= OnRewardApplied;
        EventBus.OnLevelUpScreenShown -= OnLevelUpScreenShown;
        EventBus.OnChestOpened        -= OnChestOpened;
    }

    private void OnWeaponFired(Vector3 _)         => Play(_clipShoot);
    private void OnEnemyHit(Vector3 _)            => Play(_clipHit);
    private void OnEnemyKilled(Vector3 _, int __) => Play(_clipEnemyDie);
    private void OnLevelUpScreenShown()           => Play(_clipLevelUpScreen);
    private void OnChestOpened(UpgradeOption _)   => Play(_clipChestOpen);

    private void OnRewardApplied(UpgradeOption option)
    {
        if (option == null) return;
        switch (option.type)
        {
            case UpgradeType.PowerUp:       Play(_clipPowerUp);       break;
            case UpgradeType.NewWeapon:     Play(_clipWeaponGet);     break;
            case UpgradeType.WeaponUpgrade: Play(_clipWeaponUpgrade); break;
        }
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && _source != null)
            _source.PlayOneShot(clip);
    }

    // ── Procedural clip generation ───────────────────────────────────────────

    private static AudioClip CreateShootClip()
    {
        int len = SampleRate / 20; // ~50 ms
        var data = new float[len];
        float freq = 400f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t * 20f);
        }
        return CreateClip("Shoot", data);
    }

    private static AudioClip CreateHitClip()
    {
        int len = SampleRate / 30;
        var data = new float[len];
        for (int i = 0; i < len; i++)
            data[i] = (Random.value * 2f - 1f) * (1f - i / (float)len);
        return CreateClip("Hit", data);
    }

    private static AudioClip CreateEnemyDieClip()
    {
        int len = SampleRate / 8;
        var data = new float[len];
        float freq = 180f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            float env = 1f - t * 8f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env;
        }
        return CreateClip("EnemyDie", data);
    }

    private static AudioClip CreatePowerUpClip()
    {
        int len = (int)(SampleRate * 0.2f);
        var data = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            float f = 523f + t * 200f;
            data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * (1f - t * 5f) * 0.5f;
        }
        return CreateClip("PowerUp", data);
    }

    private static AudioClip CreateWeaponGetClip()
    {
        int len = (int)(SampleRate * 0.25f);
        var data = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            float f = 330f + t * 300f;
            data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * (1f - t * 4f) * 0.5f;
        }
        return CreateClip("WeaponGet", data);
    }

    private static AudioClip CreateWeaponUpgradeClip()
    {
        int len = (int)(SampleRate * 0.15f);
        var data = new float[len];
        float f1 = 440f, f2 = 554f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            float env = 1f - t * 6f;
            data[i] = (Mathf.Sin(2f * Mathf.PI * f1 * t) + Mathf.Sin(2f * Mathf.PI * f2 * t)) * 0.25f * env;
        }
        return CreateClip("WeaponUpgrade", data);
    }

    private static AudioClip CreateLevelUpScreenClip()
    {
        int len = (int)(SampleRate * 0.35f);
        var data = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            float f = 392f + Mathf.Floor(t * 4f) * 131f; // short arpeggio
            float env = t < 0.05f ? t * 20f : (1f - (t - 0.05f) * 3f);
            data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Clamp01(env) * 0.4f;
        }
        return CreateClip("LevelUpScreen", data);
    }

    private static AudioClip CreateChestOpenClip()
    {
        int len = (int)(SampleRate * 0.3f);
        var data = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SampleRate;
            float freq = 600f - t * 200f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t * 3f) * 0.5f;
        }
        return CreateClip("ChestOpen", data);
    }

    private static AudioClip CreateClip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
