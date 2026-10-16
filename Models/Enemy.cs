using System.Numerics;

namespace HelldiversDesktop.Models;

public class Enemy
{
    public Faction Faction;
    public EnemyType Type;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Hp;
    public float MaxHp;
    public float Radius = 20f;
    public float Speed = 2.0f;
    public float Damage = 20f;
    public float WalkCycle = 0f;
    public bool IsHvt = false;
    public float FacingAngle = 0f;
    public string? SpriteKey = null;
    public float HitFlashTimer = 0f;
    public int Id;
    public float AttackLunge = 0f;
    public bool InAttackRange = false;
    private static int _nextEnemyId = 1;

    public float ShieldHp = 0f;
    public float MaxShieldHp = 0f;
    public float ShieldRechargeTimer = 0f;
    public float TeleportCooldown = 2.5f;
    public float PsionicStormTimer = 4.0f;

    // Special mechanics timers
    public bool Charging = false;
    public float ChargeCooldown = 2.5f;
    public float ChargeTime = 0f;
    public float ChargeSpeed = 5.8f;
    public float ShootCooldown = 2.0f;
    public float SpitTimer = 3.5f;
    public float LeapCooldown = 2.4f;
    public float RocketCooldown = 3.2f;

    private static readonly Random _rnd = new();

    public static Enemy Create(EnemyType type, Faction faction, Vector2 pos, bool isHvt = false, string? spriteKey = null)
    {
        var e = new Enemy
        {
            Id = _nextEnemyId++,
            Type = type,
            Faction = faction,
            Position = pos,
            IsHvt = isHvt,
            SpriteKey = spriteKey
        };

        if (faction == Faction.Terminids)
        {
            switch (type)
            {
                case EnemyType.Charger:
                    e.Hp = e.MaxHp = isHvt ? 2200 : 1600;
                    e.Radius = 38f;
                    e.Speed = 1.4f;
                    e.ChargeSpeed = 5.8f;
                    e.Damage = 45f;
                    e.SpriteKey ??= (isHvt ? "impaler" : "charger");
                    break;
                case EnemyType.Titan:
                    e.Hp = e.MaxHp = 4800;
                    e.Radius = 65f;
                    e.Speed = 1.3f;
                    e.Damage = 65f;
                    e.SpitTimer = 4.0f;
                    e.IsHvt = true;
                    e.SpriteKey ??= "bile_titan";
                    break;
                case EnemyType.Spitter:
                    e.Hp = e.MaxHp = 750;
                    e.Radius = 24f;
                    e.Speed = 1.6f;
                    e.Damage = 25f;
                    e.ShootCooldown = 2.4f;
                    if (e.SpriteKey == null)
                    {
                        double roll = _rnd.NextDouble();
                        e.SpriteKey = roll < 0.5 ? "bile_spewer" : (roll < 0.8 ? "nursing_spewer" : "bile_spewer_puke");
                    }
                    break;
                case EnemyType.Hunter:
                    e.Hp = e.MaxHp = 180;
                    e.Radius = 17f;
                    e.Speed = 2.9f;
                    e.Damage = 22f;
                    e.LeapCooldown = 2.4f;
                    e.SpriteKey ??= (_rnd.NextDouble() < 0.65 ? "hunter" : "stalker");
                    break;
                case EnemyType.Warrior:
                    e.Hp = e.MaxHp = 200;
                    e.Radius = 24f;
                    e.Speed = 1.7f;
                    e.Damage = 24f;
                    if (e.SpriteKey == null)
                    {
                        double roll = _rnd.NextDouble();
                        e.SpriteKey = roll < 0.55 ? "warrior" : (roll < 0.82 ? "hive_guard" : "brood_commander");
                    }
                    break;
                case EnemyType.Scavenger:
                default:
                    e.Hp = e.MaxHp = 45;
                    e.Radius = 14f;
                    e.Speed = 2.4f;
                    e.Damage = 14f;
                    e.SpriteKey ??= (_rnd.NextDouble() < 0.65 ? "scavenger" : "pouncer");
                    break;
            }
        }
        else if (faction == Faction.Automatons)
        {
            switch (type)
            {
                case EnemyType.Hulk:
                    e.Hp = e.MaxHp = isHvt ? 2000 : 1500;
                    e.Radius = 40f;
                    e.Speed = 1.3f;
                    e.Damage = 50f;
                    e.ShootCooldown = 2.5f;
                    e.SpriteKey ??= (isHvt ? "tank" : "hulk");
                    break;
                case EnemyType.Berserker:
                    e.Hp = e.MaxHp = 650;
                    e.Radius = 22f;
                    e.Speed = 2.6f;
                    e.Damage = 32f;
                    e.SpriteKey ??= (_rnd.NextDouble() < 0.5 ? "berserker" : "berserker_alt");
                    break;
                case EnemyType.Devastator:
                    e.Hp = e.MaxHp = 600;
                    e.Radius = 26f;
                    e.Speed = 1.5f;
                    e.Damage = 25f;
                    e.ShootCooldown = 2.0f;
                    e.RocketCooldown = 3.4f;
                    if (e.SpriteKey == null)
                    {
                        double roll = _rnd.NextDouble();
                        e.SpriteKey = roll < 0.4 ? "devastator" : (roll < 0.7 ? "heavy_devastator" : "rocket_devastator");
                    }
                    break;
                case EnemyType.FactoryStrider:
                    e.Hp = e.MaxHp = 5000;
                    e.Radius = 70f;
                    e.Speed = 1.1f;
                    e.Damage = 70f;
                    e.ShootCooldown = 1.8f;
                    e.IsHvt = true;
                    e.SpriteKey ??= "factory_strider";
                    break;
                case EnemyType.Commissar:
                    e.Hp = e.MaxHp = 85;
                    e.Radius = 17f;
                    e.Speed = 2.1f;
                    e.Damage = 22f;
                    e.ShootCooldown = 1.9f;
                    e.SpriteKey ??= "commissar";
                    break;
                case EnemyType.Hound:
                    e.Hp = e.MaxHp = 135;
                    e.Radius = 16f;
                    e.Speed = 3.3f;
                    e.Damage = 26f;
                    e.LeapCooldown = 1.8f;
                    e.SpriteKey ??= "hound";
                    break;
                case EnemyType.Butcher:
                    e.Hp = e.MaxHp = 950;
                    e.Radius = 26f;
                    e.Speed = 2.4f;
                    e.Damage = 42f;
                    e.SpriteKey ??= "butcher";
                    break;
                case EnemyType.Warlord:
                    e.Hp = e.MaxHp = isHvt ? 3400 : 2800;
                    e.Radius = 46f;
                    e.Speed = 1.25f;
                    e.Damage = 55f;
                    e.ShootCooldown = 2.2f;
                    e.RocketCooldown = 3.6f;
                    e.IsHvt = true;
                    e.SpriteKey ??= "warlord";
                    break;
                case EnemyType.Trooper:
                default:
                    e.Hp = e.MaxHp = 45;
                    e.Radius = 16f;
                    e.Speed = 1.8f;
                    e.Damage = 16f;
                    e.ShootCooldown = 2.2f;
                    if (e.SpriteKey == null)
                    {
                        double roll = _rnd.NextDouble();
                        e.SpriteKey = roll < 0.5 ? "trooper" : (roll < 0.8 ? "raider" : "scout_strider");
                    }
                    break;
            }
        }
        else // The Illuminate (Los Iluminados)
        {
            switch (type)
            {
                case EnemyType.Watcher:
                    e.Hp = e.MaxHp = 95;
                    e.ShieldHp = e.MaxShieldHp = 70;
                    e.Radius = 16f;
                    e.Speed = 2.4f;
                    e.Damage = 15f;
                    e.ShootCooldown = 2.6f;
                    e.SpriteKey ??= "watcher";
                    break;
                case EnemyType.Outcast:
                    e.Hp = e.MaxHp = 240;
                    e.ShieldHp = e.MaxShieldHp = 160;
                    e.Radius = 18f;
                    e.Speed = 3.1f;
                    e.Damage = 38f;
                    e.TeleportCooldown = 2.4f;
                    e.SpriteKey ??= "outcast";
                    break;
                case EnemyType.Tripod:
                    e.Hp = e.MaxHp = 780;
                    e.ShieldHp = e.MaxShieldHp = 360;
                    e.Radius = 28f;
                    e.Speed = 1.6f;
                    e.Damage = 32f;
                    e.ShootCooldown = 2.2f;
                    e.SpriteKey ??= "tripod";
                    break;
                case EnemyType.Illusionist:
                    e.Hp = e.MaxHp = 1250;
                    e.ShieldHp = e.MaxShieldHp = 650;
                    e.Radius = 32f;
                    e.Speed = 1.45f;
                    e.Damage = 45f;
                    e.ShootCooldown = 2.5f;
                    e.TeleportCooldown = 4.2f;
                    e.SpriteKey ??= "illusionist";
                    break;
                case EnemyType.CouncilMember:
                default:
                    e.Hp = e.MaxHp = 4600;
                    e.ShieldHp = e.MaxShieldHp = 1800;
                    e.Radius = 58f;
                    e.Speed = 1.2f;
                    e.Damage = 65f;
                    e.ShootCooldown = 1.8f;
                    e.PsionicStormTimer = 4.2f;
                    e.IsHvt = true;
                    e.SpriteKey ??= "council_member";
                    break;
            }
        }

        return e;
    }

    public void Update(float dt, Vector2 playerPos, bool playerIsDiving, Action<Bullet> spawnBullet, Action<string> onVoiceAlert)
    {
        WalkCycle += dt * 8f;
        if (HitFlashTimer > 0) HitFlashTimer -= dt;

        // Recarga de Escudo de Energía de los Iluminados
        if (MaxShieldHp > 0f)
        {
            if (ShieldRechargeTimer > 0f)
            {
                ShieldRechargeTimer -= dt;
            }
            else if (ShieldHp < MaxShieldHp)
            {
                ShieldHp = MathF.Min(MaxShieldHp, ShieldHp + dt * 45f);
            }
        }

        var diff = playerPos - Position;
        var dist = diff.Length();
        var angleToPlayer = MathF.Atan2(diff.Y, diff.X);

        InAttackRange = dist < (Radius + 35f);
        if (InAttackRange && AttackLunge <= 0f) AttackLunge = 12f;
        if (AttackLunge > 0f) AttackLunge = MathF.Max(0f, AttackLunge - dt * 24f);

        if (Type == EnemyType.Charger)
        {
            ChargeCooldown -= dt;
            if (!Charging && ChargeCooldown <= 0 && dist < 380)
            {
                Charging = true;
                Velocity = new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * ChargeSpeed;
                ChargeTime = 1.6f;
                FacingAngle = angleToPlayer;
            }

            if (Charging)
            {
                Position += Velocity;
                ChargeTime -= dt;
                if (ChargeTime <= 0)
                {
                    Charging = false;
                    ChargeCooldown = 2.8f;
                }
            }
            else
            {
                FacingAngle = angleToPlayer;
                Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            }
        }
        else if (Type == EnemyType.Titan)
        {
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            SpitTimer -= dt;
            if (SpitTimer <= 0 && dist < 480)
            {
                SpitTimer = 4.2f;
                onVoiceAlert("☣️ ¡TITÁN ESCUPIENDO BILIS CORROSIVA!");
                for (int s = -2; s <= 2; s++)
                {
                    var sAngle = angleToPlayer + s * 0.16f;
                    spawnBullet(new Bullet
                    {
                        Position = Position + new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 55,
                        Velocity = new Vector2(MathF.Cos(sAngle), MathF.Sin(sAngle)) * 9.5f,
                        Damage = 32,
                        Type = BulletType.Acid,
                        IsPlayerBullet = false,
                        Life = 1.8f
                    });
                }
            }
        }
        else if (Type == EnemyType.Hulk)
        {
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            ShootCooldown -= dt;
            if (ShootCooldown <= 0)
            {
                ShootCooldown = 2.4f;
                if (dist < 220)
                {
                    // Flamethrower
                    var rnd = new Random();
                    for (int f = 0; f < 6; f++)
                    {
                        var spread = ((float)rnd.NextDouble() - 0.5f) * 0.5f;
                        var speed = (float)rnd.NextDouble() * 4f + 7f;
                        spawnBullet(new Bullet
                        {
                            Position = Position + new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 24,
                            Velocity = new Vector2(MathF.Cos(angleToPlayer + spread), MathF.Sin(angleToPlayer + spread)) * speed,
                            Damage = 26,
                            Type = BulletType.Flame,
                            IsPlayerBullet = false,
                            Life = 0.8f
                        });
                    }
                }
                else
                {
                    // Heavy Lasers
                    spawnBullet(new Bullet
                    {
                        Position = Position + new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 24,
                        Velocity = new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 11f,
                        Damage = 32,
                        Type = BulletType.BotLaser,
                        IsPlayerBullet = false,
                        Life = 2.2f
                    });
                }
            }
        }
        else if (Type == EnemyType.Warlord)
        {
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            ShootCooldown -= dt;
            RocketCooldown -= dt;

            // Disparo de cañones dobles
            if (ShootCooldown <= 0)
            {
                ShootCooldown = 2.0f;
                for (int c = -1; c <= 1; c += 2)
                {
                    Vector2 cOff = new(-MathF.Sin(angleToPlayer) * c * 16f, MathF.Cos(angleToPlayer) * c * 16f);
                    spawnBullet(new Bullet
                    {
                        Position = Position + cOff,
                        Velocity = new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 12.5f,
                        Damage = 36,
                        Type = BulletType.BotLaser,
                        IsPlayerBullet = false,
                        Life = 2.2f
                    });
                }
            }

            // Salva de cohetes pesados
            if (RocketCooldown <= 0 && dist < 420)
            {
                RocketCooldown = 4.2f;
                onVoiceAlert("⚠️ ¡WARLORD DISPARANDO SALVA DE COHETES!");
                for (int r = -1; r <= 1; r++)
                {
                    float rAng = angleToPlayer + r * 0.18f;
                    spawnBullet(new Bullet
                    {
                        Position = Position + new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 25f,
                        Velocity = new Vector2(MathF.Cos(rAng), MathF.Sin(rAng)) * 8.5f,
                        Damage = 45,
                        Type = BulletType.Rocket,
                        IsPlayerBullet = false,
                        Life = 2.5f
                    });
                }
            }
        }
        else if (Type == EnemyType.Hound)
        {
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            LeapCooldown -= dt;

            // Salto rápido de mordida cybernética
            if (LeapCooldown <= 0 && dist > 80 && dist < 240)
            {
                LeapCooldown = 2.2f;
                Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 75f;
                AttackLunge = 18f;
            }
        }
        else if (Type == EnemyType.Butcher)
        {
            // Persecución implacable cuerpo a cuerpo
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
        }
        else if (Type == EnemyType.Hunter)
        {
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            LeapCooldown -= dt;
            if (LeapCooldown <= 0 && dist > 140 && dist < 300)
            {
                LeapCooldown = 3.4f;
                Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 85f;
            }
        }
        else if (Type == EnemyType.Outcast)
        {
            // Asesino Iluminado con espadas psiónicas y Teletransportación dimensional
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            TeleportCooldown -= dt;

            if (TeleportCooldown <= 0 && dist > 90 && dist < 360)
            {
                TeleportCooldown = 3.2f;
                // Teletransporte parpadeante hacia el Helldiver
                float blinkAngle = angleToPlayer + ((float)_rnd.NextDouble() - 0.5f) * 0.8f;
                Position = playerPos - new Vector2(MathF.Cos(blinkAngle), MathF.Sin(blinkAngle)) * 65f;
                AttackLunge = 16f;
            }
        }
        else if (Type == EnemyType.Illusionist)
        {
            FacingAngle = angleToPlayer;
            // Mantiene distancia media táctica
            if (dist > 300) Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            else if (dist < 180) Position -= new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;

            TeleportCooldown -= dt;
            if (TeleportCooldown <= 0 && dist < 120)
            {
                // Teletransporte de escape defensivo
                TeleportCooldown = 4.5f;
                Position -= new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 160f;
            }

            ShootCooldown -= dt;
            if (ShootCooldown <= 0)
            {
                ShootCooldown = 2.4f;
                // Proyectil psiónico teledirigido
                spawnBullet(new Bullet
                {
                    Position = Position,
                    Velocity = new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 7.5f,
                    Damage = Damage,
                    Type = BulletType.PsionicOrb,
                    IsPlayerBullet = false,
                    Life = 3.2f
                });
            }
        }
        else if (Type == EnemyType.CouncilMember)
        {
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;

            ShootCooldown -= dt;
            PsionicStormTimer -= dt;

            if (ShootCooldown <= 0)
            {
                ShootCooldown = 2.0f;
                for (int o = -2; o <= 2; o++)
                {
                    float oAng = angleToPlayer + o * 0.22f;
                    spawnBullet(new Bullet
                    {
                        Position = Position + new Vector2(MathF.Cos(oAng), MathF.Sin(oAng)) * 32f,
                        Velocity = new Vector2(MathF.Cos(oAng), MathF.Sin(oAng)) * 8.0f,
                        Damage = 35,
                        Type = BulletType.PsionicOrb,
                        IsPlayerBullet = false,
                        Life = 3.0f
                    });
                }
            }

            if (PsionicStormTimer <= 0)
            {
                PsionicStormTimer = 5.5f;
                onVoiceAlert("⚡ ¡MIEMBRO DEL CONSEJO DESATANDO TORMENTA PSÍONICA!");
                for (int a = 0; a < 8; a++)
                {
                    float angle = a * (MathF.PI * 2f / 8f);
                    spawnBullet(new Bullet
                    {
                        Position = Position,
                        Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 6.5f,
                        Damage = 28,
                        Type = BulletType.PsionicOrb,
                        IsPlayerBullet = false,
                        Life = 2.5f
                    });
                }
            }
        }
        else if (Type == EnemyType.Tripod)
        {
            FacingAngle = angleToPlayer;
            if (dist > 260) Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            else if (dist < 180) Position -= new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;

            ShootCooldown -= dt;
            if (ShootCooldown <= 0)
            {
                ShootCooldown = 2.0f;
                spawnBullet(new Bullet
                {
                    Position = Position + new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 22f,
                    Velocity = new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 12.0f,
                    Damage = Damage,
                    Type = BulletType.PsionicBeam,
                    IsPlayerBullet = false,
                    Life = 1.8f
                });
            }
        }
        else if (Type == EnemyType.Watcher)
        {
            FacingAngle = angleToPlayer;
            // Dron de vigilancia que orbita al jugador y alerta
            if (dist > 280) Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            else if (dist < 190) Position -= new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;

            ShootCooldown -= dt;
            if (ShootCooldown <= 0 && dist < 320)
            {
                ShootCooldown = 3.0f;
                onVoiceAlert("👁️ ¡VIGÍA ILUMINADO EMITIENDO ALERTA DIMENSIONAL!");
                spawnBullet(new Bullet
                {
                    Position = Position,
                    Velocity = new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 9.0f,
                    Damage = 16,
                    Type = BulletType.PsionicOrb,
                    IsPlayerBullet = false,
                    Life = 2.5f
                });
            }
        }
        else if (Type == EnemyType.Spitter || Type == EnemyType.Trooper || Type == EnemyType.Devastator || Type == EnemyType.Commissar)
        {
            FacingAngle = angleToPlayer;
            if (dist > 240) Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
            else if (dist < 160) Position -= new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;

            ShootCooldown -= dt;
            if (ShootCooldown <= 0)
            {
                ShootCooldown = Type == EnemyType.Devastator ? 1.8f : 2.2f;
                spawnBullet(new Bullet
                {
                    Position = Position,
                    Velocity = new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * 8.5f,
                    Damage = Damage,
                    Type = Faction == Faction.Terminids ? BulletType.Acid : BulletType.BotLaser,
                    IsPlayerBullet = false,
                    Life = 2.2f
                });
            }
        }
        else
        {
            FacingAngle = angleToPlayer;
            Position += new Vector2(MathF.Cos(angleToPlayer), MathF.Sin(angleToPlayer)) * Speed;
        }

        // Clamp to world
        Position.X = Math.Clamp(Position.X, 30, Constants.WorldSize - 30);
        Position.Y = Math.Clamp(Position.Y, 30, Constants.WorldSize - 30);
    }
}
