using System;
using System.Collections.Generic;
using Raylib_cs;

namespace HelldiversDesktop.Audio
{
    /// <summary>
    /// Motor DSP PCM 44100Hz, 16-bit Mono.
    /// Arquitectura de sonido suave, orgánica y cinemática sin distorsión dura (sin clipping ni estridencia):
    /// - Filtro pasa-bajos de dos polos (Butterworth IIR) para eliminar siseos y pitos molestos.
    /// - Saturación Soft-Knee analógica (Tanh suave) con headroom limpio de 20,000 unidades.
    /// - Rampa de ataque de 3ms y decaimiento de 6ms para eliminar cualquier click o pop de corriente continua.
    /// - Sub-graves tectónicos profundos para artillería, bombas de 500kg y escopetas pesadas.
    /// </summary>
    public unsafe class SoundManager
    {
        private const int SampleRate = 44100;
        private bool _initialized = false;
        private float _volume = 0.75f;
        private readonly Dictionary<string, Sound> _cachedSounds = new();
        private readonly Random _rnd = new(1337);

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

        public void Init(float initialVolume = 0.75f)
        {
            try
            {
                Raylib.InitAudioDevice();
                _initialized = Raylib.IsAudioDeviceReady();
                if (_initialized)
                {
                    PreloadMultilayerProceduralSounds();
                    Volume = initialVolume;
                }
            }
            catch { }
        }

        private void PreloadMultilayerProceduralSounds()
        {
            // 1. Arsenal Balístico y de Energía Suave y Realista
            _cachedSounds["shoot_liberator"]    = GenerateLiberatorGunshot();
            _cachedSounds["shoot_breaker"]      = GenerateBreakerGunshot();
            _cachedSounds["shoot_punisher"]     = GeneratePunisherGunshot();
            _cachedSounds["shoot_dominator"]    = GenerateDominatorGunshot();
            _cachedSounds["shoot_scythe"]       = GenerateScytheBeam();
            _cachedSounds["shoot_scorcher"]     = GenerateScorcherPlasma();
            _cachedSounds["shoot_autocannon"]   = GenerateAutocannonGunshot();
            _cachedSounds["shoot_railgun"]      = GenerateRailgunDischarge();
            _cachedSounds["shoot_flamethrower"] = GenerateFlamethrowerRoar();
            _cachedSounds["shoot_quasar"]       = GenerateQuasarPulse();

            // Compatibilidad hacia atrás
            _cachedSounds["shoot_laser"]   = _cachedSounds["shoot_scythe"];
            _cachedSounds["shoot_plasma"]  = _cachedSounds["shoot_scorcher"];
            _cachedSounds["shoot_shotgun"] = _cachedSounds["shoot_breaker"];

            // 2. Explosiones Cinemáticas Cálidas con Sub-Graves Tectónicos
            _cachedSounds["explosion_500kg"] = Generate500kgNuclearRoar();
            _cachedSounds["explosion"]       = GenerateExplosion(duration: 0.85f, isHeavy: true);
            _cachedSounds["explosion_small"] = GenerateExplosion(duration: 0.42f, isHeavy: false);

            // 3. Manejo de Armas, Estratagemas y Eventos Tácticos
            _cachedSounds["reload"]         = GenerateReloadSound();
            _cachedSounds["empty_click"]    = GenerateEmptyClick();
            _cachedSounds["turret_deploy"]  = GenerateTurretDeploySound();
            _cachedSounds["deflect"]        = GenerateDeflectRicochet();
            _cachedSounds["crit"]           = GenerateFleshyCritPunch();
            _cachedSounds["acid"]           = GenerateAcidSizzle();
            _cachedSounds["dive"]           = GenerateDiveWhoosh();
            _cachedSounds["pelican"]        = GeneratePelicanTurbine();
            _cachedSounds["victory"]        = GenerateDemocracyFanfare();
            _cachedSounds["buzzsaw"]        = GenerateBerserkerSaw();
        }

        public void PlayShoot(string type)
        {
            if (!_initialized) return;
            string key = type.ToLowerInvariant() switch
            {
                "scythe" or "laser" or "sickle" => "shoot_scythe",
                "scorcher" or "plasma" => "shoot_scorcher",
                "breaker" => "shoot_breaker",
                "punisher" => "shoot_punisher",
                "shotgun" => "shoot_breaker",
                "dominator" => "shoot_dominator",
                "autocannon" or "anti_materiel" => "shoot_autocannon",
                "railgun" => "shoot_railgun",
                "flamethrower" => "shoot_flamethrower",
                "quasar" => "shoot_quasar",
                _ => "shoot_liberator"
            };
            Play(key);
        }

        public void PlayReload() => Play("reload");
        public void PlayEmptyClick() => Play("empty_click");
        public void PlayTurretDeploy() => Play("turret_deploy");
        public void PlayExplosion500kg() => Play("explosion_500kg");
        public void PlayExplosion() => Play("explosion");
        public void PlayExplosion(string type) => Play(type == "500kg" || type == "orbital" ? "explosion_500kg" : "explosion");
        public void PlayVoiceAlert() => Play("victory");
        public void PlayDeflect() => Play("deflect");
        public void PlayCrit() => Play("crit");
        public void PlayAcid() => Play("acid");
        public void PlayDive() => Play("dive");
        public void PlayPelican() => Play("pelican");
        public void PlayVictory() => Play("victory");
        public void PlayBerserkerSaw() => Play("buzzsaw");

        private void Play(string key)
        {
            if (!_initialized) return;
            if (_cachedSounds.TryGetValue(key, out var snd))
            {
                Raylib.PlaySound(snd);
            }
        }

        // =========================================================================
        // PIPELINE DSP POST-PROCESAMIENTO SUAVE Y CÁLIDO (ANTI-FATIGA AUDITIVA)
        // =========================================================================
        private Sound CreateProcessedSound(float[] raw, float duration, float targetGain = 19000f, float lpfCutoff = 2600f)
        {
            int count = raw.Length;
            short[] samples = new short[count];

            float dt = 1f / SampleRate;
            float attackTime = 0.0035f;
            float releaseTime = 0.007f;

            // Filtro pasa-bajos de dos polos (Butterworth-like suave roll-off)
            float rc = 1f / (2f * MathF.PI * lpfCutoff);
            float alpha = dt / (rc + dt);
            float lp1 = 0f, lp2 = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float s = raw[i];

                // Rampa de ataque (elimina el "clack" de corriente continua al iniciar)
                if (t < attackTime) s *= (t / attackTime);
                // Rampa de salida (elimina el click al terminar la muestra)
                if (t > duration - releaseTime) s *= MathF.Max(0f, (duration - t) / releaseTime);

                // Saturación suave analógica Soft-Knee (sin ondas cuadradas estridentes)
                float sat = MathF.Tanh(s * 1.15f);

                // Doble filtrado IIR para suavidad de agudos
                lp1 += alpha * (sat - lp1);
                lp2 += alpha * (lp1 - lp2);

                // Normalización con margen de seguridad (headroom limpio de 20k)
                samples[i] = (short)Math.Clamp(lp2 * targetGain, -26000f, 26000f);
            }

            return LoadSoundFromShortBuffer(samples);
        }

        // =========================================================================
        // DSP 1: RIFLE DE ASALTO LIBERATOR AR-23 (PUNCH 5.56 NATO + MECÁNICA BULLPUP)
        // =========================================================================
        private Sound GenerateLiberatorGunshot()
        {
            float duration = 0.22f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phaseKick = 0f, lpPowder = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;

                // 1. Kick sub-bajo redondo (90Hz -> 45Hz)
                float sKick = 0f;
                if (t < 0.045f)
                {
                    float kickEnv = MathF.Pow(1f - t / 0.045f, 1.4f);
                    float freq = 45f + 145f * MathF.Exp(-t * 70f);
                    phaseKick += 2f * MathF.PI * freq / SampleRate;
                    sKick = MathF.Sin(phaseKick) * kickEnv * 1.25f;
                }

                // 2. Expansión de pólvora (ruido filtrado cálido a 1800Hz)
                float sPowder = 0f;
                if (t < 0.11f)
                {
                    float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                    float cutoff = 350f + 1800f * MathF.Max(0f, 1f - t / 0.10f);
                    float alpha = (1f / SampleRate) / (1f / (2f * MathF.PI * cutoff) + (1f / SampleRate));
                    lpPowder += alpha * (white - lpPowder);
                    sPowder = lpPowder * MathF.Exp(-t * 26f) * 1.1f;
                }

                // 3. Cerrojo mecánico bullpup (click suave de acero a los 0.025s)
                float sMech = 0f;
                if (t >= 0.025f && t <= 0.075f)
                {
                    float tm = t - 0.025f;
                    float fm = MathF.Sin(2f * MathF.PI * 680f * tm + 2.5f * MathF.Sin(2f * MathF.PI * 980f * tm));
                    sMech = fm * MathF.Exp(-tm * 55f) * 0.45f;
                }

                raw[i] = sKick * 1.05f + sPowder * 0.95f + sMech * 0.4f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 19500f, lpfCutoff: 2600f);
        }

        // =========================================================================
        // DSP 2: ESCOPETA AUTOMÁTICA BREAKER SG-225 (ONDAS CALIBRE 12 + TAMBOR)
        // =========================================================================
        private Sound GenerateBreakerGunshot()
        {
            float duration = 0.28f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phaseKick = 0f, lpPowder = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;

                // Doble golpe sub-bajo calibre 12 (55Hz -> 28Hz)
                float sKick = 0f;
                if (t < 0.07f)
                {
                    float kickEnv = MathF.Pow(1f - t / 0.07f, 1.25f);
                    float freq = 30f + 120f * MathF.Exp(-t * 45f);
                    phaseKick += 2f * MathF.PI * freq / SampleRate;
                    sKick = MathF.Sin(phaseKick) * kickEnv * 1.8f;
                }

                // Ráfaga ancha de perdigones amortiguada
                float sPowder = 0f;
                if (t < 0.16f)
                {
                    float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                    float cutoff = 260f + 1700f * MathF.Max(0f, 1f - t / 0.15f);
                    float alpha = (1f / SampleRate) / (1f / (2f * MathF.PI * cutoff) + (1f / SampleRate));
                    lpPowder += alpha * (white - lpPowder);
                    sPowder = lpPowder * MathF.Exp(-t * 18f) * 1.3f;
                }

                // Tambor rotativo mecánico a los 0.04s
                float sDrum = 0f;
                if (t >= 0.04f && t <= 0.10f)
                {
                    float tm = t - 0.04f;
                    sDrum = MathF.Sin(2f * MathF.PI * 480f * tm) * MathF.Exp(-tm * 40f) * 0.35f;
                }

                raw[i] = sKick * 1.15f + sPowder * 1.1f + sDrum * 0.35f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 20000f, lpfCutoff: 2400f);
        }

        // =========================================================================
        // DSP 3: ESCOPETA PESADA PUNISHER SG-8 (ESTRUENDO CAVERNOSO + BOMBA MANUAL)
        // =========================================================================
        private Sound GeneratePunisherGunshot()
        {
            float duration = 0.34f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phaseKick = 0f, lpPowder = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;

                // Golpe cavernoso masivo (48Hz -> 24Hz)
                float sKick = 0f;
                if (t < 0.08f)
                {
                    float kickEnv = MathF.Pow(1f - t / 0.08f, 1.2f);
                    float freq = 24f + 110f * MathF.Exp(-t * 40f);
                    phaseKick += 2f * MathF.PI * freq / SampleRate;
                    sKick = MathF.Sin(phaseKick) * kickEnv * 1.95f;
                }

                // Estallido sordo de pólvora pesada
                float sPowder = 0f;
                if (t < 0.20f)
                {
                    float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                    float cutoff = 220f + 1400f * MathF.Max(0f, 1f - t / 0.17f);
                    float alpha = (1f / SampleRate) / (1f / (2f * MathF.PI * cutoff) + (1f / SampleRate));
                    lpPowder += alpha * (white - lpPowder);
                    sPowder = lpPowder * MathF.Exp(-t * 14f) * 1.25f;
                }

                // Bombeo manual de corredera (clack de madera/polímero)
                float sPump = 0f;
                if (t >= 0.13f && t <= 0.23f)
                {
                    float tm = t - 0.13f;
                    float fPump = tm < 0.05f ? 520f : 680f;
                    sPump = MathF.Sin(2f * MathF.PI * fPump * tm) * MathF.Exp(-tm * 32f) * 0.35f;
                }

                raw[i] = sKick * 1.1f + sPowder * 1.15f + sPump * 0.4f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 20500f, lpfCutoff: 2200f);
        }

        // =========================================================================
        // DSP 4: JAR-5 DOMINATOR (DOBLE PROPULSIÓN: DETONACIÓN + COHETE GYROJET)
        // =========================================================================
        private Sound GenerateDominatorGunshot()
        {
            float duration = 0.32f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phaseKick = 0f, phaseRocket = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;

                // 1. Detonación de expulsión inicial
                float sLaunch = 0f;
                if (t < 0.05f)
                {
                    float env = MathF.Pow(1f - t / 0.05f, 1.35f);
                    float freq = 38f + 130f * MathF.Exp(-t * 60f);
                    phaseKick += 2f * MathF.PI * freq / SampleRate;
                    sLaunch = MathF.Sin(phaseKick) * env * 1.45f;
                }

                // 2. Encendido del micro-cohete Gyrojet (siseo cálido y potente)
                float sRocket = 0f;
                if (t >= 0.03f && t <= 0.26f)
                {
                    float tr = t - 0.03f;
                    float rEnv = MathF.Sin((tr / 0.23f) * MathF.PI);
                    float rFreq = 260f + 380f * (tr / 0.23f);
                    phaseRocket += 2f * MathF.PI * rFreq / SampleRate;
                    float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                    sRocket = (MathF.Sin(phaseRocket) * 0.55f + white * 0.35f) * rEnv * 1.1f;
                }

                raw[i] = sLaunch * 1.05f + sRocket * 1.0f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 19500f, lpfCutoff: 2500f);
        }

        // =========================================================================
        // DSP 5: RIFLE LÁSER SICKLE LAS-16 / SCYTHE (ENERGÍA PULSADA CÁLIDA + CHORUS)
        // =========================================================================
        private Sound GenerateScytheBeam()
        {
            float duration = 0.22f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phaseC1 = 0f, phaseC2 = 0f, phaseM = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Pow(1f - (float)i / frameCount, 1.5f);

                // Modulación armónica suave descendente
                float modFreq = 180f + 420f * MathF.Exp(-t * 18f);
                phaseM += 2f * MathF.PI * modFreq / SampleRate;
                float modSignal = MathF.Sin(phaseM) * (2.2f * (1f - t / duration));

                // Portadora dual suave estilo sci-fi aterciopelado
                phaseC1 += 2f * MathF.PI * (280f + modSignal * 90f) / SampleRate;
                phaseC2 += 2f * MathF.PI * (562f + modSignal * 110f) / SampleRate;

                // Siseo sordo de disipación
                float hiss = (float)(_rnd.NextDouble() * 2.0 - 1.0) * 0.14f;

                raw[i] = (MathF.Sin(phaseC1) * 0.65f + MathF.Sin(phaseC2) * 0.35f + hiss) * env;
            }

            return CreateProcessedSound(raw, duration, targetGain: 18500f, lpfCutoff: 3000f);
        }

        // =========================================================================
        // DSP 6: RIFLE DE PLASMA SCORCHER PLAS-1 (PULSO DE CONFINAMIENTO MAGNÉTICO)
        // =========================================================================
        private Sound GenerateScorcherPlasma()
        {
            float duration = 0.22f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phasePlasma = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Pow(1f - (float)i / frameCount, 1.6f);

                // Estallido de plasma con barrido suave
                float freq = 380f * MathF.Exp(-t * 24f) + 110f;
                phasePlasma += 2f * MathF.PI * freq / SampleRate;
                float sPlasma = MathF.Sin(phasePlasma + MathF.Sin(phasePlasma * 1.8f) * 1.2f);

                raw[i] = sPlasma * env * 1.35f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 19000f, lpfCutoff: 2700f);
        }

        // =========================================================================
        // DSP 7: CAÑÓN AUTOMÁTICO AC-8 (IMPACTO ARTILLERO 20mm + CASQUILLO PESADO)
        // =========================================================================
        private Sound GenerateAutocannonGunshot()
        {
            float duration = 0.38f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phaseKick = 0f, lpPowder = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;

                // 1. Golpe sísmico de 20mm (32Hz a 95Hz)
                float sKick = 0f;
                if (t < 0.09f)
                {
                    float kickEnv = MathF.Pow(1f - t / 0.09f, 1.25f);
                    float freq = 30f + 115f * MathF.Exp(-t * 35f);
                    phaseKick += 2f * MathF.PI * freq / SampleRate;
                    sKick = MathF.Sin(phaseKick) * kickEnv * 2.1f;
                }

                // 2. Onda de choque expansiva amortiguada
                float sPowder = 0f;
                if (t < 0.22f)
                {
                    float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                    float cutoff = 200f + 1600f * MathF.Max(0f, 1f - t / 0.18f);
                    float alpha = (1f / SampleRate) / (1f / (2f * MathF.PI * cutoff) + (1f / SampleRate));
                    lpPowder += alpha * (white - lpPowder);
                    sPowder = lpPowder * MathF.Exp(-t * 13f) * 1.35f;
                }

                // 3. Deslizamiento de la recámara de acero (0.07s a 0.16s)
                float sSlide = 0f;
                if (t >= 0.07f && t <= 0.17f)
                {
                    float tm = t - 0.07f;
                    sSlide = MathF.Sin(2f * MathF.PI * 380f * tm + 2f * MathF.Sin(2f * MathF.PI * 720f * tm)) * MathF.Exp(-tm * 28f) * 0.45f;
                }

                raw[i] = sKick * 1.15f + sPowder * 1.2f + sSlide * 0.5f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 20500f, lpfCutoff: 2300f);
        }

        // =========================================================================
        // DSP 8: CAÑÓN DE RIEL RS-422 (ACELERACIÓN ELECTROMAGNÉTICA + SNAP IÓNICO)
        // =========================================================================
        private Sound GenerateRailgunDischarge()
        {
            float duration = 0.38f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phaseMag = 0f, phaseCrack = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;

                // 1. Crack supersónico enfocado (1200Hz cayendo a 220Hz)
                float sCrack = 0f;
                if (t < 0.07f)
                {
                    float fCrack = 1200f * MathF.Exp(-t * 40f);
                    phaseCrack += 2f * MathF.PI * fCrack / SampleRate;
                    sCrack = MathF.Sin(phaseCrack) * MathF.Exp(-t * 30f) * 1.6f;
                }

                // 2. Pulso electromagnético profundo de inductancia (55Hz)
                float sMag = 0f;
                if (t < 0.22f)
                {
                    phaseMag += 2f * MathF.PI * (50f + 90f * MathF.Exp(-t * 22f)) / SampleRate;
                    sMag = MathF.Sin(phaseMag) * MathF.Exp(-t * 11f) * 1.9f;
                }

                // 3. Silbido iónico residual suave
                float white = (float)(_rnd.NextDouble() * 2.0 - 1.0) * MathF.Exp(-t * 14f) * 0.22f;

                raw[i] = sCrack * 1.1f + sMag * 1.25f + white;
            }

            return CreateProcessedSound(raw, duration, targetGain: 20000f, lpfCutoff: 2700f);
        }

        // =========================================================================
        // DSP 9: LANZALLAMAS X-45 (RUGIDO CONTINUO DE GAS COMBUSTIBLE A PRESIÓN)
        // =========================================================================
        private Sound GenerateFlamethrowerRoar()
        {
            float duration = 0.18f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float lpNoise = 0f, phaseHum = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);

                // Filtro paso bajo suave para el bufido sordo del napalm (400Hz - 700Hz)
                float cutoff = 380f + 250f * MathF.Sin(t * 70f);
                float alpha = (1f / SampleRate) / (1f / (2f * MathF.PI * cutoff) + (1f / SampleRate));
                lpNoise += alpha * (white - lpNoise);

                // Zumbido cálido de combustión (75Hz)
                phaseHum += 2f * MathF.PI * 75f / SampleRate;
                float hum = MathF.Sin(phaseHum) * 0.45f;

                raw[i] = lpNoise * 1.6f + hum;
            }

            return CreateProcessedSound(raw, duration, targetGain: 18000f, lpfCutoff: 2100f);
        }

        // =========================================================================
        // DSP 10: CAÑÓN QUÁSAR LAS-99 (DESCARGA DE CONDENSADOR CUÁNTICO)
        // =========================================================================
        private Sound GenerateQuasarPulse()
        {
            float duration = 0.52f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float phasePlasma = 0f, phaseSub = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;

                // Sub-bajo de liberación (38Hz -> 80Hz)
                float fSub = 38f + 70f * MathF.Exp(-t * 10f);
                phaseSub += 2f * MathF.PI * fSub / SampleRate;
                float sSub = MathF.Sin(phaseSub) * MathF.Exp(-t * 5f) * 1.9f;

                // Modulación cuántica sin estridencia
                float fMod = 280f + 50f * MathF.Sin(t * 30f);
                phasePlasma += 2f * MathF.PI * fMod / SampleRate;
                float sPlasma = MathF.Sin(phasePlasma + 1.8f * MathF.Sin(phaseSub)) * MathF.Exp(-t * 7f) * 0.95f;

                raw[i] = sSub * 1.15f + sPlasma * 0.85f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 19500f, lpfCutoff: 2500f);
        }

        // =========================================================================
        // DSP 11: EXPLOSIÓN NUCLEAR EAGLE 500KG (SILBIDO AÉREO + ONDA TECTÓNICA SÍSMICA)
        // =========================================================================
        private Sound Generate500kgNuclearRoar()
        {
            float duration = 1.8f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];

            float phaseWhistle = 0f;
            float phaseSub = 0f;
            float brownianState = 0f;
            float lpBrownian = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float sampleVal;

                if (t < 0.16f)
                {
                    // Silbido descendente suave de caída libre (600Hz a 180Hz)
                    float progress = t / 0.16f;
                    float whistleFreq = 180f + 420f * MathF.Pow(1f - progress, 2.0f);
                    phaseWhistle += 2f * MathF.PI * whistleFreq / SampleRate;
                    float whistle = MathF.Sin(phaseWhistle) * (0.25f + 0.45f * progress);

                    // Impacto al tocar el suelo
                    float impact = progress > 0.88f ? (float)(_rnd.NextDouble() * 2.0 - 1.0) * 1.1f : 0f;
                    sampleVal = whistle + impact;
                }
                else
                {
                    // Sub-bajo sísmico profundo (52Hz cayendo a 20Hz) + Ruido Browniano
                    float tBoom = t - 0.16f;
                    float boomProgress = tBoom / (duration - 0.16f);

                    float subFreq = 20f + 32f * MathF.Exp(-tBoom * 1.8f);
                    phaseSub += 2f * MathF.PI * subFreq / SampleRate;
                    float subBass = MathF.Sin(phaseSub) * MathF.Exp(-tBoom * 1.2f) * 1.4f;

                    // Paseo aleatorio integrado (ruido marrón cálido)
                    float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                    brownianState = (brownianState * 0.95f) + white * 0.14f;

                    // Filtro Low-Pass a 200Hz para rugido de cine
                    float cutoff = 200f;
                    float dt = 1f / SampleRate;
                    float rc = 1f / (2f * MathF.PI * cutoff);
                    float alpha = dt / (rc + dt);
                    lpBrownian += alpha * (brownianState - lpBrownian);

                    // Modulación por onda lenta de 12Hz para vibración tectónica
                    float lfoRoar = 0.75f + 0.25f * MathF.Sin(2f * MathF.PI * 12f * tBoom);
                    float masterEnv = MathF.Pow(1f - boomProgress, 1.25f);

                    sampleVal = (subBass + lpBrownian * 1.2f) * lfoRoar * masterEnv;
                }

                raw[i] = sampleVal;
            }

            return CreateProcessedSound(raw, duration, targetGain: 22000f, lpfCutoff: 1800f);
        }

        // =========================================================================
        // DSP 12: EXPLOSIONES ORGÁNICAS TÁCTICAS (GRANADAS, ORBITALES, MISILES)
        // =========================================================================
        private Sound GenerateExplosion(float duration, bool isHeavy)
        {
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];
            float brown = 0f;
            float lp = 0f;
            float phaseSub = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / frameCount;
                float env = MathF.Pow(1f - progress, isHeavy ? 1.5f : 2.0f);

                // Sub-punch expansivo
                float subFreq = isHeavy ? (30f + 70f * MathF.Exp(-t * 16f)) : (48f + 95f * MathF.Exp(-t * 22f));
                phaseSub += 2f * MathF.PI * subFreq / SampleRate;
                float punch = MathF.Sin(phaseSub) * MathF.Exp(-t * 12f) * 1.35f;

                // Ruido marrón cálido amortiguado
                float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                brown = brown * 0.93f + white * 0.16f;
                float alpha = 0.07f;
                lp += alpha * (brown - lp);

                raw[i] = (punch * 1.15f + lp * 1.2f) * env;
            }

            return CreateProcessedSound(raw, duration, targetGain: isHeavy ? 21000f : 19000f, lpfCutoff: isHeavy ? 2000f : 2300f);
        }

        // =========================================================================
        // DSP 13: ACCIONES TÁCTICAS (RECARGA, DESPLIEGUE, IMPACTOS, DIVE, FANFARRIA)
        // =========================================================================
        private Sound GenerateReloadSound()
        {
            float duration = 0.36f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float s = 0f;

                // Expulsión de cargador (0.02s a 0.08s)
                if (t >= 0.02f && t <= 0.08f)
                {
                    float tm = t - 0.02f;
                    s += MathF.Sin(2f * MathF.PI * 620f * tm) * MathF.Exp(-tm * 50f) * 0.4f;
                }

                // Inserción de cargador nuevo con click sólido (0.16s a 0.24s)
                if (t >= 0.16f && t <= 0.24f)
                {
                    float tm = t - 0.16f;
                    s += MathF.Sin(2f * MathF.PI * 280f * tm) * MathF.Exp(-tm * 38f) * 0.75f;
                    s += (float)(_rnd.NextDouble() * 2.0 - 1.0) * MathF.Exp(-tm * 70f) * 0.25f;
                }

                // Liberación de cerrojo (bolt slam a los 0.28s)
                if (t >= 0.28f && t <= 0.35f)
                {
                    float tm = t - 0.28f;
                    s += MathF.Sin(2f * MathF.PI * 840f * tm) * MathF.Exp(-tm * 45f) * 0.65f;
                }

                raw[i] = s;
            }

            return CreateProcessedSound(raw, duration, targetGain: 18000f, lpfCutoff: 2600f);
        }

        private Sound GenerateEmptyClick()
        {
            float duration = 0.07f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                raw[i] = MathF.Sin(2f * MathF.PI * 1250f * t) * MathF.Exp(-t * 80f) * 0.85f;
            }

            return CreateProcessedSound(raw, duration, targetGain: 16000f, lpfCutoff: 2800f);
        }

        private Sound GenerateTurretDeploySound()
        {
            float duration = 0.52f;
            int frameCount = (int)(SampleRate * duration);
            float[] raw = new float[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float air = (float)(_rnd.NextDouble() * 2.0 - 1.0) * MathF.Exp(-t * 10f) * 0.45f;
                float thud = (t < 0.12f) ? MathF.Sin(2f * MathF.PI * 55f * t) * MathF.Exp(-t * 25f) * 1.3f : 0f;
                float metal = (t >= 0.08f && t <= 0.22f) ? MathF.Sin(2f * MathF.PI * 580f * (t - 0.08f)) * MathF.Exp(-(t - 0.08f) * 22f) * 0.5f : 0f;

                raw[i] = air + thud + metal;
            }

            return CreateProcessedSound(raw, duration, targetGain: 19000f, lpfCutoff: 2400f);
        }

        private Sound GenerateDeflectRicochet()
        {
            int frameCount = (int)(SampleRate * 0.10f);
            float[] raw = new float[frameCount];
            float phase = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float freq = 1200f - 600f * MathF.Exp(-t * 35f);
                phase += 2f * MathF.PI * freq / SampleRate;
                raw[i] = MathF.Sin(phase) * MathF.Exp(-t * 30f) * 0.8f;
            }

            return CreateProcessedSound(raw, 0.10f, targetGain: 17000f, lpfCutoff: 2600f);
        }

        private Sound GenerateFleshyCritPunch()
        {
            int frameCount = (int)(SampleRate * 0.14f);
            float[] raw = new float[frameCount];
            float phase = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float freq = 75f + 120f * MathF.Exp(-t * 40f);
                phase += 2f * MathF.PI * freq / SampleRate;
                float squish = (float)(_rnd.NextDouble() * 2.0 - 1.0) * MathF.Exp(-t * 50f) * 0.45f;
                raw[i] = MathF.Sin(phase) * MathF.Exp(-t * 22f) * 1.1f + squish;
            }

            return CreateProcessedSound(raw, 0.14f, targetGain: 19000f, lpfCutoff: 2200f);
        }

        private Sound GenerateAcidSizzle()
        {
            int frameCount = (int)(SampleRate * 0.24f);
            float[] raw = new float[frameCount];
            float lp = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                float bubble = MathF.Sin(t * 140f + MathF.Sin(t * 240f) * 3f);
                lp += 0.2f * (white * bubble - lp);
                raw[i] = lp * MathF.Exp(-t * 6f) * 0.9f;
            }

            return CreateProcessedSound(raw, 0.24f, targetGain: 17000f, lpfCutoff: 2400f);
        }

        private Sound GenerateDiveWhoosh()
        {
            int frameCount = (int)(SampleRate * 0.20f);
            float[] raw = new float[frameCount];
            float lp = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / frameCount;
                float cutoff = 220f + 650f * MathF.Sin(progress * MathF.PI);
                float dt = 1f / SampleRate;
                float rc = 1f / (2f * MathF.PI * cutoff);
                float alpha = dt / (rc + dt);
                float white = (float)(_rnd.NextDouble() * 2.0 - 1.0);
                lp += alpha * (white - lp);
                float env = MathF.Sin(progress * MathF.PI);
                raw[i] = lp * env * 1.2f;
            }

            return CreateProcessedSound(raw, 0.20f, targetGain: 18000f, lpfCutoff: 2100f);
        }

        private Sound GeneratePelicanTurbine()
        {
            int frameCount = (int)(SampleRate * 0.80f);
            float[] raw = new float[frameCount];
            float phase1 = 0f, phase2 = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                phase1 += 2f * MathF.PI * 85f / SampleRate;
                phase2 += 2f * MathF.PI * 170f / SampleRate;
                float rotor = (MathF.Sin(phase1) * 0.7f + MathF.Sin(phase2) * 0.35f);
                float hiss = (float)(_rnd.NextDouble() * 0.2 - 0.1);
                float env = MathF.Sin(((float)i / frameCount) * MathF.PI);
                raw[i] = (rotor + hiss) * env * 1.1f;
            }

            return CreateProcessedSound(raw, 0.80f, targetGain: 18500f, lpfCutoff: 2200f);
        }

        private Sound GenerateDemocracyFanfare()
        {
            int frameCount = (int)(SampleRate * 0.48f);
            float[] raw = new float[frameCount];
            float[] chord = { 392f, 493.88f, 587.33f, 783.99f }; // G mayor triunfal cálido

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float s = 0f;
                foreach (var f in chord)
                {
                    s += MathF.Sin(2f * MathF.PI * f * t);
                }
                s /= chord.Length;
                float env = MathF.Pow(1f - (float)i / frameCount, 1.2f);
                raw[i] = s * env * 1.25f;
            }

            return CreateProcessedSound(raw, 0.48f, targetGain: 18000f, lpfCutoff: 2800f);
        }

        private Sound GenerateBerserkerSaw()
        {
            int frameCount = (int)(SampleRate * 0.30f);
            float[] raw = new float[frameCount];
            float phase = 0f;

            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / SampleRate;
                float sawFreq = 140f + 30f * MathF.Sin(t * 35f);
                phase += 2f * MathF.PI * sawFreq / SampleRate;
                float saw = (2f * (phase / (2f * MathF.PI) - MathF.Floor(phase / (2f * MathF.PI) + 0.5f)));
                float grit = (float)(_rnd.NextDouble() * 2.0 - 1.0) * 0.25f;
                raw[i] = (saw + grit) * 0.9f;
            }

            return CreateProcessedSound(raw, 0.30f, targetGain: 17500f, lpfCutoff: 2300f);
        }

        private Sound LoadSoundFromShortBuffer(short[] samples)
        {
            fixed (short* ptr = samples)
            {
                Wave wave = new()
                {
                    FrameCount = (uint)samples.Length,
                    SampleRate = SampleRate,
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
            _cachedSounds.Clear();
            Raylib.CloseAudioDevice();
        }
    }
}
