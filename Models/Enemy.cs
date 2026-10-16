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
        else // Automatons
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

        return e;
    }

    public void Update(float dt, Vector2 playerPos, bool playerIsDiving, Action<Bullet> spawnBullet, Action<string> onVoiceAlert)
    {
        WalkCycle += dt * 8f;
        if (HitFlashTimer > 0) HitFlashTimer -= dt;
        var diff = playerPos - Position;
        var dist = diff.Length();
        var angleToPlayer = MathF.Atan2(diff.Y, diff.X);

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
