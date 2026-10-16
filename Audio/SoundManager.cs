using Raylib_cs;

namespace HelldiversDesktop.Audio;

public unsafe class SoundManager
{
    private bool _initialized = false;
    private float _volume = 0.70f;
    private readonly Dictionary<string, Sound> _cachedSounds = new();

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_initialized)
            {
                Raylib.SetMasterVolume(_volume);
            }
        }
    }

    public void Init(float initialVolume = 0.70f)
    {
        try
        {
            Raylib.InitAudioDevice();
            _initialized = Raylib.IsAudioDeviceReady();
            if (_initialized)
            {
                PreloadProceduralSounds();
                Volume = initialVolume;
            }
        }
        catch { }
    }

    private void PreloadProceduralSounds()
    {
        _cachedSounds["shoot_ar"] = GenerateTone(450, 0.08f, WaveType.Sawtooth);
        _cachedSounds["shoot_shotgun"] = GenerateNoise(0.18f, true);
        _cachedSounds["shoot_laser"] = GenerateTone(850, 0.07f, WaveType.Sine);
        _cachedSounds["shoot_plasma"] = GenerateTone(320, 0.12f, WaveType.Square);
        _cachedSounds["deflect"] = GenerateTone(920, 0.06f, WaveType.Triangle);
        _cachedSounds["crit"] = GenerateTone(130, 0.18f, WaveType.Square);
        _cachedSounds["acid"] = GenerateNoise(0.25f, false);
        _cachedSounds["explosion"] = GenerateNoise(0.6f, true);
        _cachedSounds["dive"] = GenerateTone(180, 0.22f, WaveType.Sine);
        _cachedSounds["pelican"] = GenerateTone(380, 0.75f, WaveType.Square);
        _cachedSounds["victory"] = GenerateTone(523, 0.4f, WaveType.Sine);
    }

    public void PlayShoot(string type)
    {
        if (!_initialized) return;
        string key = type switch
        {
            "laser" => "shoot_laser",
            "plasma" => "shoot_plasma",
            "shotgun" or "punisher" => "shoot_shotgun",
            _ => "shoot_ar"
        };
        Play(key);
    }

    public void PlayDeflect() => Play("deflect");
    public void PlayCrit() => Play("crit");
    public void PlayAcid() => Play("acid");
    public void PlayExplosion() => Play("explosion");
    public void PlayDive() => Play("dive");
    public void PlayPelican() => Play("pelican");
    public void PlayVictory() => Play("victory");

    private void Play(string key)
    {
        if (!_initialized) return;
        if (_cachedSounds.TryGetValue(key, out var snd))
        {
            Raylib.PlaySound(snd);
        }
    }

    private enum WaveType { Sine, Square, Sawtooth, Triangle }

    private Sound GenerateTone(float freq, float duration, WaveType type)
    {
        const int sampleRate = 44100;
        int frameCount = (int)(sampleRate * duration);
        short[] samples = new short[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / frameCount;
            float envelope = 1f - progress; // Linear decay

            float sampleVal = type switch
            {
                WaveType.Sine => MathF.Sin(2f * MathF.PI * freq * t),
                WaveType.Square => MathF.Sin(2f * MathF.PI * freq * t) >= 0 ? 0.7f : -0.7f,
                WaveType.Sawtooth => (2f * (t * freq - MathF.Floor(t * freq + 0.5f))),
                WaveType.Triangle => (2f * MathF.Abs(2f * (t * freq - MathF.Floor(t * freq + 0.5f))) - 1f),
                _ => 0f
            };

            samples[i] = (short)(sampleVal * envelope * 24000);
        }

        fixed (short* ptr = samples)
        {
            Wave wave = new()
            {
                FrameCount = (uint)frameCount,
                SampleRate = sampleRate,
                SampleSize = 16,
                Channels = 1,
                Data = ptr
            };
            return Raylib.LoadSoundFromWave(wave);
        }
    }

    private Sound GenerateNoise(float duration, bool heavy)
    {
        const int sampleRate = 44100;
        int frameCount = (int)(sampleRate * duration);
        short[] samples = new short[frameCount];
        var rnd = new Random();
        float currentVal = 0;

        for (int i = 0; i < frameCount; i++)
        {
            float progress = (float)i / frameCount;
            float envelope = MathF.Pow(1f - progress, heavy ? 2f : 1.2f);
            float white = (float)rnd.NextDouble() * 2f - 1f;

            // Simple low-pass filter
            float filter = heavy ? 0.08f : 0.2f;
            currentVal += filter * (white - currentVal);

            samples[i] = (short)(currentVal * envelope * 28000);
        }

        fixed (short* ptr = samples)
        {
            Wave wave = new()
            {
                FrameCount = (uint)frameCount,
                SampleRate = sampleRate,
                SampleSize = 16,
                Channels = 1,
                Data = ptr
            };
            return Raylib.LoadSoundFromWave(wave);
        }
    }

    public void Close()
    {
        if (!_initialized) return;
        foreach (var snd in _cachedSounds.Values)
        {
            Raylib.UnloadSound(snd);
        }
        Raylib.CloseAudioDevice();
    }
}
