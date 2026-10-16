using System.Numerics;
using Raylib_cs;
using HelldiversDesktop.Models;
using HelldiversDesktop.Audio;
using HelldiversDesktop.Rendering;

namespace HelldiversDesktop;

public static class Program
{
    // Game state
    private static GameState _state = GameState.Menu;
    private static GameState _previousState = GameState.Menu;
    private static SaveData _save = new();
    private static SoundManager _sound = new();
    private static Campaign _campaign = new();
    private static Player _player = new();

    // World & Camera
    private static Camera2D _camera;
    private static Vector2 _extractionPos;
    private static bool _extractionAvailable = false;
    private static bool _extractionActive = false;
    private static float _extractionTimer = 22f;
    private static bool _pelicanLanded = false;
    private static float _hordeSpawnTimer = 4.0f;
    private static float _screenShake = 0f;
    private static Random _rnd = new();

    // Entities
    private static List<Enemy> _enemies = new();
    private static List<Bullet> _bullets = new();
    private static List<Particle> _particles = new();
    private static List<DamageNumber> _damageNumbers = new();
    private static List<SplatterDecal> _splatters = new();
    private static List<AcidPool> _acidPools = new();
    private static List<BattlefieldDebris> _debrisList = new();
    private static List<AtmosphericMote> _ambientMotes = new();
    private static List<Vector2> _terrainRocks = new();

    // Active Stratagem Instances
    private static List<AirstrikeInstance> _airstrikes = new();
    private static List<TurretInstance> _turrets = new();
    private static List<OrbitalLaserInstance> _orbitalLasers = new();
    private static List<GasCloudInstance> _gasClouds = new();
    private static float[] _stratagemCooldowns = new float[3];

    // Comms / Alerts ticker
    private static string _radioAlert = "SUPER TIERRA: ¡DEFENDED LA LIBERTAD Y LA DEMOCRACIA GESTIONADA!";
    private static float _radioAlertTimer = 5.0f;

    // UI selection state
    private static int _selectedStratagemSlot = 0;
    private static int _selectedBestiaryIndex = 1;

    public static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(Constants.DefaultScreenWidth, Constants.DefaultScreenHeight, "HELLDIVERS 2: DEFENDERS OF DEMOCRACY (DESKTOP NATIVE)");
        Raylib.SetTargetFPS(Constants.TargetFps);

        EnemySpriteManager.Init();
        _save = SaveData.Load();
        _sound.Init(_save.Settings.MasterVolume);

        if (_save.Settings.Fullscreen && !Raylib.IsWindowFullscreen())
        {
            Raylib.ToggleFullscreen();
        }

        _camera = new Camera2D
        {
            Zoom = 1.0f,
            Offset = new Vector2(Constants.DefaultScreenWidth / 2f, Constants.DefaultScreenHeight / 2f)
        };

        // Initialize atmospheric particles
        for (int i = 0; i < 90; i++)
        {
            _ambientMotes.Add(new AtmosphericMote
            {
                Position = new Vector2((float)_rnd.NextDouble() * Constants.DefaultScreenWidth, (float)_rnd.NextDouble() * Constants.DefaultScreenHeight),
                Velocity = new Vector2((float)_rnd.NextDouble() * 30f - 15f, (float)_rnd.NextDouble() * 20f + 10f),
                Size = (float)_rnd.NextDouble() * 2.5f + 1.2f,
                Alpha = (float)_rnd.NextDouble() * 0.45f + 0.2f
            });
        }

        // Generate initial campaign
        StartNewCampaign();

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            if (dt > 0.05f) dt = 0.05f; // Cap delta to prevent tunneling

            Update(dt);
            Draw();
        }

        _sound.Close();
        EnemySpriteManager.Close();
        Raylib.CloseWindow();
    }

    private static void StartNewCampaign()
    {
        _campaign = Campaign.GenerateRandom(_save.PrestigeLevel);
        _extractionPos = new Vector2(Constants.WorldSize / 2f, Constants.WorldSize / 2f);
        _extractionAvailable = false;
        _extractionActive = false;
        _extractionTimer = 22f;
        _pelicanLanded = false;
        _hordeSpawnTimer = 4.0f;

        // Clear world entities
        _enemies.Clear();
        _bullets.Clear();
        _particles.Clear();
        _damageNumbers.Clear();
        _splatters.Clear();
        _acidPools.Clear();
        _debrisList.Clear();
        _airstrikes.Clear();
        _turrets.Clear();
        _orbitalLasers.Clear();
        _gasClouds.Clear();
        Array.Clear(_stratagemCooldowns, 0, _stratagemCooldowns.Length);

        // Generate tactical terrain rocks avoiding extraction and player spawn
        _terrainRocks.Clear();
        for (int i = 0; i < 45; i++)
        {
            Vector2 rockPos = new Vector2((float)_rnd.NextDouble() * (Constants.WorldSize - 400) + 200, (float)_rnd.NextDouble() * (Constants.WorldSize - 400) + 200);
            if (Vector2.Distance(rockPos, _extractionPos) > 120 && Vector2.Distance(rockPos, new Vector2(Constants.WorldSize / 2f, Constants.WorldSize / 2f)) > 100)
            {
                _terrainRocks.Add(rockPos);
            }
        }

        // Initialize Player
        _player.Reset(_save);
        // Apply Hangar damage bonus
        _player.CurrentWeapon.Damage = (int)(WeaponRegistry.GetWeapon(_save.EquippedWeapon).Damage * (1f + _save.Hangar.DamageLevel * 0.10f));

        // Spawn initial HVT if mission is Assassinate
        if (_campaign.Mission.Type == MissionType.Assassinate)
        {
            var hvtType = _campaign.Planet.Faction == Faction.Terminids ? EnemyType.Titan : EnemyType.Hulk;
            var hvtPos = new Vector2(Constants.WorldSize * 0.75f, Constants.WorldSize * 0.75f);
            _enemies.Add(Enemy.Create(hvtType, _campaign.Planet.Faction, hvtPos, isHvt: true));
        }

        SetVoiceAlert($"DESPLIEGUE EN {_campaign.Planet.Name.ToUpper()}: {_campaign.Mission.Title}");
    }

    private static void SetVoiceAlert(string text)
    {
        _radioAlert = text;
        _radioAlertTimer = 5.0f;
    }

    private static void Update(float dt)
    {
        // Update screen shake
        if (_screenShake > 0)
        {
            _screenShake = MathF.Max(0, _screenShake - dt * 25f);
        }

        // Update radio alert timer
        if (_radioAlertTimer > 0)
        {
            _radioAlertTimer -= dt;
        }

        // Global ambient motes
        foreach (var m in _ambientMotes)
        {
            m.Position += m.Velocity * dt;
            if (m.Position.Y > Raylib.GetScreenHeight()) m.Position.Y = 0;
            if (m.Position.X > Raylib.GetScreenWidth()) m.Position.X = 0;
            if (m.Position.X < 0) m.Position.X = Raylib.GetScreenWidth();
        }

        switch (_state)
        {
            case GameState.Menu:
                UpdateMenu();
                break;
            case GameState.Playing:
                UpdatePlaying(dt);
                break;
            case GameState.Paused:
                if (Raylib.IsKeyPressed(KeyboardKey.Escape)) _state = GameState.Playing;
                break;
            case GameState.Armory:
                UpdateArmory();
                break;
            case GameState.Stratagems:
                UpdateStratagems();
                break;
            case GameState.Hangar:
                UpdateHangar();
                break;
            case GameState.Settings:
                UpdateSettings();
                break;
            case GameState.Bestiary:
                UpdateBestiary();
                break;
            case GameState.Victory:
                if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    _save.AddXp(120);
                    _save.Samples += 4;
                    _save.Medals += 2;
                    _save.OperationsCompleted++;
                    _save.Save();
                    StartNewCampaign();
                    _state = GameState.Menu;
                }
                break;
            case GameState.GameOver:
                if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    StartNewCampaign();
                    _state = GameState.Menu;
                }
                break;
        }
    }

    private static void UpdateMenu()
    {
        Vector2 mouse = Raylib.GetMousePosition();
        int cx = Raylib.GetScreenWidth() / 2;

        Rectangle btnPlay = new(cx - 150, 480, 300, 50);
        Rectangle btnArmory = new(cx - 280, 545, 175, 42);
        Rectangle btnStrat = new(cx - 90, 545, 175, 42);
        Rectangle btnHangar = new(cx + 100, 545, 175, 42);
        Rectangle btnReroll = new(cx - 280, 600, 175, 38);
        Rectangle btnBestiary = new(cx - 90, 600, 175, 38);
        Rectangle btnSettings = new(cx + 100, 600, 175, 38);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(mouse, btnPlay))
            {
                _state = GameState.Playing;
                _sound.PlayShoot("ar");
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnArmory))
            {
                _state = GameState.Armory;
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnStrat))
            {
                _state = GameState.Stratagems;
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnHangar))
            {
                _state = GameState.Hangar;
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnReroll))
            {
                StartNewCampaign();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnBestiary))
            {
                _previousState = GameState.Menu;
                _state = GameState.Bestiary;
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnSettings))
            {
                _previousState = GameState.Menu;
                _state = GameState.Settings;
                _sound.PlayDeflect();
            }
        }
    }

    private static void UpdatePlaying(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _state = GameState.Paused;
            return;
        }

        // Camera follow with screen shake
        _camera.Target = _player.Position;
        _camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
        if (_save.Settings.ScreenShake && _screenShake > 0)
        {
            float ox = ((float)_rnd.NextDouble() - 0.5f) * _screenShake * 2f;
            float oy = ((float)_rnd.NextDouble() - 0.5f) * _screenShake * 2f;
            _camera.Offset += new Vector2(ox, oy);
        }

        // WASD Input
        Vector2 moveInput = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W)) moveInput.Y -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.S)) moveInput.Y += 1f;
        if (Raylib.IsKeyDown(KeyboardKey.A)) moveInput.X -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.D)) moveInput.X += 1f;

        // Tactical Dive
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            _player.Dive(moveInput);
            _sound.PlayDive();
        }

        // Stratagems cooldowns & hotkeys (1, 2, 3)
        for (int i = 0; i < 3; i++)
        {
            if (_stratagemCooldowns[i] > 0) _stratagemCooldowns[i] -= dt;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.One)) TriggerStratagem(0);
        if (Raylib.IsKeyPressed(KeyboardKey.Two)) TriggerStratagem(1);
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) TriggerStratagem(2);

        Vector2 mouseScreen = Raylib.GetMousePosition();
        Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, _camera);
        bool isFiring = Raylib.IsMouseButtonDown(MouseButton.Left);

        // Update Player (Shooting, Cape simulation, Shield regen, Aim mode)
        _player.Update(dt, moveInput, _enemies,
            spawnBullet: b => _bullets.Add(b),
            spawnParticle: p => _particles.Add(p),
            playShootSfx: () =>
            {
                if (_player.CurrentWeapon.IsLaser) _sound.PlayShoot("laser");
                else if (_player.CurrentWeapon.IsPlasma) _sound.PlayShoot("plasma");
                else if (_player.CurrentWeapon.Pellets > 1) _sound.PlayShoot("shotgun");
                else _sound.PlayShoot("ar");
            },
            mouseWorld: mouseWorld,
            autoAim: _save.Settings.AutoAimAssist,
            isFiring: isFiring);

        // Terrain Rock Collisions for Player
        foreach (var rock in _terrainRocks)
        {
            var diff = _player.Position - rock;
            var dist = diff.Length();
            if (dist < _player.Radius + 22f && dist > 0.01f)
            {
                _player.Position = rock + Vector2.Normalize(diff) * (_player.Radius + 22f);
            }
        }

        // Check Objectives & Extraction Readiness
        UpdateObjectives(dt);

        // Enemy Horde Wave Breaches (Spawning from single directional vectors)
        UpdateEnemySpawning(dt);

        // Pairwise Soft Repulsion (Anti-Clumping Flocking)
        UpdateEnemyFlocking();

        // Update Enemies
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var e = _enemies[i];
            e.Update(dt, _player.Position, _player.IsDiving,
                spawnBullet: b => _bullets.Add(b),
                onVoiceAlert: msg => SetVoiceAlert(msg));

            // Enemy melee hit player
            float hitDist = Vector2.Distance(e.Position, _player.Position);
            if (hitDist < e.Radius + _player.Radius)
            {
                float dmg = e.Damage * dt * 2.2f;
                if (_player.IsDiving) dmg *= 0.4f; // Evasion / mitigation
                _player.TakeDamage(dmg);
            }

            if (e.Hp <= 0)
            {
                KillEnemy(e);
                _enemies.RemoveAt(i);
            }
        }

        // Update Bullets & Authentic Weakpoint Collisions
        UpdateBullets(dt);

        // Update Stratagem Entities
        UpdateActiveStratagems(dt);

        // Update VFX (Particles, Damage numbers, Acid pools, Debris)
        UpdateVfx(dt);

        // Check Player Death
        if (_player.Hp <= 0)
        {
            _sound.PlayExplosion();
            _state = GameState.GameOver;
        }

        // Check Extraction Boarding
        if (_pelicanLanded)
        {
            Vector2 rampPos = _extractionPos + new Vector2(0, 22f);
            if (Vector2.Distance(_player.Position, rampPos) < 32f)
            {
                _sound.PlayVictory();
                _state = GameState.Victory;
            }
        }
    }

    private static void TriggerStratagem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _save.EquippedStratagems.Count) return;
        if (_stratagemCooldowns[slotIndex] > 0) return;

        string id = _save.EquippedStratagems[slotIndex];
        if (!StratagemRegistry.All.TryGetValue(id, out var def)) return;

        _stratagemCooldowns[slotIndex] = def.Cooldown;
        SetVoiceAlert($"ESTRATAGEMA: {def.Name.ToUpper()} DESPLEGADO!");

        Vector2 deployPos = _player.Position + new Vector2(MathF.Cos(_player.Angle), MathF.Sin(_player.Angle)) * 120f;

        switch (id)
        {
            case "eagle_500kg":
                _airstrikes.Add(new AirstrikeInstance
                {
                    Position = deployPos,
                    Timer = 2.4f,
                    MaxTimer = 2.4f,
                    Radius = 240f,
                    Type = "500kg"
                });
                break;
            case "precision_strike":
                _airstrikes.Add(new AirstrikeInstance
                {
                    Position = deployPos,
                    Timer = 1.6f,
                    MaxTimer = 1.6f,
                    Radius = 140f,
                    Type = "precision"
                });
                break;
            case "sentry_gatling":
                _turrets.Add(new TurretInstance
                {
                    Position = _player.Position,
                    Duration = 24f,
                    FireRate = 0.08f,
                    Range = 420f,
                    Type = "gatling"
                });
                break;
            case "sentry_autocannon":
                _turrets.Add(new TurretInstance
                {
                    Position = _player.Position,
                    Duration = 28f,
                    FireRate = 0.55f,
                    Range = 550f,
                    Type = "autocannon"
                });
                break;
            case "orbital_laser":
                _orbitalLasers.Add(new OrbitalLaserInstance
                {
                    Position = deployPos,
                    Duration = 14f,
                    DamagePerSec = 280f
                });
                break;
            case "orbital_gas":
                _gasClouds.Add(new GasCloudInstance
                {
                    Position = deployPos,
                    Radius = 180f,
                    Duration = 12f
                });
                break;
        }
    }

    private static void UpdateObjectives(float dt)
    {
        // Outposts
        if (_campaign.Mission.Type == MissionType.Outposts)
        {
            bool allOutpostsDead = true;
            foreach (var outP in _campaign.Outposts)
            {
                if (!outP.Destroyed)
                {
                    allOutpostsDead = false;
                    // Outpost periodic local patrol spawn
                    outP.SpawnTimer -= dt;
                    if (outP.SpawnTimer <= 0)
                    {
                        outP.SpawnTimer = 6.5f;
                        if (_enemies.Count < 50)
                        {
                            var enemyType = _campaign.Planet.Faction == Faction.Terminids ? EnemyType.Warrior : EnemyType.Trooper;
                            _enemies.Add(Enemy.Create(enemyType, _campaign.Planet.Faction, outP.Position + new Vector2(_rnd.Next(-30, 30), _rnd.Next(-30, 30))));
                        }
                    }
                }
            }
            if (allOutpostsDead && !_extractionAvailable)
            {
                _extractionAvailable = true;
                SetVoiceAlert("SUPER TIERRA: OBJETIVO CUMPLIDO. PROCEDA A LA ZONA DE EXTRACCIÓN.");
            }
        }
        else if (_campaign.Mission.Type == MissionType.Icbm || _campaign.Mission.Type == MissionType.Seismic)
        {
            bool allCompleted = true;
            foreach (var it in _campaign.Interactables)
            {
                if (!it.Completed)
                {
                    allCompleted = false;
                    float dist = Vector2.Distance(_player.Position, it.Position);
                    if (dist < it.Radius)
                    {
                        it.Active = true;
                        it.Progress += dt;
                        if (it.Progress >= it.MaxProgress)
                        {
                            it.Completed = true;
                            _sound.PlayVictory();
                            SetVoiceAlert($"SUPER TIERRA: {it.Type.ToUpper()} ACTIVADO EXITOSAMENTE.");
                        }
                    }
                    else
                    {
                        it.Active = false;
                    }
                }
            }
            if (allCompleted && !_extractionAvailable)
            {
                _extractionAvailable = true;
                SetVoiceAlert("SUPER TIERRA: DATOS TRANSMITIDOS. EXTRACCIÓN AUTORIZADA.");
            }
        }
        else if (_campaign.Mission.Type == MissionType.Assassinate)
        {
            bool hvtAlive = _enemies.Any(e => e.IsHvt);
            if (!hvtAlive && !_extractionAvailable)
            {
                _extractionAvailable = true;
                SetVoiceAlert("SUPER TIERRA: OBJETIVO DE ALTO VALOR ELIMINADO. EXTRAIGAN AHORA.");
            }
        }

        // Extraction Zone Holdout Logic
        if (_extractionAvailable)
        {
            float distToPad = Vector2.Distance(_player.Position, _extractionPos);

            // Activate extraction when player enters within 350m
            if (!_extractionActive && distToPad <= 350f)
            {
                _extractionActive = true;
                _sound.PlayPelican();
                SetVoiceAlert("PELICAN-1: TRANSPORTE EN CAMINO. MANTENGAN EL PERÍMETRO DEFENSIVO (350M)");
            }

            if (_extractionActive && !_pelicanLanded)
            {
                _extractionTimer -= dt;

                // Spawn intense extraction wave
                if (_hordeSpawnTimer > 3.0f) _hordeSpawnTimer = 2.5f;

                if (_extractionTimer <= 0)
                {
                    _pelicanLanded = true;
                    _sound.PlayPelican();
                    SetVoiceAlert("PELICAN-1: TRANSPORTE EN TIERRA. ¡ABORDEN POR LA RAMPA TRASERA!");
                }
            }
        }
    }

    private static void UpdateEnemySpawning(float dt)
    {
        _hordeSpawnTimer -= dt;
        if (_hordeSpawnTimer <= 0)
        {
            _hordeSpawnTimer = (float)_rnd.NextDouble() * 5.0f + 11.0f;

            if (_enemies.Count >= 70) return; // Prevent excessive lag

            // Group spawn from ONE direction (Wave breach)
            float waveAngle = (float)_rnd.NextDouble() * MathF.PI * 2f;
            float spawnDist = (float)_rnd.NextDouble() * 200f + 650f;
            Vector2 clusterOrigin = _player.Position + new Vector2(MathF.Cos(waveAngle), MathF.Sin(waveAngle)) * spawnDist;

            int swarmCount = _rnd.Next(6, 12);
            for (int i = 0; i < swarmCount; i++)
            {
                Vector2 spawnPos = clusterOrigin + new Vector2(_rnd.Next(-80, 80), _rnd.Next(-80, 80));
                spawnPos.X = Math.Clamp(spawnPos.X, 50, Constants.WorldSize - 50);
                spawnPos.Y = Math.Clamp(spawnPos.Y, 50, Constants.WorldSize - 50);

                EnemyType type;
                if (_campaign.Planet.Faction == Faction.Terminids)
                {
                    double roll = _rnd.NextDouble();
                    if (roll < 0.45) type = EnemyType.Scavenger;
                    else if (roll < 0.70) type = EnemyType.Warrior;
                    else if (roll < 0.85) type = EnemyType.Hunter;
                    else if (roll < 0.94) type = EnemyType.Spitter;
                    else type = EnemyType.Charger;
                }
                else // Automatons
                {
                    double roll = _rnd.NextDouble();
                    if (roll < 0.35) type = EnemyType.Trooper;
                    else if (roll < 0.50) type = EnemyType.Commissar;
                    else if (roll < 0.72) type = EnemyType.Berserker;
                    else if (roll < 0.88) type = EnemyType.Devastator;
                    else type = EnemyType.Hulk;
                }

                _enemies.Add(Enemy.Create(type, _campaign.Planet.Faction, spawnPos));
            }

            if (_rnd.NextDouble() < 0.35)
            {
                SetVoiceAlert(_campaign.Planet.Faction == Faction.Terminids ? "☣️ ¡BRECHA DE INSECTOS DETECTADA!" : "⚠️ ¡NAVE DE TRANSPORTE AUTÓMATA DETECTADA!");
            }
        }
    }

    private static void UpdateEnemyFlocking()
    {
        // Pairwise soft repulsion to prevent enemy clumping
        for (int i = 0; i < _enemies.Count; i++)
        {
            var e1 = _enemies[i];
            for (int j = i + 1; j < _enemies.Count; j++)
            {
                var e2 = _enemies[j];
                var delta = e1.Position - e2.Position;
                var dist = delta.Length();
                float minDist = e1.Radius + e2.Radius;
                if (dist > 0.01f && dist < minDist)
                {
                    var push = Vector2.Normalize(delta) * (minDist - dist) * 0.4f;
                    e1.Position += push;
                    e2.Position -= push;
                }
            }
        }
    }

    private static void UpdateBullets(float dt)
    {
        for (int b = _bullets.Count - 1; b >= 0; b--)
        {
            var bullet = _bullets[b];
            bullet.Position += bullet.Velocity;
            bullet.Life -= dt;

            if (bullet.Life <= 0)
            {
                _bullets.RemoveAt(b);
                continue;
            }

            if (bullet.IsPlayerBullet)
            {
                bool bulletConsumed = false;

                // Collide with Outposts
                foreach (var outP in _campaign.Outposts)
                {
                    if (!outP.Destroyed && Vector2.Distance(bullet.Position, outP.Position) < outP.Radius)
                    {
                        outP.Hp -= bullet.Damage;
                        _damageNumbers.Add(new DamageNumber { Position = bullet.Position, Text = $"{(int)bullet.Damage}", Color = Constants.DemocracyRed });
                        bulletConsumed = true;
                        if (outP.Hp <= 0)
                        {
                            outP.Destroyed = true;
                            _sound.PlayExplosion();
                            _screenShake += 8f;
                            SetVoiceAlert("SUPER TIERRA: ¡PUESTO ENEMIGO DESTRUIDO!");
                        }
                        break;
                    }
                }

                if (bulletConsumed)
                {
                    _bullets.RemoveAt(b);
                    continue;
                }

                // Collide with Enemies
                for (int e = 0; e < _enemies.Count; e++)
                {
                    var enemy = _enemies[e];
                    if (Vector2.Distance(bullet.Position, enemy.Position) < enemy.Radius)
                    {
                        // Authentic Weakpoint Angle Calculation
                        var impactVec = Vector2.Normalize(bullet.Position - enemy.Position);
                        var enemyFacing = new Vector2(MathF.Cos(enemy.FacingAngle), MathF.Sin(enemy.FacingAngle));
                        float dot = Vector2.Dot(impactVec, enemyFacing);
                        float angleDiff = MathF.Acos(Math.Clamp(dot, -1f, 1f));

                        float finalDamage = bullet.Damage;
                        bool deflected = false;
                        bool crit = false;
                        string hitText = $"{(int)finalDamage}";
                        Color numColor = Color.White;

                        if (enemy.Type == EnemyType.Charger)
                        {
                            if (angleDiff < MathF.PI / 2.2f)
                            {
                                // Frontal Horn deflection
                                deflected = true;
                                finalDamage = 0;
                                hitText = "BLINDADO";
                                numColor = Color.LightGray;
                                _sound.PlayDeflect();
                            }
                            else if (angleDiff > MathF.PI * 0.62f)
                            {
                                // Rear fleshy abdomen critical
                                crit = true;
                                finalDamage *= 2.2f;
                                hitText = "CRÍTICO!";
                                numColor = Constants.OrangeHazard;
                                _sound.PlayCrit();
                            }
                        }
                        else if (enemy.Type == EnemyType.Hulk)
                        {
                            if (angleDiff < MathF.PI / 2.0f)
                            {
                                // Frontal heavy steel deflection
                                deflected = true;
                                finalDamage = 0;
                                hitText = "BLINDADO";
                                numColor = Color.LightGray;
                                _sound.PlayDeflect();
                            }
                            else if (angleDiff > MathF.PI * 0.62f)
                            {
                                // Rear radiator heatsink critical
                                crit = true;
                                finalDamage *= 2.5f;
                                hitText = "CRÍTICO TÉRMICO!";
                                numColor = Constants.DemocracyRed;
                                _sound.PlayCrit();
                            }
                        }

                        enemy.Hp -= finalDamage;
                        enemy.HitFlashTimer = 0.12f;

                        // Spawn spark / blood particles
                        for (int p = 0; p < (deflected ? 4 : 7); p++)
                        {
                            _particles.Add(new Particle
                            {
                                Position = bullet.Position,
                                Velocity = new Vector2((float)_rnd.NextDouble() * 6f - 3f, (float)_rnd.NextDouble() * 6f - 3f),
                                Radius = (float)_rnd.NextDouble() * 2f + 1f,
                                Color = deflected ? Constants.Gold : (enemy.Faction == Faction.Terminids ? Constants.OrangeHazard : Constants.Cyan),
                                Life = 0.25f
                            });
                        }

                        _damageNumbers.Add(new DamageNumber
                        {
                            Position = bullet.Position,
                            Text = hitText,
                            Color = numColor,
                            IsCrit = crit
                        });

                        bullet.Pierce--;
                        if (bullet.Pierce <= 0)
                        {
                            bulletConsumed = true;
                            break;
                        }
                    }
                }

                if (bulletConsumed)
                {
                    _bullets.RemoveAt(b);
                }
            }
            else // Enemy Bullet hitting Player
            {
                if (Vector2.Distance(bullet.Position, _player.Position) < _player.Radius + 4f)
                {
                    float dmg = bullet.Damage;
                    if (_player.IsDiving) dmg *= 0.45f;
                    _player.TakeDamage(dmg);

                    if (bullet.Type == BulletType.Acid) _sound.PlayAcid();

                    _bullets.RemoveAt(b);
                }
            }
        }
    }

    private static void KillEnemy(Enemy e)
    {
        // Add XP and Samples chance
        _save.AddXp(e.IsHvt ? 35 : (e.Type == EnemyType.Charger || e.Type == EnemyType.Hulk ? 20 : 5));
        if (_rnd.NextDouble() < 0.12) _save.Samples++;

        // Add Splatter Decal
        var decal = new SplatterDecal
        {
            Position = e.Position,
            Radius = e.Radius * 1.3f,
            IsBots = e.Faction == Faction.Automatons,
            Color = e.Faction == Faction.Terminids ? new Color(202, 138, 4, 180) : new Color(15, 23, 42, 200),
            AccentColor = e.Faction == Faction.Terminids ? Constants.OrangeHazard : Constants.DemocracyRed
        };
        for (int i = 0; i < 6; i++)
        {
            decal.Drops.Add(new SplatterDrop
            {
                Offset = new Vector2((float)_rnd.NextDouble() * e.Radius * 1.5f - e.Radius * 0.75f, (float)_rnd.NextDouble() * e.Radius * 1.5f - e.Radius * 0.75f),
                Radius = (float)_rnd.NextDouble() * 5f + 2f
            });
        }
        _splatters.Add(decal);
        if (_splatters.Count > 60) _splatters.RemoveAt(0);

        // Acid Pool on Spitter or Titan death
        if (e.Type == EnemyType.Spitter || e.Type == EnemyType.Titan)
        {
            _acidPools.Add(new AcidPool { Position = e.Position, Radius = e.Radius * 1.2f });
            _sound.PlayAcid();
        }

        // Battlefield Debris for heavy bots
        if (e.Type == EnemyType.Hulk || e.Type == EnemyType.FactoryStrider)
        {
            _debrisList.Add(new BattlefieldDebris { Position = e.Position, Radius = e.Radius, IsBots = true });
            _screenShake += 6f;
        }
    }

    private static void UpdateActiveStratagems(float dt)
    {
        // 500kg & Precision Airstrikes
        for (int i = _airstrikes.Count - 1; i >= 0; i--)
        {
            var air = _airstrikes[i];
            air.Timer -= dt;
            if (air.Timer <= 0)
            {
                _sound.PlayExplosion();
                _screenShake += 16f;

                // Massive AoE damage
                float dmg = air.Type == "500kg" ? 1600f : 1000f;
                foreach (var e in _enemies)
                {
                    if (Vector2.Distance(e.Position, air.Position) < air.Radius)
                    {
                        e.Hp -= dmg;
                    }
                }
                foreach (var outP in _campaign.Outposts)
                {
                    if (!outP.Destroyed && Vector2.Distance(outP.Position, air.Position) < air.Radius)
                    {
                        outP.Hp -= dmg;
                        if (outP.Hp <= 0) outP.Destroyed = true;
                    }
                }

                // Explosion shockwave particles
                for (int p = 0; p < 35; p++)
                {
                    float angle = (float)_rnd.NextDouble() * MathF.PI * 2f;
                    float spd = (float)_rnd.NextDouble() * 12f + 4f;
                    _particles.Add(new Particle
                    {
                        Position = air.Position,
                        Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * spd,
                        Radius = (float)_rnd.NextDouble() * 6f + 3f,
                        Color = Constants.DemocracyRed,
                        Life = 0.5f
                    });
                }

                _airstrikes.RemoveAt(i);
            }
        }

        // Turrets
        for (int i = _turrets.Count - 1; i >= 0; i--)
        {
            var t = _turrets[i];
            t.Duration -= dt;
            t.Cooldown -= dt;

            // Find closest enemy
            Enemy? closest = null;
            float minDist = t.Range;
            foreach (var e in _enemies)
            {
                if (e.Hp <= 0) continue;
                float d = Vector2.Distance(t.Position, e.Position);
                if (d < minDist) { minDist = d; closest = e; }
            }

            if (closest != null)
            {
                t.Angle = MathF.Atan2(closest.Position.Y - t.Position.Y, closest.Position.X - t.Position.X);
                if (t.Cooldown <= 0)
                {
                    t.Cooldown = t.FireRate;
                    _sound.PlayShoot(t.Type == "autocannon" ? "plasma" : "ar");

                    _bullets.Add(new Bullet
                    {
                        Position = t.Position + new Vector2(MathF.Cos(t.Angle), MathF.Sin(t.Angle)) * 18f,
                        Velocity = new Vector2(MathF.Cos(t.Angle), MathF.Sin(t.Angle)) * 18f,
                        Damage = t.Type == "autocannon" ? 280f : 45f,
                        Pierce = t.Type == "autocannon" ? 3 : 1,
                        IsPlayerBullet = true,
                        Life = 1.4f
                    });
                }
            }

            if (t.Duration <= 0) _turrets.RemoveAt(i);
        }

        // Orbital Laser
        for (int i = _orbitalLasers.Count - 1; i >= 0; i--)
        {
            var las = _orbitalLasers[i];
            las.Duration -= dt;

            // Track highest HP enemy
            Enemy? best = null;
            float maxHp = 0;
            foreach (var e in _enemies)
            {
                if (e.Hp > maxHp) { maxHp = e.Hp; best = e; }
            }

            if (best != null)
            {
                las.Position += Vector2.Normalize(best.Position - las.Position) * 4.5f;
            }

            // Deal burn damage in radius
            foreach (var e in _enemies)
            {
                if (Vector2.Distance(e.Position, las.Position) < 65f)
                {
                    e.Hp -= las.DamagePerSec * dt;
                }
            }

            if (las.Duration <= 0) _orbitalLasers.RemoveAt(i);
        }

        // Gas Clouds
        for (int i = _gasClouds.Count - 1; i >= 0; i--)
        {
            var gas = _gasClouds[i];
            gas.Duration -= dt;
            foreach (var e in _enemies)
            {
                if (Vector2.Distance(e.Position, gas.Position) < gas.Radius)
                {
                    e.Hp -= 65f * dt;
                }
            }
            if (gas.Duration <= 0) _gasClouds.RemoveAt(i);
        }
    }

    private static void UpdateVfx(float dt)
    {
        // Particles
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Position += p.Velocity;
            p.Life -= dt;
            if (p.Life <= 0) _particles.RemoveAt(i);
        }

        // Damage numbers
        for (int i = _damageNumbers.Count - 1; i >= 0; i--)
        {
            var dn = _damageNumbers[i];
            dn.Position.Y += dn.VelocityY;
            dn.Life -= dt;
            if (dn.Life <= 0) _damageNumbers.RemoveAt(i);
        }

        // Acid pools
        for (int i = _acidPools.Count - 1; i >= 0; i--)
        {
            var ap = _acidPools[i];
            ap.Life -= dt;
            if (Vector2.Distance(_player.Position, ap.Position) < ap.Radius)
            {
                _player.TakeDamage(25f * dt);
            }
            if (ap.Life <= 0) _acidPools.RemoveAt(i);
        }
    }

    private static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Constants.TerminalBg);

        if (_state == GameState.Playing || _state == GameState.Paused)
        {
            DrawPlayingWorld();
            DrawHUD();
            if (_state == GameState.Paused) DrawPauseOverlay();
        }
        else if (_state == GameState.Menu)
        {
            DrawWarTableMenu();
        }
        else if (_state == GameState.Armory)
        {
            DrawArmory();
        }
        else if (_state == GameState.Stratagems)
        {
            DrawStratagems();
        }
        else if (_state == GameState.Hangar)
        {
            DrawHangar();
        }
        else if (_state == GameState.Settings)
        {
            DrawSettings();
        }
        else if (_state == GameState.Bestiary)
        {
            DrawBestiary();
        }
        else if (_state == GameState.Victory)
        {
            DrawVictoryScreen();
        }
        else if (_state == GameState.GameOver)
        {
            DrawGameOverScreen();
        }

        Raylib.EndDrawing();
    }

    private static void DrawPlayingWorld()
    {
        Raylib.BeginMode2D(_camera);

        // 1. Terrain, Grid, Objectives, Pelican Pad & Ship
        GameRenderer.DrawWorld(_camera, _campaign, _terrainRocks, _extractionPos, _extractionActive, _pelicanLanded, _extractionTimer, _player);

        // 2. Splatter Decals
        foreach (var s in _splatters)
        {
            Raylib.DrawCircleV(s.Position, s.Radius, s.Color);
            foreach (var d in s.Drops)
            {
                Raylib.DrawCircleV(s.Position + d.Offset, d.Radius, s.AccentColor);
            }
        }

        // 3. Acid Pools
        foreach (var ap in _acidPools)
        {
            Raylib.DrawCircleV(ap.Position, ap.Radius, new Color(132, 204, 22, 110));
            Raylib.DrawCircleLines((int)ap.Position.X, (int)ap.Position.Y, ap.Radius, Constants.LibertyGreen);
        }

        // 4. Gas Clouds
        foreach (var g in _gasClouds)
        {
            Raylib.DrawCircleV(g.Position, g.Radius, new Color(22, 101, 52, 90));
            Raylib.DrawCircleLines((int)g.Position.X, (int)g.Position.Y, g.Radius, Constants.LibertyGreen);
        }

        // 5. Active Turrets
        foreach (var t in _turrets)
        {
            Raylib.DrawCircleV(t.Position, 16f, new Color(30, 41, 59, 255));
            Raylib.DrawCircleLines((int)t.Position.X, (int)t.Position.Y, 16f, Constants.Gold);
            Raylib.DrawLineEx(t.Position, t.Position + new Vector2(MathF.Cos(t.Angle), MathF.Sin(t.Angle)) * 24f, 4f, Constants.Gold);
        }

        // 6. Orbital Lasers
        foreach (var ol in _orbitalLasers)
        {
            Raylib.DrawCircleV(ol.Position, 45f, new Color(239, 68, 68, 80));
            Raylib.DrawCircleV(ol.Position, 16f, Color.White);
            Raylib.DrawCircleLines((int)ol.Position.X, (int)ol.Position.Y, 45f, Constants.DemocracyRed);
        }

        // 7. Airstrike Beacons & Impact Radius
        foreach (var air in _airstrikes)
        {
            Raylib.DrawCircleLines((int)air.Position.X, (int)air.Position.Y, air.Radius, Constants.DemocracyRed);
            Raylib.DrawCircleV(air.Position, 8f, Constants.DemocracyRed);
            Raylib.DrawText($"{air.Type.ToUpper()} {air.Timer:0.0}s", (int)air.Position.X - 25, (int)air.Position.Y - 24, 14, Constants.DemocracyRed);
        }

        // 8. Enemies
        foreach (var e in _enemies)
        {
            if (e.Hp <= 0) continue;
            if (e.Faction == Faction.Terminids)
                GameRenderer.DrawTerminid(e);
            else
                GameRenderer.DrawAutomaton(e);
        }

        // 9. Helldiver Player
        GameRenderer.DrawPlayer(_player);

        // 10. Bullets & Projectiles
        foreach (var b in _bullets)
        {
            Color bCol = b.Type switch
            {
                BulletType.Laser => Constants.Cyan,
                BulletType.Plasma => Constants.Cyan,
                BulletType.Dominator => Constants.OrangeHazard,
                BulletType.Acid => Constants.LibertyGreen,
                BulletType.Flame => Constants.OrangeHazard,
                BulletType.BotLaser => Constants.DemocracyRed,
                _ => Constants.Gold
            };
            Raylib.DrawCircleV(b.Position, b.Type == BulletType.Plasma ? 5f : 3f, bCol);
        }

        // 11. Particles & Casings
        foreach (var p in _particles)
        {
            if (p.IsTracer)
            {
                Raylib.DrawLineEx(p.TracerStart, p.Position, 2f, p.Color);
            }
            else
            {
                Raylib.DrawCircleV(p.Position, p.Radius, p.Color);
            }
        }

        // 12. Floating Damage Numbers
        if (_save.Settings.ShowDamageNumbers)
        {
            foreach (var dn in _damageNumbers)
            {
                Raylib.DrawText(dn.Text, (int)dn.Position.X, (int)dn.Position.Y, dn.IsCrit ? 18 : 13, dn.Color);
            }
        }

        // 13. Tactical Aim Reticle / Crosshair
        Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
        GameRenderer.DrawCrosshair(mouseWorld, _player.AimTarget != null);

        Raylib.EndMode2D();
    }

    private static string GetStratagemCode(string id) => id switch
    {
        "eagle_500kg" => "▲ ► ▼ ▼ ▼",
        "sentry_gatling" => "▼ ▲ ► ◄",
        "orbital_laser" => "► ▼ ▲ ► ▼",
        "sentry_autocannon" => "▼ ▲ ► ▲ ◄ ▲",
        "orbital_gas" => "► ► ▼ ►",
        "precision_strike" => "► ► ▲",
        "guard_dog" => "▼ ▲ ◄ ▲ ► ▼",
        _ => ""
    };

    private static void DrawTacticalPanel(Rectangle rec, Color borderColor, Color bgColor)
    {
        Raylib.DrawRectangleRec(rec, bgColor);
        Raylib.DrawRectangleLinesEx(rec, 1, borderColor);

        // Corner notch brackets
        int x = (int)rec.X, y = (int)rec.Y, w = (int)rec.Width, h = (int)rec.Height;
        Color notchCol = borderColor.A > 200 ? borderColor : Constants.Gold;
        // Top-left
        Raylib.DrawLine(x, y, x + 5, y, notchCol);
        Raylib.DrawLine(x, y, x, y + 5, notchCol);
        // Top-right
        Raylib.DrawLine(x + w, y, x + w - 5, y, notchCol);
        Raylib.DrawLine(x + w, y, x + w, y + 5, notchCol);
        // Bottom-left
        Raylib.DrawLine(x, y + h, x + 5, y + h, notchCol);
        Raylib.DrawLine(x, y + h, x, y + h - 5, notchCol);
        // Bottom-right
        Raylib.DrawLine(x + w, y + h, x + w - 5, y + h, notchCol);
        Raylib.DrawLine(x + w, y + h, x + w, y + h - 5, notchCol);
    }

    private static void DrawHUD()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();

        // 1. Helldiver Health & Shield Bar (Top Left)
        Rectangle vitalsRec = new(20, 20, 260, 68);
        DrawTacticalPanel(vitalsRec, Constants.BorderColor, new Color(15, 23, 42, 235));

        var (rankTitle, rankIcon) = _save.GetMilitaryRank();
        Raylib.DrawText($"{rankIcon} {rankTitle.ToUpper()}", 32, 28, 12, Constants.Gold);
        Raylib.DrawText($"NV. {_save.PrestigeLevel}", (int)vitalsRec.X + (int)vitalsRec.Width - 55, 28, 11, Constants.Cyan);

        // HP bar with low health pulse
        float hpPct = Math.Clamp(_player.Hp / _player.MaxHp, 0, 1);
        Color hpColor = hpPct < 0.25f && (int)(Raylib.GetTime() * 6) % 2 == 0 ? new Color((byte)255, (byte)50, (byte)50, (byte)255) : Constants.DemocracyRed;

        Raylib.DrawRectangle(32, 45, 236, 14, new Color(25, 30, 42, 255));
        Raylib.DrawRectangle(32, 45, (int)(236 * hpPct), 14, hpColor);
        Raylib.DrawRectangleLines(32, 45, 236, 14, hpPct < 0.25f ? Constants.DemocracyRed : Color.DarkGray);
        string hpStr = $"VIDA: {(int)_player.Hp} / {(int)_player.MaxHp}";
        Raylib.DrawText(hpStr, 32 + 118 - Raylib.MeasureText(hpStr, 10) / 2, 47, 10, Color.White);

        // Shield bar (if present)
        if (_player.MaxShield > 0)
        {
            float shPct = Math.Clamp(_player.Shield / _player.MaxShield, 0, 1);
            Raylib.DrawRectangle(32, 62, 236, 5, new Color(15, 35, 55, 255));
            Raylib.DrawRectangle(32, 62, (int)(236 * shPct), 5, Constants.Cyan);
            Raylib.DrawRectangleLines(32, 62, 236, 5, new Color(56, 189, 248, 100));
        }
        else
        {
            // Dive ready status pip
            string diveStatus = _player.DiveCooldown <= 0 ? "[ESPACIO // ESQUIVAR: LISTO]" : $"[ESQUIVAR: {_player.DiveCooldown:0.0}s]";
            Raylib.DrawText(diveStatus, 32, 63, 10, _player.DiveCooldown <= 0 ? Constants.LibertyGreen : Color.Gray);
        }

        // 2. Weapon Card & Ammo Indicator (Bottom Left)
        Rectangle wRec = new(20, sh - 82, 230, 62);
        DrawTacticalPanel(wRec, Constants.BorderColor, new Color(15, 23, 42, 235));
        Raylib.DrawText(_player.CurrentWeapon.Name.ToUpper(), (int)wRec.X + 14, (int)wRec.Y + 10, 13, Constants.Gold);
        Raylib.DrawText(_player.CurrentWeapon.TypeLabel.ToUpper(), (int)wRec.X + 14, (int)wRec.Y + 26, 10, Constants.Cyan);

        if (_player.IsReloading)
        {
            float reloadPct = 1f - Math.Clamp(_player.ReloadTimer / _player.ReloadDuration, 0, 1);
            Raylib.DrawText("RECARGANDO...", (int)wRec.X + 14, (int)wRec.Y + 42, 13, Constants.OrangeHazard);
            Raylib.DrawRectangle((int)wRec.X + 120, (int)wRec.Y + 44, 95, 10, new Color(30, 41, 59, 255));
            Raylib.DrawRectangle((int)wRec.X + 120, (int)wRec.Y + 44, (int)(95 * reloadPct), 10, Constants.OrangeHazard);
            Raylib.DrawRectangleLines((int)wRec.X + 120, (int)wRec.Y + 44, 95, 10, Color.DarkGray);
        }
        else
        {
            string ammoNow = $"{_player.Ammo}";
            Raylib.DrawText(ammoNow, (int)wRec.X + 14, (int)wRec.Y + 40, 18, Color.White);
            Raylib.DrawText($" / {_player.MaxAmmo}", (int)wRec.X + 14 + Raylib.MeasureText(ammoNow, 18), (int)wRec.Y + 44, 12, Color.LightGray);

            // Magazine pip meter
            float ammoPct = Math.Clamp((float)_player.Ammo / _player.MaxAmmo, 0, 1);
            Color ammoMeterCol = ammoPct > 0.35f ? Constants.Gold : Constants.DemocracyRed;
            Raylib.DrawRectangle((int)wRec.X + 115, (int)wRec.Y + 45, 100, 8, new Color(30, 41, 59, 255));
            Raylib.DrawRectangle((int)wRec.X + 115, (int)wRec.Y + 45, (int)(100 * ammoPct), 8, ammoMeterCol);
            Raylib.DrawRectangleLines((int)wRec.X + 115, (int)wRec.Y + 45, 100, 8, Color.DarkGray);
        }

        // 3. Stratagems Cards (Bottom Center)
        int stratWidth = 105;
        int stratTotalW = 3 * stratWidth + 24;
        int stratStartX = sw / 2 - stratTotalW / 2;

        for (int i = 0; i < 3; i++)
        {
            int bx = stratStartX + i * (stratWidth + 12);
            int by = sh - 82;
            string stratId = i < _save.EquippedStratagems.Count ? _save.EquippedStratagems[i] : "";
            StratagemRegistry.All.TryGetValue(stratId, out var def);

            Rectangle sRec = new(bx, by, stratWidth, 62);
            bool onCd = _stratagemCooldowns[i] > 0;
            DrawTacticalPanel(sRec, onCd ? Color.DarkGray : Constants.Gold, new Color(15, 23, 42, 235));

            string keyText = $"[{i + 1}]";
            Raylib.DrawText(keyText, bx + 8, by + 6, 11, Constants.Gold);
            string sName = def != null ? def.Icon : "[---]";
            Raylib.DrawText(sName, bx + 28, by + 6, 11, Color.White);

            // D-Pad call-in arrows
            string codeArrows = GetStratagemCode(stratId);
            Raylib.DrawText(codeArrows, bx + (stratWidth - Raylib.MeasureText(codeArrows, 11)) / 2, by + 24, 11, Constants.Gold);

            if (onCd)
            {
                // Semi-transparent cooldown curtain
                Raylib.DrawRectangleRec(sRec, new Color((byte)0, (byte)0, (byte)0, (byte)160));
                string cdStr = $"{_stratagemCooldowns[i]:0.0}s";
                Raylib.DrawText(cdStr, bx + (stratWidth - Raylib.MeasureText(cdStr, 14)) / 2, by + 40, 14, Constants.DemocracyRed);
            }
            else
            {
                string cat = def != null ? def.Category.ToUpper() : "LISTO";
                if (cat.Length > 12) cat = cat.Substring(0, 12);
                Raylib.DrawText(cat, bx + (stratWidth - Raylib.MeasureText(cat, 9)) / 2, by + 44, 9, Constants.LibertyGreen);
            }
        }

        // 4. Tactical Radar (Top Right)
        int mapR = 64;
        int mapX = sw - mapR - 30;
        int mapY = mapR + 25;
        Vector2 radarCenter = new(mapX, mapY);

        // Circular background
        Raylib.DrawCircleV(radarCenter, mapR, new Color(10, 15, 26, 235));
        // Distance rings
        Raylib.DrawCircleLines((int)radarCenter.X, (int)radarCenter.Y, mapR * 0.33f, new Color(56, 189, 248, 40));
        Raylib.DrawCircleLines((int)radarCenter.X, (int)radarCenter.Y, mapR * 0.66f, new Color(56, 189, 248, 40));
        Raylib.DrawCircleLines((int)radarCenter.X, (int)radarCenter.Y, mapR, Constants.BorderColor);
        // Outer decorative ring
        Raylib.DrawCircleLines((int)radarCenter.X, (int)radarCenter.Y, mapR + 3, new Color(56, 189, 248, 70));

        // Active radar sweep line
        float sweepAngle = (float)Raylib.GetTime() * 2.2f;
        Vector2 sweepEnd = radarCenter + new Vector2(MathF.Cos(sweepAngle), MathF.Sin(sweepAngle)) * mapR;
        Raylib.DrawLineEx(radarCenter, sweepEnd, 1.5f, new Color(56, 189, 248, 120));

        // Radar cardinal markers
        Raylib.DrawText("N", (int)radarCenter.X - 3, (int)radarCenter.Y - mapR + 3, 8, Constants.Cyan);
        Raylib.DrawText("S", (int)radarCenter.X - 3, (int)radarCenter.Y + mapR - 10, 8, Constants.Cyan);
        Raylib.DrawText("E", (int)radarCenter.X + mapR - 10, (int)radarCenter.Y - 4, 8, Constants.Cyan);
        Raylib.DrawText("O", (int)radarCenter.X - mapR + 3, (int)radarCenter.Y - 4, 8, Constants.Cyan);

        float mapScale = (mapR * 1.8f) / Constants.WorldSize;

        // Extraction beacon on radar
        Vector2 relExt = (_extractionPos - _player.Position) * mapScale;
        if (relExt.Length() < mapR - 4)
        {
            Raylib.DrawCircleV(radarCenter + relExt, 4, _extractionAvailable ? Constants.LibertyGreen : Color.Gray);
            if (_extractionActive)
            {
                float extPulse = 4f + MathF.Sin((float)Raylib.GetTime() * 6f) * 2f;
                Raylib.DrawCircleLines((int)(radarCenter.X + relExt.X), (int)(radarCenter.Y + relExt.Y), extPulse, Constants.LibertyGreen);
            }
        }

        // Objective blips on radar
        foreach (var op in _campaign.Outposts)
        {
            if (!op.Destroyed)
            {
                Vector2 rel = (op.Position - _player.Position) * mapScale;
                if (rel.Length() < mapR - 3)
                {
                    Raylib.DrawCircleV(radarCenter + rel, 3, Constants.DemocracyRed);
                }
            }
        }
        foreach (var it in _campaign.Interactables)
        {
            Vector2 rel = (it.Position - _player.Position) * mapScale;
            if (rel.Length() < mapR - 3)
            {
                Raylib.DrawCircleV(radarCenter + rel, 3, it.Completed ? Constants.LibertyGreen : Constants.Gold);
            }
        }

        // Enemy blips on radar
        foreach (var en in _enemies)
        {
            if (en.Hp <= 0) continue;
            Vector2 rel = (en.Position - _player.Position) * mapScale;
            if (rel.Length() < mapR - 3)
            {
                Color enBlipCol = en.Faction == Faction.Terminids ? Constants.OrangeHazard : Constants.DemocracyRed;
                if (en.IsHvt)
                {
                    Raylib.DrawCircleV(radarCenter + rel, 3.5f, Constants.Gold);
                    Raylib.DrawCircleLines((int)(radarCenter.X + rel.X), (int)(radarCenter.Y + rel.Y), 5f, Constants.Gold);
                }
                else
                {
                    Raylib.DrawCircleV(radarCenter + rel, 2f, enBlipCol);
                }
            }
        }

        // Player icon (center gold chevron)
        Raylib.DrawCircleV(radarCenter, 3.5f, Constants.Gold);
        Vector2 arrowTip = radarCenter + new Vector2(MathF.Cos(_player.Angle), MathF.Sin(_player.Angle)) * 8f;
        Raylib.DrawLineEx(radarCenter, arrowTip, 2f, Constants.Gold);

        // Header label
        Raylib.DrawText("RADAR TÁCTICO", (int)radarCenter.X - Raylib.MeasureText("RADAR TÁCTICO", 9) / 2, (int)radarCenter.Y + mapR + 6, 9, Constants.Cyan);

        // 5. Mission Banner (Top Center)
        int bannerW = 460;
        Rectangle bRec = new(sw / 2 - bannerW / 2, 16, bannerW, 36);
        DrawTacticalPanel(bRec, _pelicanLanded ? Constants.LibertyGreen : Constants.BorderColor, new Color(15, 23, 42, 235));

        string objText = _campaign.Mission.Title.ToUpper();
        if (_campaign.Mission.Type == MissionType.Outposts)
        {
            int remaining = _campaign.Outposts.Count(o => !o.Destroyed);
            objText += $" [{remaining} PUESTOS ACTIVOS]";
        }
        else if (_extractionActive)
        {
            objText = _pelicanLanded ? "¡PELICAN-1 EN TIERRA! ¡ABORDAD LA RAMPA!" : $"PELICAN-1 ATERRIZANDO EN: {_extractionTimer:0.0}s";
        }
        Color bannerTextCol = _pelicanLanded ? Constants.LibertyGreen : (_extractionActive ? Constants.OrangeHazard : Constants.Gold);
        Raylib.DrawText(objText, sw / 2 - Raylib.MeasureText(objText, 12) / 2, 28, 12, bannerTextCol);

        // 6. Radio Alert Banner
        if (_radioAlertTimer > 0)
        {
            int rw = Raylib.MeasureText(_radioAlert, 12) + 40;
            Rectangle rRec = new(sw / 2 - rw / 2, 58, rw, 26);
            DrawTacticalPanel(rRec, Constants.Gold, new Color(15, 23, 42, 245));
            // Radio icon indicator
            Raylib.DrawCircle((int)rRec.X + 16, (int)rRec.Y + 13, 3, Constants.DemocracyRed);
            Raylib.DrawText(_radioAlert, (int)rRec.X + 26, (int)rRec.Y + 7, 12, Constants.Gold);
        }

        // 7. FPS Counter
        if (_save.Settings.ShowFps)
        {
            Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 20, sh - 100, 11, Constants.LibertyGreen);
        }
    }

    private static void DrawHoloPlanet(Vector2 center, float radius, Faction faction)
    {
        Color primary = faction == Faction.Automatons ? Constants.Cyan : Constants.OrangeHazard;
        Color glow = faction == Faction.Automatons ? new Color((byte)56, (byte)189, (byte)248, (byte)35) : new Color((byte)249, (byte)115, (byte)22, (byte)35);

        // Atmosphere glow
        Raylib.DrawCircleV(center, radius + 8, glow);
        Raylib.DrawCircleV(center, radius, new Color(10, 16, 28, 255));
        Raylib.DrawCircleLines((int)center.X, (int)center.Y, radius, primary);

        // Latitude lines
        Raylib.DrawEllipseLines((int)center.X, (int)center.Y, (int)radius, (int)(radius * 0.35f), new Color((byte)primary.R, (byte)primary.G, (byte)primary.B, (byte)80));
        Raylib.DrawEllipseLines((int)center.X, (int)(center.Y - radius * 0.45f), (int)(radius * 0.85f), (int)(radius * 0.22f), new Color((byte)primary.R, (byte)primary.G, (byte)primary.B, (byte)50));
        Raylib.DrawEllipseLines((int)center.X, (int)(center.Y + radius * 0.45f), (int)(radius * 0.85f), (int)(radius * 0.22f), new Color((byte)primary.R, (byte)primary.G, (byte)primary.B, (byte)50));

        // Rotating longitude arc
        float t = (float)Raylib.GetTime() * 0.7f;
        float longW = radius * MathF.Abs(MathF.Cos(t));
        Raylib.DrawEllipseLines((int)center.X, (int)center.Y, (int)Math.Max(longW, 3f), (int)radius, new Color((byte)primary.R, (byte)primary.G, (byte)primary.B, (byte)110));

        // Equatorial tilt line
        Raylib.DrawLineEx(new Vector2(center.X - radius - 6, center.Y + 12), new Vector2(center.X + radius + 6, center.Y - 12), 1f, new Color((byte)primary.R, (byte)primary.G, (byte)primary.B, (byte)60));
    }

    private static void DrawWarTableMenu()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        int cx = sw / 2;

        // Header Title & Galactic Subtitle
        string title = "★ SUPER TIERRA: TABLA DE GUERRA GALÁCTICA ★";
        Raylib.DrawText("HELLDIVERS 2", cx - Raylib.MeasureText("HELLDIVERS 2", 36) / 2, 38, 36, Constants.Gold);
        Raylib.DrawText(title, cx - Raylib.MeasureText(title, 13) / 2, 80, 13, Constants.Cyan);

        // Prestige Rank Dossier Card
        Rectangle dossierRec = new(cx - 310, 108, 620, 78);
        DrawTacticalPanel(dossierRec, Constants.BorderColor, new Color(15, 23, 42, 240));

        var (mTitle, icon) = _save.GetMilitaryRank();
        string rankStr = $"{icon} {mTitle.ToUpper()} // NIVEL DE PRESTIGIO {_save.PrestigeLevel}";
        Raylib.DrawText(rankStr, (int)dossierRec.X + 20, (int)dossierRec.Y + 12, 13, Constants.Gold);

        // XP progress bar
        int xpNeeded = _save.GetXpNeededForRank();
        float xpPct = Math.Clamp((float)_save.PrestigeXp / xpNeeded, 0, 1);
        Raylib.DrawRectangle((int)dossierRec.X + 20, (int)dossierRec.Y + 34, 340, 8, new Color(30, 41, 59, 255));
        Raylib.DrawRectangle((int)dossierRec.X + 20, (int)dossierRec.Y + 34, (int)(340 * xpPct), 8, Constants.Gold);
        Raylib.DrawRectangleLines((int)dossierRec.X + 20, (int)dossierRec.Y + 34, 340, 8, Color.DarkGray);
        Raylib.DrawText($"EXP: {_save.PrestigeXp} / {xpNeeded} ({Math.Round(xpPct * 100)}%)", (int)dossierRec.X + 370, (int)dossierRec.Y + 32, 10, Color.LightGray);

        // Resources Pills
        string resText = $"☣️ {_save.Samples} MUESTRAS   🏅 {_save.Medals} MEDALLAS   ★ {_save.OperationsCompleted} VICTORIAS";
        Raylib.DrawText(resText, (int)dossierRec.X + 20, (int)dossierRec.Y + 54, 11, Constants.LibertyGreen);

        // Tactical Planet Briefing Card
        Rectangle planetCard = new(cx - 310, 200, 620, 250);
        Color factionBorder = _campaign.Planet.Faction == Faction.Automatons ? Constants.Cyan : Constants.DemocracyRed;
        DrawTacticalPanel(planetCard, factionBorder, new Color(12, 18, 32, 245));

        // Holographic planet sphere
        Vector2 planetCenter = new((int)planetCard.X + 75, (int)planetCard.Y + 85);
        DrawHoloPlanet(planetCenter, 46f, _campaign.Planet.Faction);

        // Planet text briefing on right
        int infoX = (int)planetCard.X + 145;
        string planetHdr = $"PLANETA ASIGNADO: {_campaign.Planet.Name.ToUpper()}";
        Raylib.DrawText(planetHdr, infoX, (int)planetCard.Y + 18, 17, Constants.Gold);

        string facName = _campaign.Planet.Faction == Faction.Automatons ? "LEGIÓN AUTÓMATA (CYBORGS)" : "ENJAMBRE TERMINIDO (INSECTOS)";
        Color facCol = _campaign.Planet.Faction == Faction.Automatons ? Constants.Cyan : Constants.OrangeHazard;
        Raylib.DrawText($"FACCIÓN: {facName}", infoX, (int)planetCard.Y + 44, 12, facCol);

        // Threat Rating with skulls
        string skulls = "";
        for (int s = 0; s < 9; s++) skulls += (s < _campaign.Planet.ThreatLevel ? "💀 " : "░ ");
        Raylib.DrawText($"NIVEL DE AMENAZA: DIFICULTAD {_campaign.Planet.ThreatLevel}/9  [{skulls.Trim()}]", infoX, (int)planetCard.Y + 68, 11, Constants.DemocracyRed);

        Raylib.DrawText($"BIOMA PLANETARIO: {_campaign.Planet.Biome.ToUpper()}", infoX, (int)planetCard.Y + 92, 11, Color.LightGray);

        // Objective section
        Rectangle objBox = new(infoX, (int)planetCard.Y + 118, 455, 112);
        Raylib.DrawRectangleRec(objBox, new Color(18, 26, 44, 255));
        Raylib.DrawRectangleLinesEx(objBox, 1, Constants.BorderColor);

        Raylib.DrawText("OBJETIVO DE SUPER TIERRA:", (int)objBox.X + 12, (int)objBox.Y + 12, 12, Constants.Gold);
        Raylib.DrawText(_campaign.Mission.Title.ToUpper(), (int)objBox.X + 12, (int)objBox.Y + 34, 14, Constants.LibertyGreen);
        Raylib.DrawText(_campaign.Mission.Description, (int)objBox.X + 12, (int)objBox.Y + 58, 11, Color.LightGray);

        // Buttons
        Vector2 mouse = Raylib.GetMousePosition();

        Rectangle btnPlay = new(cx - 150, 480, 300, 50);
        bool hoverPlay = Raylib.CheckCollisionPointRec(mouse, btnPlay);
        Raylib.DrawRectangleRec(btnPlay, hoverPlay ? Constants.Gold : Constants.SuperEarthYellow);
        Raylib.DrawRectangleLinesEx(btnPlay, 2, Color.White);
        string playTxt = hoverPlay ? ">>> DESPLEGAR EN COMBATE <<<" : "DESPLEGAR EN MISIÓN";
        Raylib.DrawText(playTxt, cx - Raylib.MeasureText(playTxt, 16) / 2, (int)btnPlay.Y + 17, 16, Color.Black);

        // Sub buttons row 1
        Rectangle btnArmory = new(cx - 280, 545, 175, 42);
        Rectangle btnStrat = new(cx - 90, 545, 175, 42);
        Rectangle btnHangar = new(cx + 100, 545, 175, 42);

        DrawButton(btnArmory, "ARMERÍA", Raylib.CheckCollisionPointRec(mouse, btnArmory));
        DrawButton(btnStrat, "ESTRATAGEMAS", Raylib.CheckCollisionPointRec(mouse, btnStrat));
        DrawButton(btnHangar, "HANGAR NAVE", Raylib.CheckCollisionPointRec(mouse, btnHangar));

        // Sub buttons row 2
        Rectangle btnReroll = new(cx - 280, 600, 175, 38);
        Rectangle btnBestiary = new(cx - 90, 600, 175, 38);
        Rectangle btnSettings = new(cx + 100, 600, 175, 38);
        DrawButton(btnReroll, "CAMBIAR PLANETA", Raylib.CheckCollisionPointRec(mouse, btnReroll));
        DrawButton(btnBestiary, "BESTIARIO", Raylib.CheckCollisionPointRec(mouse, btnBestiary));
        DrawButton(btnSettings, "AJUSTES", Raylib.CheckCollisionPointRec(mouse, btnSettings));
    }

    private static void DrawButton(Rectangle rec, string text, bool hover)
    {
        Raylib.DrawRectangleRec(rec, hover ? new Color(28, 42, 68, 255) : new Color(15, 23, 42, 240));
        Raylib.DrawRectangleLinesEx(rec, 1, hover ? Constants.Gold : Constants.BorderColor);

        // Corner ticks on hover
        if (hover)
        {
            int x = (int)rec.X, y = (int)rec.Y, w = (int)rec.Width, h = (int)rec.Height;
            Raylib.DrawLine(x, y, x + 4, y, Constants.Gold);
            Raylib.DrawLine(x, y, x, y + 4, Constants.Gold);
            Raylib.DrawLine(x + w, y + h, x + w - 4, y + h, Constants.Gold);
            Raylib.DrawLine(x + w, y + h, x + w, y + h - 4, Constants.Gold);
        }

        Raylib.DrawText(text, (int)rec.X + (int)rec.Width / 2 - Raylib.MeasureText(text, 12) / 2, (int)rec.Y + 14, 12, hover ? Constants.Gold : Color.White);
    }

    private static void UpdateArmory()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) { _state = GameState.Menu; return; }

        Vector2 mouse = Raylib.GetMousePosition();
        int sw = Raylib.GetScreenWidth();
        int cx = sw / 2;

        // Back button
        Rectangle btnBack = new(cx - 100, 640, 200, 42);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, btnBack))
        {
            _state = GameState.Menu;
            return;
        }

        // Weapon cards selection
        var weapons = WeaponRegistry.Weapons.Values.ToList();
        for (int i = 0; i < weapons.Count; i++)
        {
            int col = i % 2;
            int row = i / 2;
            Rectangle cardRec = new(cx - 360 + col * 370, 110 + row * 125, 350, 115);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, cardRec))
            {
                _save.EquippedWeapon = weapons[i].Key;
                _save.Save();
                _player.Reset(_save);
                _sound.PlayDeflect();
            }
        }
    }

    private static void DrawStatBar(int x, int y, int width, int height, float pct, Color fillCol, string label, string val)
    {
        Raylib.DrawText(label, x, y, 9, Color.LightGray);
        int barX = x + 58;
        int barW = width - 105;
        Raylib.DrawRectangle(barX, y + 1, barW, height, new Color(25, 33, 48, 255));
        Raylib.DrawRectangle(barX, y + 1, (int)(barW * Math.Clamp(pct, 0f, 1f)), height, fillCol);
        Raylib.DrawRectangleLines(barX, y + 1, barW, height, new Color(50, 65, 85, 255));
        Raylib.DrawText(val, barX + barW + 6, y, 9, Color.White);
    }

    private static void DrawArmory()
    {
        int sw = Raylib.GetScreenWidth();
        int cx = sw / 2;

        Raylib.DrawText("ARMERÍA DE LA SUPER TIERRA", cx - Raylib.MeasureText("ARMERÍA DE LA SUPER TIERRA", 26) / 2, 36, 26, Constants.Gold);
        string sub = "SELECCIÓN DE ARMA REGLAMENTARIA Y BALÍSTICA TÁCTICA";
        Raylib.DrawText(sub, cx - Raylib.MeasureText(sub, 12) / 2, 70, 12, Constants.Cyan);

        var weapons = WeaponRegistry.Weapons.Values.ToList();
        Vector2 mouse = Raylib.GetMousePosition();

        for (int i = 0; i < weapons.Count; i++)
        {
            var w = weapons[i];
            int col = i % 2;
            int row = i / 2;
            Rectangle cardRec = new(cx - 360 + col * 370, 100 + row * 128, 350, 118);

            bool isEquipped = _save.EquippedWeapon == w.Key;
            bool hover = Raylib.CheckCollisionPointRec(mouse, cardRec);

            Color cardBorder = isEquipped ? Constants.LibertyGreen : (hover ? Constants.Gold : Constants.BorderColor);
            DrawTacticalPanel(cardRec, cardBorder, hover ? new Color(22, 33, 52, 255) : new Color(13, 20, 36, 245));

            // Weapon Title & Icon
            Raylib.DrawText(w.Name.ToUpper(), (int)cardRec.X + 14, (int)cardRec.Y + 10, 13, isEquipped ? Constants.LibertyGreen : Constants.Gold);

            if (isEquipped)
            {
                Raylib.DrawText("[✓ EQUIPADA]", (int)cardRec.X + (int)cardRec.Width - 92, (int)cardRec.Y + 10, 11, Constants.LibertyGreen);
            }
            else if (hover)
            {
                Raylib.DrawText("[CLIC EQUIPAR]", (int)cardRec.X + (int)cardRec.Width - 98, (int)cardRec.Y + 10, 10, Constants.Gold);
            }

            // Stat Bars
            int statX = (int)cardRec.X + 14;
            int statY = (int)cardRec.Y + 28;
            DrawStatBar(statX, statY, 320, 6, w.Damage / 275f, Constants.DemocracyRed, "DAÑO", $"{(int)w.Damage}");
            DrawStatBar(statX, statY + 14, 320, 6, w.Rpm / 750f, Constants.OrangeHazard, "CADENCIA", $"{w.Rpm} RPM");
            DrawStatBar(statX, statY + 28, 320, 6, (float)w.Ammo / 75f, Constants.Gold, "CARGADOR", $"{w.Ammo} DISP");

            // Special trait pill
            Rectangle traitPill = new(statX, statY + 46, Raylib.MeasureText(w.SpecialTrait, 10) + 14, 16);
            Raylib.DrawRectangleRec(traitPill, new Color(20, 40, 65, 255));
            Raylib.DrawRectangleLinesEx(traitPill, 1, Constants.Cyan);
            Raylib.DrawText(w.SpecialTrait, (int)traitPill.X + 7, (int)traitPill.Y + 3, 10, Constants.Cyan);

            // Description
            Raylib.DrawText(w.Description, statX, statY + 68, 9, Color.Gray);
        }

        Rectangle btnBack = new(cx - 100, 640, 200, 42);
        DrawButton(btnBack, "VOLVER AL PUENTE", Raylib.CheckCollisionPointRec(mouse, btnBack));
    }

    private static void UpdateStratagems()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) { _state = GameState.Menu; return; }

        Vector2 mouse = Raylib.GetMousePosition();
        int cx = Raylib.GetScreenWidth() / 2;

        // Slot buttons (1, 2, 3)
        for (int i = 0; i < 3; i++)
        {
            Rectangle slotRec = new(cx - 180 + i * 130, 110, 100, 50);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, slotRec))
            {
                _selectedStratagemSlot = i;
            }
        }

        // Available stratagem cards
        var all = StratagemRegistry.All.Values.ToList();
        for (int i = 0; i < all.Count; i++)
        {
            int col = i % 2;
            int row = i / 2;
            Rectangle stratRec = new(cx - 360 + col * 370, 190 + row * 95, 350, 85);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, stratRec))
            {
                _save.EquippedStratagems[_selectedStratagemSlot] = all[i].Id;
                _save.Save();
                _player.Reset(_save);
                _sound.PlayDeflect();
            }
        }

        Rectangle btnBack = new(cx - 100, 630, 200, 42);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, btnBack))
        {
            _state = GameState.Menu;
        }
    }

    private static void DrawStratagems()
    {
        int sw = Raylib.GetScreenWidth();
        int cx = sw / 2;

        Raylib.DrawText("ESTRATAGEMAS DE SUPER TIERRA", cx - Raylib.MeasureText("ESTRATAGEMAS DE SUPER TIERRA", 26) / 2, 36, 26, Constants.Gold);
        string sub = "CONFIGURACIÓN DEL PROTOCOLO DE TRANSMISIÓN ORBITAL (CARGADOR TÁCTICO)";
        Raylib.DrawText(sub, cx - Raylib.MeasureText(sub, 12) / 2, 70, 12, Constants.Cyan);

        Vector2 mouse = Raylib.GetMousePosition();

        // 3 Slots selector with glowing active outline
        for (int i = 0; i < 3; i++)
        {
            Rectangle slotRec = new(cx - 200 + i * 140, 100, 120, 56);
            bool isCurrent = _selectedStratagemSlot == i;
            string eqId = i < _save.EquippedStratagems.Count ? _save.EquippedStratagems[i] : "";
            StratagemRegistry.All.TryGetValue(eqId, out var def);

            Color sBorder = isCurrent ? Constants.Gold : Constants.BorderColor;
            DrawTacticalPanel(slotRec, sBorder, isCurrent ? new Color(28, 44, 76, 255) : new Color(15, 23, 42, 240));

            Raylib.DrawText($"RANURA {i + 1}", (int)slotRec.X + 16, (int)slotRec.Y + 8, 11, isCurrent ? Constants.Gold : Color.LightGray);
            Raylib.DrawText(def != null ? def.Icon : "[---]", (int)slotRec.X + 16, (int)slotRec.Y + 24, 13, Color.White);

            if (isCurrent)
            {
                Raylib.DrawText("▼ ASIGNANDO", (int)slotRec.X + 16, (int)slotRec.Y + 40, 9, Constants.Gold);
            }
        }

        // Available stratagems
        var all = StratagemRegistry.All.Values.ToList();
        for (int i = 0; i < all.Count; i++)
        {
            var def = all[i];
            int col = i % 2;
            int row = i / 2;
            Rectangle stratRec = new(cx - 360 + col * 370, 175 + row * 105, 350, 95);

            bool isEquippedInAny = _save.EquippedStratagems.Contains(def.Id);
            bool hover = Raylib.CheckCollisionPointRec(mouse, stratRec);

            Color cardBorder = isEquippedInAny ? Constants.LibertyGreen : (hover ? Constants.Gold : Constants.BorderColor);
            DrawTacticalPanel(stratRec, cardBorder, hover ? new Color(22, 33, 52, 255) : new Color(13, 20, 36, 245));

            Raylib.DrawText($"{def.Icon} {def.Name.ToUpper()}", (int)stratRec.X + 14, (int)stratRec.Y + 12, 13, isEquippedInAny ? Constants.LibertyGreen : Constants.Gold);

            // D-Pad call-in arrow sequence
            string codeArrows = GetStratagemCode(def.Id);
            Raylib.DrawText(codeArrows, (int)stratRec.X + 14, (int)stratRec.Y + 30, 12, Constants.Gold);

            // Category & Cooldown
            string catStr = $"{def.Category.ToUpper()} // REC: {def.Cooldown}s";
            Raylib.DrawText(catStr, (int)stratRec.X + 14, (int)stratRec.Y + 48, 10, Constants.Cyan);

            // Description
            Raylib.DrawText(def.Description, (int)stratRec.X + 14, (int)stratRec.Y + 66, 9, Color.Gray);

            if (isEquippedInAny)
            {
                Raylib.DrawText("[EN CARGADOR]", (int)stratRec.X + (int)stratRec.Width - 95, (int)stratRec.Y + 12, 10, Constants.LibertyGreen);
            }
            else if (hover)
            {
                Raylib.DrawText("[CLIC ASIGNAR]", (int)stratRec.X + (int)stratRec.Width - 98, (int)stratRec.Y + 12, 10, Constants.Gold);
            }
        }

        Rectangle btnBack = new(cx - 100, 630, 200, 42);
        DrawButton(btnBack, "VOLVER AL PUENTE", Raylib.CheckCollisionPointRec(mouse, btnBack));
    }

    private static void UpdateHangar()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) { _state = GameState.Menu; return; }

        Vector2 mouse = Raylib.GetMousePosition();
        int cx = Raylib.GetScreenWidth() / 2;

        // Upgrade 1: Max HP
        Rectangle b1 = new(cx + 140, 150, 110, 36);
        int cost1 = (_save.Hangar.MaxHpLevel + 1) * 5;
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, b1) && _save.Samples >= cost1)
        {
            _save.Samples -= cost1;
            _save.Hangar.MaxHpLevel++;
            _save.Save();
            _player.Reset(_save);
            _sound.PlayVictory();
        }

        // Upgrade 2: Speed
        Rectangle b2 = new(cx + 140, 230, 110, 36);
        int cost2 = (_save.Hangar.SpeedLevel + 1) * 5;
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, b2) && _save.Samples >= cost2)
        {
            _save.Samples -= cost2;
            _save.Hangar.SpeedLevel++;
            _save.Save();
            _player.Reset(_save);
            _sound.PlayVictory();
        }

        // Upgrade 3: Damage
        Rectangle b3 = new(cx + 140, 310, 110, 36);
        int cost3 = (_save.Hangar.DamageLevel + 1) * 8;
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, b3) && _save.Samples >= cost3)
        {
            _save.Samples -= cost3;
            _save.Hangar.DamageLevel++;
            _save.Save();
            _player.Reset(_save);
            _sound.PlayVictory();
        }

        // Upgrade 4: Shield Generator
        Rectangle b4 = new(cx + 140, 390, 110, 36);
        int cost4 = (_save.Hangar.ShieldLevel + 1) * 10;
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, b4) && _save.Samples >= cost4)
        {
            _save.Samples -= cost4;
            _save.Hangar.ShieldLevel++;
            _save.Save();
            _player.Reset(_save);
            _sound.PlayVictory();
        }

        Rectangle btnBack = new(cx - 100, 560, 200, 42);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, btnBack))
        {
            _state = GameState.Menu;
        }
    }

    private static void DrawHangar()
    {
        int sw = Raylib.GetScreenWidth();
        int cx = sw / 2;

        Raylib.DrawText("HANGAR Y MÓDULOS DE LA NAVE", cx - Raylib.MeasureText("HANGAR Y MÓDULOS DE LA NAVE", 26) / 2, 36, 26, Constants.Gold);
        string sub = $"MUESTRAS DISPONIBLES EN DESTRUCTOR: {_save.Samples} ☣️";
        Raylib.DrawText(sub, cx - Raylib.MeasureText(sub, 14) / 2, 70, 14, Constants.LibertyGreen);

        Vector2 mouse = Raylib.GetMousePosition();

        DrawHangarUpgradeCard(cx, 140, "REFUERZO DE ARMADURA DE GRAFENO", "+20 HP Máxima de combate por nivel", _save.Hangar.MaxHpLevel, (_save.Hangar.MaxHpLevel + 1) * 5, mouse);
        DrawHangarUpgradeCard(cx, 220, "SERVOMOTORES DE MOVILIDAD TÁCTICA", "+7% Velocidad de carrera por nivel", _save.Hangar.SpeedLevel, (_save.Hangar.SpeedLevel + 1) * 5, mouse);
        DrawHangarUpgradeCard(cx, 300, "CALIBRACIÓN BALÍSTICA DE COMBATE", "+10% Daño de Armas de fuego por nivel", _save.Hangar.DamageLevel, (_save.Hangar.DamageLevel + 1) * 8, mouse);
        DrawHangarUpgradeCard(cx, 380, "GENERADOR DE ESCUDO RELÉ SH-20", "+25 Capacidad de Escudo por nivel", _save.Hangar.ShieldLevel, (_save.Hangar.ShieldLevel + 1) * 10, mouse);

        Rectangle btnBack = new(cx - 100, 560, 200, 42);
        DrawButton(btnBack, "VOLVER AL PUENTE", Raylib.CheckCollisionPointRec(mouse, btnBack));
    }

    private static void DrawHangarUpgradeCard(int cx, int y, string title, string desc, int level, int cost, Vector2 mouse)
    {
        Rectangle card = new(cx - 280, y, 560, 65);
        DrawTacticalPanel(card, Constants.BorderColor, new Color(13, 20, 36, 245));

        Raylib.DrawText(title, (int)card.X + 16, (int)card.Y + 12, 13, Constants.Gold);
        Raylib.DrawText(desc, (int)card.X + 16, (int)card.Y + 28, 10, Color.LightGray);

        // 5 visual tier pips
        int pipX = (int)card.X + 16;
        int pipY = (int)card.Y + 45;
        for (int p = 0; p < 5; p++)
        {
            bool filled = p < level;
            Raylib.DrawRectangle(pipX + p * 16, pipY, 12, 6, filled ? Constants.LibertyGreen : new Color(30, 42, 60, 255));
            Raylib.DrawRectangleLines(pipX + p * 16, pipY, 12, 6, filled ? Constants.LibertyGreen : Color.DarkGray);
        }
        Raylib.DrawText($"NV. {level} / 5", pipX + 90, pipY - 2, 9, Constants.Cyan);

        Rectangle btnBuy = new(cx + 140, y + 14, 115, 36);
        bool canAfford = _save.Samples >= cost;
        bool hover = Raylib.CheckCollisionPointRec(mouse, btnBuy);

        Color btnBg = hover && canAfford ? Constants.LibertyGreen : (canAfford ? new Color(20, 90, 46, 255) : new Color(25, 33, 48, 255));
        Raylib.DrawRectangleRec(btnBuy, btnBg);
        Raylib.DrawRectangleLinesEx(btnBuy, 1, canAfford ? Constants.LibertyGreen : Color.DarkGray);

        string buyTxt = canAfford ? $"MEJORAR ({cost} ☣️)" : $"REQ {cost} ☣️";
        Raylib.DrawText(buyTxt, (int)btnBuy.X + (int)btnBuy.Width / 2 - Raylib.MeasureText(buyTxt, 11) / 2, (int)btnBuy.Y + 12, 11, canAfford ? Color.White : Color.Gray);
    }

    private static void DrawPauseOverlay()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 195));

        int cx = sw / 2;
        int cy = sh / 2;

        Rectangle pauseCard = new(cx - 160, cy - 130, 320, 260);
        DrawTacticalPanel(pauseCard, Constants.Gold, new Color(12, 18, 30, 250));

        Raylib.DrawText("MISIÓN EN PAUSA", cx - Raylib.MeasureText("MISIÓN EN PAUSA", 22) / 2, cy - 105, 22, Constants.Gold);
        Raylib.DrawText("SISTEMAS EN ESPERA TÁCTICA", cx - Raylib.MeasureText("SISTEMAS EN ESPERA TÁCTICA", 11) / 2, cy - 75, 11, Constants.Cyan);

        Vector2 mouse = Raylib.GetMousePosition();

        Rectangle btnResume = new(cx - 130, cy - 35, 260, 40);
        Rectangle btnSettings = new(cx - 130, cy + 15, 260, 40);
        Rectangle btnMenu = new(cx - 130, cy + 65, 260, 40);

        DrawButton(btnResume, "REANUDAR COMBATE", Raylib.CheckCollisionPointRec(mouse, btnResume));
        DrawButton(btnSettings, "AJUSTES Y VOLUMEN", Raylib.CheckCollisionPointRec(mouse, btnSettings));
        DrawButton(btnMenu, "ABANDONAR AL DESTRUCTOR", Raylib.CheckCollisionPointRec(mouse, btnMenu));

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(mouse, btnResume))
            {
                _state = GameState.Playing;
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnSettings))
            {
                _previousState = GameState.Paused;
                _state = GameState.Settings;
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnMenu))
            {
                _state = GameState.Menu;
            }
        }
    }

    private static void UpdateSettings()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _state = _previousState;
            return;
        }

        Vector2 mouse = Raylib.GetMousePosition();
        int cx = Raylib.GetScreenWidth() / 2;

        // 1. Volume buttons
        Rectangle btnVolMinus = new(cx + 80, 140, 36, 32);
        Rectangle btnVolPlus = new(cx + 195, 140, 36, 32);
        Rectangle btnMute = new(cx + 240, 140, 85, 32);

        // 2. Screen Shake toggle
        Rectangle btnShake = new(cx + 100, 215, 180, 34);

        // 3. Damage Numbers toggle
        Rectangle btnDmgNum = new(cx + 100, 290, 180, 34);

        // 4. Aim Assist toggle
        Rectangle btnAim = new(cx + 100, 365, 180, 34);

        // 5. FPS Counter toggle
        Rectangle btnFps = new(cx + 100, 440, 180, 34);

        // 6. Fullscreen toggle
        Rectangle btnFull = new(cx + 100, 515, 180, 34);

        // Back button
        Rectangle btnBack = new(cx - 100, 610, 200, 42);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(mouse, btnVolMinus))
            {
                _save.Settings.MasterVolume = MathF.Max(0f, _save.Settings.MasterVolume - 0.10f);
                _sound.Volume = _save.Settings.MasterVolume;
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnVolPlus))
            {
                _save.Settings.MasterVolume = MathF.Min(1f, _save.Settings.MasterVolume + 0.10f);
                _sound.Volume = _save.Settings.MasterVolume;
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnMute))
            {
                if (_save.Settings.MasterVolume > 0.01f)
                    _save.Settings.MasterVolume = 0f;
                else
                    _save.Settings.MasterVolume = 0.70f;
                _sound.Volume = _save.Settings.MasterVolume;
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnShake))
            {
                _save.Settings.ScreenShake = !_save.Settings.ScreenShake;
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnDmgNum))
            {
                _save.Settings.ShowDamageNumbers = !_save.Settings.ShowDamageNumbers;
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnAim))
            {
                _save.Settings.AutoAimAssist = !_save.Settings.AutoAimAssist;
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnFps))
            {
                _save.Settings.ShowFps = !_save.Settings.ShowFps;
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnFull))
            {
                Raylib.ToggleFullscreen();
                _save.Settings.Fullscreen = Raylib.IsWindowFullscreen();
                _save.Save();
                _sound.PlayDeflect();
            }
            else if (Raylib.CheckCollisionPointRec(mouse, btnBack))
            {
                _state = _previousState;
                _sound.PlayDeflect();
            }
        }
    }

    private static void DrawSettings()
    {
        int sw = Raylib.GetScreenWidth();
        int cx = sw / 2;

        Raylib.DrawText("AJUSTES Y CONFIGURACIÓN", cx - Raylib.MeasureText("AJUSTES Y CONFIGURACIÓN", 26) / 2, 36, 26, Constants.Gold);
        string sub = "SISTEMAS DE CONTROL, TELEMETRÍA Y CALIBRACIÓN DE COMBATE";
        Raylib.DrawText(sub, cx - Raylib.MeasureText(sub, 12) / 2, 70, 12, Constants.Cyan);

        Vector2 mouse = Raylib.GetMousePosition();

        // 1. Volume Card
        Rectangle c1 = new(cx - 300, 115, 600, 68);
        DrawTacticalPanel(c1, Constants.BorderColor, new Color(13, 20, 36, 245));
        Raylib.DrawText("VOLUMEN GENERAL (AUDIO MASTER)", (int)c1.X + 20, (int)c1.Y + 16, 13, Constants.Gold);

        int volPercent = (int)Math.Round(_save.Settings.MasterVolume * 100);
        Raylib.DrawText($"POTENCIA DE SALIDA: {volPercent}%", (int)c1.X + 20, (int)c1.Y + 38, 11, Color.LightGray);

        Rectangle btnVolMinus = new(cx + 80, 133, 36, 32);
        Rectangle btnVolPlus = new(cx + 195, 133, 36, 32);
        Rectangle btnMute = new(cx + 240, 133, 85, 32);

        DrawButton(btnVolMinus, " - ", Raylib.CheckCollisionPointRec(mouse, btnVolMinus));
        // Volume meter bar
        Raylib.DrawRectangle(cx + 122, 139, 68, 20, new Color(30, 41, 59, 255));
        Raylib.DrawRectangle(cx + 122, 139, (int)(68 * _save.Settings.MasterVolume), 20, Constants.Gold);
        Raylib.DrawRectangleLines(cx + 122, 139, 68, 20, Color.DarkGray);
        DrawButton(btnVolPlus, " + ", Raylib.CheckCollisionPointRec(mouse, btnVolPlus));
        DrawButton(btnMute, volPercent == 0 ? "ACTIVAR" : "MUTEAR", Raylib.CheckCollisionPointRec(mouse, btnMute));

        // 2. Screen Shake
        DrawSettingsToggleCard(cx, 195, "TEMBLOR DE PANTALLA", "Vibración de cámara en detonaciones de 500kg y pisadas pesadas.", _save.Settings.ScreenShake ? "ACTIVADO" : "DESACTIVADO", _save.Settings.ScreenShake, mouse);

        // 3. Damage Numbers
        DrawSettingsToggleCard(cx, 275, "NÚMEROS DE DAÑO FLOTANTES", "Muestra cifras de daño, críticos e indicadores de blindaje rebotado.", _save.Settings.ShowDamageNumbers ? "ACTIVADO" : "DESACTIVADO", _save.Settings.ShowDamageNumbers, mouse);

        // 4. Aim Assist
        DrawSettingsToggleCard(cx, 355, "MODO DE PUNTERÍA", "Auto-apuntado táctico a la mayor amenaza vs disparo manual al cursor.", _save.Settings.AutoAimAssist ? "AUTO-PRIORIDAD" : "MANUAL AL CURSOR", _save.Settings.AutoAimAssist, mouse);

        // 5. FPS Display
        DrawSettingsToggleCard(cx, 435, "CONTADOR DE FPS (144 HZ)", "Muestra la tasa de cuadros por segundo en tiempo real en la interfaz.", _save.Settings.ShowFps ? "MOSTRAR" : "OCULTAR", _save.Settings.ShowFps, mouse);

        // 6. Fullscreen
        bool isFull = Raylib.IsWindowFullscreen();
        DrawSettingsToggleCard(cx, 515, "PANTALLA COMPLETA", "Alterna entre pantalla completa nativa y ventana estándar de 1280x720.", isFull ? "PANTALLA COMPLETA" : "MODO VENTANA", isFull, mouse);

        // Back button
        Rectangle btnBack = new(cx - 100, 615, 200, 42);
        DrawButton(btnBack, "VOLVER", Raylib.CheckCollisionPointRec(mouse, btnBack));
    }

    private static void DrawSettingsToggleCard(int cx, int y, string title, string desc, string stateText, bool active, Vector2 mouse)
    {
        Rectangle card = new(cx - 300, y, 600, 68);
        DrawTacticalPanel(card, Constants.BorderColor, new Color(13, 20, 36, 245));

        Raylib.DrawText(title, (int)card.X + 20, (int)card.Y + 14, 13, Constants.Gold);
        Raylib.DrawText(desc, (int)card.X + 20, (int)card.Y + 36, 10, Color.LightGray);

        Rectangle btn = new(cx + 100, y + 16, 180, 36);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        Color bg = hover ? (active ? Constants.LibertyGreen : new Color(51, 65, 85, 255)) : (active ? new Color(20, 90, 46, 255) : new Color(25, 33, 48, 255));
        Raylib.DrawRectangleRec(btn, bg);
        Raylib.DrawRectangleLinesEx(btn, 1, active ? Constants.LibertyGreen : Color.DarkGray);
        Raylib.DrawText(stateText, (int)btn.X + (int)btn.Width / 2 - Raylib.MeasureText(stateText, 11) / 2, (int)btn.Y + 12, 11, active ? Color.White : Color.LightGray);
    }

    private static void DrawVictoryScreen()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        int cx = sw / 2;

        Raylib.DrawRectangle(0, 0, sw, sh, new Color(6, 13, 22, 245));

        // Victory Laurel Stars
        string stars = "★  ★  ★";
        Raylib.DrawText(stars, cx - Raylib.MeasureText(stars, 28) / 2, 85, 28, Constants.Gold);

        Raylib.DrawText("¡MISIÓN CUMPLIDA!", cx - Raylib.MeasureText("¡MISIÓN CUMPLIDA!", 38) / 2, 125, 38, Constants.Gold);
        Raylib.DrawText("LA DEMOCRACIA GESTIONADA HA PREVALECIDO EN EL PLANETA", cx - Raylib.MeasureText("LA DEMOCRACIA GESTIONADA HA PREVALECIDO EN EL PLANETA", 14) / 2, 175, 14, Constants.LibertyGreen);

        // Rewards box
        Rectangle rBox = new(cx - 260, 220, 520, 220);
        DrawTacticalPanel(rBox, Constants.Gold, new Color(13, 20, 36, 255));

        Raylib.DrawText("INFORME DE RECOMPENSAS DE SUPER TIERRA:", (int)rBox.X + 24, (int)rBox.Y + 20, 15, Constants.Gold);
        Raylib.DrawText("★ + 120 PUNTOS DE EXPERIENCIA MILITAR", (int)rBox.X + 24, (int)rBox.Y + 56, 13, Color.White);
        Raylib.DrawText("☣️ + 4 MUESTRAS EXTRAÍDAS EN EXTRACCIÓN", (int)rBox.X + 24, (int)rBox.Y + 86, 13, Constants.LibertyGreen);
        Raylib.DrawText("🏅 + 2 MEDALLAS DE HONOR DEMOCRÁTICO", (int)rBox.X + 24, (int)rBox.Y + 116, 13, Constants.Cyan);

        // Planetary Liberation meter
        Raylib.DrawText("PROGRESO DE LIBERACIÓN PLANETARIA: +0.0042%", (int)rBox.X + 24, (int)rBox.Y + 154, 11, Constants.Gold);
        Raylib.DrawRectangle((int)rBox.X + 24, (int)rBox.Y + 176, 472, 12, new Color(30, 41, 59, 255));
        Raylib.DrawRectangle((int)rBox.X + 24, (int)rBox.Y + 176, 380, 12, Constants.LibertyGreen);
        Raylib.DrawRectangleLines((int)rBox.X + 24, (int)rBox.Y + 176, 472, 12, Color.DarkGray);

        string contTxt = "PRESIONA [ESPACIO] O [ENTER] PARA REGRESAR AL DESTRUCTOR";
        Raylib.DrawText(contTxt, cx - Raylib.MeasureText(contTxt, 13) / 2, 480, 13, Constants.Gold);
    }

    private static void DrawGameOverScreen()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        int cx = sw / 2;

        Raylib.DrawRectangle(0, 0, sw, sh, new Color(18, 6, 8, 250));

        Raylib.DrawText("HELLDIVER CAÍDO EN COMBATE", cx - Raylib.MeasureText("HELLDIVER CAÍDO EN COMBATE", 36) / 2, 140, 36, Constants.DemocracyRed);
        Raylib.DrawText("MÁRTIR HEROICO DE LA DEMOCRACIA GESTIONADA", cx - Raylib.MeasureText("MÁRTIR HEROICO DE LA DEMOCRACIA GESTIONADA", 14) / 2, 195, 14, Color.LightGray);

        Rectangle goBox = new(cx - 240, 240, 480, 120);
        DrawTacticalPanel(goBox, Constants.DemocracyRed, new Color(24, 10, 14, 255));

        string epitaph = "TU SACRIFICIO POR LA SUPER TIERRA NUNCA SERÁ OLVIDADO.";
        Raylib.DrawText(epitaph, (int)goBox.X + (int)goBox.Width / 2 - Raylib.MeasureText(epitaph, 12) / 2, (int)goBox.Y + 35, 12, Color.White);
        string subEpitaph = "OTRO HELLDIVER ESTÁ LISTO EN LA CÁMARA CRIOGÉNICA.";
        Raylib.DrawText(subEpitaph, (int)goBox.X + (int)goBox.Width / 2 - Raylib.MeasureText(subEpitaph, 11) / 2, (int)goBox.Y + 65, 11, Constants.OrangeHazard);

        string respawnTxt = "PRESIONA [ESPACIO] O [ENTER] PARA DESPLEGAR UN NUEVO REFUERZO";
        Raylib.DrawText(respawnTxt, cx - Raylib.MeasureText(respawnTxt, 13) / 2, 420, 13, Constants.Gold);
    }

    private static void UpdateBestiary()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _state = GameState.Menu;
            _sound.PlayDeflect();
            return;
        }

        Vector2 mouse = Raylib.GetMousePosition();
        int cx = Raylib.GetScreenWidth() / 2;

        // Back button
        Rectangle btnBack = new(cx - 130, 660, 260, 36);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, btnBack))
        {
            _state = GameState.Menu;
            _sound.PlayDeflect();
            return;
        }

        // Card clicks
        var entries = BestiaryRegistry.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            Rectangle cardRec = GetBestiaryCardRec(i);
            if (Raylib.CheckCollisionPointRec(mouse, cardRec))
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left) && _selectedBestiaryIndex != i)
                {
                    _selectedBestiaryIndex = i;
                    _sound.PlayDeflect();
                }
            }
        }
    }

    private static Rectangle GetBestiaryCardRec(int i)
    {
        if (i < 12)
        {
            // Terminids (4 rows x 3 columns)
            int col = i % 3;
            int row = i / 3;
            return new Rectangle(35 + col * 196, 92 + row * 63, 192, 60);
        }
        else if (i < 24)
        {
            // Automatons (4 rows x 3 columns)
            int j = i - 12;
            int col = j % 3;
            int row = j / 3;
            return new Rectangle(660 + col * 196, 92 + row * 63, 192, 60);
        }
        return new Rectangle(0, 0, 0, 0);
    }

    private static void DrawHazardStripes(Rectangle rec, Color stripeColor, Color bgColor)
    {
        Raylib.DrawRectangleRec(rec, bgColor);
        Raylib.BeginScissorMode((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height);
        float step = 14f;
        for (float x = -rec.Height; x < rec.Width + rec.Height; x += step * 2f)
        {
            Vector2 p1 = new(rec.X + x, rec.Y + rec.Height);
            Vector2 p2 = new(rec.X + x + step, rec.Y + rec.Height);
            Vector2 p3 = new(rec.X + x + step + rec.Height, rec.Y);
            Vector2 p4 = new(rec.X + x + rec.Height, rec.Y);
            Raylib.DrawTriangle(p1, p2, p3, stripeColor);
            Raylib.DrawTriangle(p1, p3, p4, stripeColor);
        }
        Raylib.EndScissorMode();
        Raylib.DrawRectangleLinesEx(rec, 1.5f, stripeColor);
    }

    private static void DrawTextWrapped(string text, int x, int y, int width, int fontSize, Color color, int lineSpacing = 4)
    {
        string[] words = text.Split(' ');
        string currentLine = "";
        int curY = y;

        foreach (var word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            if (Raylib.MeasureText(testLine, fontSize) > width && !string.IsNullOrEmpty(currentLine))
            {
                Raylib.DrawText(currentLine, x, curY, fontSize, color);
                curY += fontSize + lineSpacing;
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }
        if (!string.IsNullOrEmpty(currentLine))
        {
            Raylib.DrawText(currentLine, x, curY, fontSize, color);
        }
    }

    private static void DrawBestiary()
    {
        int sw = Raylib.GetScreenWidth();
        int cx = sw / 2;
        Vector2 mouse = Raylib.GetMousePosition();
        var entries = BestiaryRegistry.Entries;

        // 1. Header with Hazard Warning Stripes
        Rectangle leftHazard = new(35, 12, 190, 48);
        DrawHazardStripes(leftHazard, Constants.OrangeHazard, new Color(18, 12, 8, 255));
        Raylib.DrawRectangle((int)leftHazard.X + 12, (int)leftHazard.Y + 12, (int)leftHazard.Width - 24, 24, new Color(10, 15, 24, 230));
        string termTag = "/// TERMINIDS ///";
        Raylib.DrawText(termTag, (int)leftHazard.X + (int)leftHazard.Width / 2 - Raylib.MeasureText(termTag, 11) / 2, (int)leftHazard.Y + 17, 11, Constants.OrangeHazard);

        Rectangle rightHazard = new(1055, 12, 190, 48);
        DrawHazardStripes(rightHazard, Constants.DemocracyRed, new Color(20, 8, 10, 255));
        Raylib.DrawRectangle((int)rightHazard.X + 12, (int)rightHazard.Y + 12, (int)rightHazard.Width - 24, 24, new Color(10, 15, 24, 230));
        string autoTag = "/// AUTOMATONS ///";
        Raylib.DrawText(autoTag, (int)rightHazard.X + (int)rightHazard.Width / 2 - Raylib.MeasureText(autoTag, 11) / 2, (int)rightHazard.Y + 17, 11, Constants.DemocracyRed);

        // Center Title & Subtitles
        string mainTitle = "HELLDIVERS II - ENEMY BESTIARY // ENEMIES OF DEMOCRACY";
        Raylib.DrawText(mainTitle, cx - Raylib.MeasureText(mainTitle, 19) / 2, 16, 19, Constants.Gold);
        string sub1 = "SUPER EARTH ARMED FORCES // ARCHIVO TÁCTICO DE INTELIGENCIA MILITAR";
        Raylib.DrawText(sub1, cx - Raylib.MeasureText(sub1, 11) / 2, 39, 11, Constants.Cyan);
        string sub2 = "[ REGISTROS DESCLASIFICADOS // VULNERABILIDADES BALÍSTICAS AUTORIZADAS ]";
        Raylib.DrawText(sub2, cx - Raylib.MeasureText(sub2, 9) / 2, 54, 9, Color.LightGray);

        // 2. Faction Column Banners
        Rectangle termHdrRec = new(35, 66, 584, 24);
        DrawTacticalPanel(termHdrRec, Constants.OrangeHazard, new Color(26, 18, 10, 250));
        Raylib.DrawText("BIOLOGÍA TERMINIDA // ENJAMBRE BIO-MUTANTE", (int)termHdrRec.X + 14, (int)termHdrRec.Y + 5, 11, Constants.OrangeHazard);
        Raylib.DrawText("[ESPECIES: 12 REGISTRADAS]", (int)termHdrRec.X + (int)termHdrRec.Width - 175, (int)termHdrRec.Y + 6, 9, Constants.Cyan);

        Rectangle autoHdrRec = new(660, 66, 584, 24);
        DrawTacticalPanel(autoHdrRec, Constants.DemocracyRed, new Color(28, 10, 14, 250));
        Raylib.DrawText("AMENAZA AUTÓMATA // LEGIÓN SINTÉTICA", (int)autoHdrRec.X + 14, (int)autoHdrRec.Y + 5, 11, Constants.DemocracyRed);
        Raylib.DrawText("[MODELOS: 12 REGISTRADOS]", (int)autoHdrRec.X + (int)autoHdrRec.Width - 170, (int)autoHdrRec.Y + 6, 9, Constants.Cyan);

        // 3. Render 24 Cards (12 Terminids, 12 Automatons)
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            Rectangle cardRec = GetBestiaryCardRec(i);
            bool isSel = (_selectedBestiaryIndex == i);
            bool isHover = Raylib.CheckCollisionPointRec(mouse, cardRec);

            Color borderCol = isSel ? Constants.Gold : (isHover ? (entry.Faction == Faction.Terminids ? Constants.OrangeHazard : Constants.Cyan) : new Color(42, 58, 78, 220));
            Color bgCol = isSel ? new Color(22, 34, 54, 255) : (isHover ? new Color(18, 26, 42, 245) : new Color(12, 18, 30, 235));

            DrawTacticalPanel(cardRec, borderCol, bgCol);

            // Corner ticks if selected
            if (isSel)
            {
                int x = (int)cardRec.X, y = (int)cardRec.Y, w = (int)cardRec.Width, h = (int)cardRec.Height;
                Raylib.DrawLine(x, y, x + 5, y, Constants.Gold);
                Raylib.DrawLine(x, y, x, y + 5, Constants.Gold);
                Raylib.DrawLine(x + w, y, x + w - 5, y, Constants.Gold);
                Raylib.DrawLine(x + w, y, x + w, y + 5, Constants.Gold);
                Raylib.DrawLine(x, y + h, x + 5, y + h, Constants.Gold);
                Raylib.DrawLine(x, y + h, x, y + h - 5, Constants.Gold);
                Raylib.DrawLine(x + w, y + h, x + w - 5, y + h, Constants.Gold);
                Raylib.DrawLine(x + w, y + h, x + w, y + h - 5, Constants.Gold);
            }

            // Miniature Viewport (Left square)
            Rectangle vpRec = new(cardRec.X + 4, cardRec.Y + 4, 52, 52);
            Raylib.DrawRectangleRec(vpRec, new Color(7, 11, 18, 255));
            Raylib.DrawRectangleLinesEx(vpRec, 1, isSel ? Constants.Gold : new Color(30, 42, 58, 255));

            // Grid lines inside viewport
            Raylib.DrawLine((int)vpRec.X + 26, (int)vpRec.Y + 2, (int)vpRec.X + 26, (int)vpRec.Y + 50, new Color(25, 38, 54, 180));
            Raylib.DrawLine((int)vpRec.X + 2, (int)vpRec.Y + 26, (int)vpRec.X + 50, (int)vpRec.Y + 26, new Color(25, 38, 54, 180));

            // AUTHENTIC EXTRACTED PIXEL ART SPRITE RENDER!
            Vector2 vpCenter = new(vpRec.X + 26, vpRec.Y + 26);
            EnemySpriteManager.DrawSpriteCentered(entry.SpriteKey, vpCenter, 44f, Color.White);

            // Threat level indicator pips (top right corner)
            int pipStartX = (int)cardRec.X + (int)cardRec.Width - 36;
            int pipY = (int)cardRec.Y + 6;
            Color pipCol = entry.ThreatPips switch
            {
                1 => Constants.LibertyGreen,
                2 => Constants.SuperEarthYellow,
                3 => Constants.OrangeHazard,
                _ => Constants.DemocracyRed
            };
            for (int p = 1; p <= 4; p++)
            {
                int px = pipStartX + (p - 1) * 8;
                if (p <= entry.ThreatPips)
                {
                    Raylib.DrawRectangle(px, pipY, 6, 6, pipCol);
                    Raylib.DrawRectangleLines(px, pipY, 6, 6, Color.White);
                }
                else
                {
                    Raylib.DrawRectangleLines(px, pipY, 6, 6, new Color(50, 65, 85, 200));
                }
            }

            // Card details text
            int textX = (int)cardRec.X + 60;
            string nameTxt = entry.Name.ToUpper();
            if (nameTxt.Length > 16) nameTxt = nameTxt.Substring(0, 15) + ".";
            Raylib.DrawText(nameTxt, textX, (int)cardRec.Y + 6, 10, isSel ? Constants.Gold : Color.White);

            string engTxt = entry.EnglishName.ToUpper();
            if (engTxt.Length > 20) engTxt = engTxt.Substring(0, 18) + ".";
            Raylib.DrawText(engTxt, textX, (int)cardRec.Y + 20, 8, Constants.Cyan);

            string armTxt = entry.ArmorClass;
            if (armTxt.Length > 18) armTxt = armTxt.Substring(0, 16) + "..";
            Color wpCol = entry.Faction == Faction.Terminids ? Constants.SuperEarthYellow : Constants.LibertyGreen;
            Raylib.DrawText(armTxt, textX, (int)cardRec.Y + 34, 8, wpCol);

            string weakTxt = $"P.DÉBIL: {entry.Weakpoints}";
            if (weakTxt.Length > 20) weakTxt = weakTxt.Substring(0, 18) + "..";
            Raylib.DrawText(weakTxt, textX, (int)cardRec.Y + 46, 7, Color.LightGray);
        }

        // 4. Detailed Tactical Intel Dossier Panel
        Rectangle dossierRec = new(35, 348, 1210, 304);
        var sel = entries[_selectedBestiaryIndex];
        Color dossierBorder = sel.Faction == Faction.Terminids ? Constants.OrangeHazard : Constants.DemocracyRed;
        DrawTacticalPanel(dossierRec, dossierBorder, new Color(10, 16, 28, 252));

        // Dossier Header bar inside
        Rectangle dBar = new(35, 348, 1210, 24);
        Raylib.DrawRectangleRec(dBar, sel.Faction == Faction.Terminids ? new Color(38, 24, 12, 255) : new Color(38, 14, 18, 255));
        Raylib.DrawRectangleLinesEx(dBar, 1, dossierBorder);
        Raylib.DrawText("[ ARCHIVO DE INTELIGENCIA DE SUPER TIERRA // DATOS BALÍSTICOS Y BIOMÉTRICOS ]", (int)dBar.X + 16, (int)dBar.Y + 6, 10, Constants.Gold);
        Raylib.DrawText("ESTADO: REGISTRO ACTIVO // ACCESO: NIVEL HELLDIVER", (int)dBar.X + (int)dBar.Width - 325, (int)dBar.Y + 6, 9, Constants.Cyan);

        // Section A: Holographic Target Scanner (Left)
        Rectangle scanRec = new(50, 378, 220, 260);
        Raylib.DrawRectangleRec(scanRec, new Color(6, 10, 18, 255));
        Raylib.DrawRectangleLinesEx(scanRec, 1, Constants.BorderColor);

        Vector2 scanCenter = new(scanRec.X + scanRec.Width / 2f, scanRec.Y + 115f);
        Raylib.DrawCircleLines((int)scanCenter.X, (int)scanCenter.Y, 75f, new Color(30, 48, 72, 180));
        Raylib.DrawCircleLines((int)scanCenter.X, (int)scanCenter.Y, 52f, new Color(30, 48, 72, 180));
        Raylib.DrawCircleLines((int)scanCenter.X, (int)scanCenter.Y, 28f, new Color(30, 48, 72, 180));

        // Radar scanning beam
        float scanAngle = (float)Raylib.GetTime() * 2.2f;
        Raylib.DrawLineEx(scanCenter, scanCenter + new Vector2(MathF.Cos(scanAngle), MathF.Sin(scanAngle)) * 77f, 1.5f, new Color(56, 189, 248, 130));

        // AUTHENTIC EXTRACTED PIXEL ART SPRITE RENDER IN SCANNER!
        float scBob = MathF.Sin((float)Raylib.GetTime() * 2f) * 4f;
        EnemySpriteManager.DrawSpriteCentered(sel.SpriteKey, scanCenter + new Vector2(0, scBob), 140f, Color.White);

        // Weakpoint target reticle
        float retPulse = 10f + MathF.Sin((float)Raylib.GetTime() * 6f) * 3f;
        Raylib.DrawCircleLines((int)scanCenter.X, (int)scanCenter.Y, retPulse, Constants.DemocracyRed);

        // Scanner label
        string scanLbl = $"ESCÁNER HOLOGRÁFICO // {sel.EnglishName.ToUpper()}";
        Raylib.DrawText(scanLbl, (int)scanRec.X + (int)scanRec.Width / 2 - Raylib.MeasureText(scanLbl, 9) / 2, (int)scanRec.Y + (int)scanRec.Height - 20, 9, Constants.LibertyGreen);

        // Section B: Classification, Threat & Biometrics (Middle)
        int midX = 285;
        Raylib.DrawText(sel.Name.ToUpper(), midX, 378, 18, Constants.Gold);
        string idTxt = $"CODENAME: {sel.Codename.ToUpper()} // ID: {sel.EnglishName.ToUpper()}";
        Raylib.DrawText(idTxt, midX, 401, 10, Constants.Cyan);

        string skulls = "";
        for (int s = 0; s < 4; s++) skulls += (s < sel.ThreatPips ? "💀 " : "░ ");
        Raylib.DrawText($"NIVEL DE AMENAZA: CLASE {sel.ThreatPips}/4  [{skulls.Trim()}]", midX, 417, 11, Constants.DemocracyRed);

        // Specs grid box
        Rectangle specBox = new(midX, 437, 455, 52);
        Raylib.DrawRectangleRec(specBox, new Color(14, 22, 36, 255));
        Raylib.DrawRectangleLinesEx(specBox, 1, Constants.BorderColor);
        Raylib.DrawText($"BLINDAJE: {sel.ArmorClass.ToUpper()}", midX + 10, (int)specBox.Y + 8, 9, Constants.SuperEarthYellow);
        Raylib.DrawText($"PUNTO DÉBIL: {sel.Weakpoints.ToUpper()}", midX + 10, (int)specBox.Y + 28, 9, Constants.LibertyGreen);

        // Field description
        Raylib.DrawText("INFORME DE COMBATE DE SUPER TIERRA:", midX, 498, 10, Constants.Gold);
        DrawTextWrapped(sel.Description, midX, 516, 455, 9, Color.LightGray, 4);

        // Section C: Countermeasures & Survival Manual (Right)
        int rightX = 760;
        Raylib.DrawText("[ PROTOCOLO DE NEUTRALIZACIÓN AUTORIZADO ]", rightX, 378, 12, Constants.Gold);

        // Recommended armament box
        Rectangle armBox = new(rightX, 400, 470, 64);
        Raylib.DrawRectangleRec(armBox, new Color(14, 22, 36, 255));
        Raylib.DrawRectangleLinesEx(armBox, 1, Constants.BorderColor);
        Raylib.DrawText("ESTRATAGEMAS Y ARMAMENTO EFECTIVO:", rightX + 10, (int)armBox.Y + 8, 9, Constants.Cyan);
        DrawTextWrapped(sel.RecommendedWeapons, rightX + 10, (int)armBox.Y + 26, 450, 9, Color.White, 2);

        // Survival guide box
        Rectangle survBox = new(rightX, 472, 470, 166);
        Raylib.DrawRectangleRec(survBox, new Color(14, 22, 36, 255));
        Raylib.DrawRectangleLinesEx(survBox, 1, Constants.BorderColor);
        Raylib.DrawText("MANUAL DE SUPERVIVENCIA Y EVASIÓN:", rightX + 10, (int)survBox.Y + 8, 9, Constants.DemocracyRed);
        DrawTextWrapped(sel.TacticalAdvice, rightX + 10, (int)survBox.Y + 26, 450, 9, Color.LightGray, 5);

        // 5. Footer Back Button
        Rectangle btnBack = new(cx - 130, 660, 260, 36);
        DrawButton(btnBack, "VOLVER AL PUENTE (ESC)", Raylib.CheckCollisionPointRec(mouse, btnBack));

        string hint = "Haz clic en cualquier espécimen para inspeccionar su dossier de combate // Usa [ESPACIO] para la Zambullida Táctica en combate.";
        Raylib.DrawText(hint, cx - Raylib.MeasureText(hint, 9) / 2, 702, 9, new Color(120, 145, 175, 200));
    }
}
