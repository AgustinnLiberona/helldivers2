using System.Numerics;

namespace HelldiversDesktop.Models;

public class Player
{
    public Vector2 Position;
    public float Radius = 17f;
    public float Speed = 4.4f;
    public float Hp = 100f;
    public float MaxHp = 100f;
    public float Shield = 0f;
    public float MaxShield = 0f;
    public float ShieldRegenTimer = 0f;

    public float Angle = 0f;
    public float WalkCycle = 0f;
    public bool IsMoving = false;
    public List<Vector2> CapeHistory = new();
    public float MuzzleFlash = 0f;

    public Weapon CurrentWeapon = WeaponRegistry.Weapons["liberator"];
    public int Ammo = 45;
    public int MaxAmmo = 45;
    public bool IsReloading = false;
    public float ReloadTimer = 0f;
    public float ReloadDuration = 1.25f;
    public float FireCooldown = 0f;

    public bool IsDiving = false;
    public float DiveTimer = 0f;
    public float DiveDuration = 0.75f;
    public float DiveCooldown = 0f;
    public Vector2 DiveVelocity = Vector2.Zero;

    public bool HasGuardDog = false;
    public float GuardDogAngle = 0f;
    public float GuardDogCooldown = 0f;

    public Enemy? AimTarget = null;

    public void Reset(SaveData save)
    {
        Position = new Vector2(Constants.WorldSize / 2f, Constants.WorldSize / 2f);
        Angle = 0f;
        AimTarget = null;

        var h = save.Hangar;
        MaxHp = 100f + h.MaxHpLevel * 20f;
        Hp = MaxHp;
        Speed = 4.4f * (1f + h.SpeedLevel * 0.07f);
        MaxShield = h.ShieldLevel > 0 ? 50f + h.ShieldLevel * 25f : 0f;
        Shield = MaxShield;

        CurrentWeapon = WeaponRegistry.GetWeapon(save.EquippedWeapon);
        MaxAmmo = CurrentWeapon.Ammo;
        Ammo = MaxAmmo;
        FireCooldown = 0f;
        IsReloading = false;

        HasGuardDog = save.EquippedStratagems.Contains("guard_dog");
        GuardDogAngle = 0f;
        GuardDogCooldown = 0f;

        IsDiving = false;
        DiveCooldown = 0f;
        DiveVelocity = Vector2.Zero;

        CapeHistory.Clear();
        for (int i = 0; i < 8; i++) CapeHistory.Add(Position);
    }

    public void Dive(Vector2 moveInput)
    {
        if (IsDiving || DiveCooldown > 0f) return;
        IsDiving = true;
        DiveTimer = DiveDuration;
        DiveCooldown = 1.1f;

        Vector2 dir = moveInput.LengthSquared() > 0.01f ? Vector2.Normalize(moveInput) : new Vector2(MathF.Cos(Angle), MathF.Sin(Angle));
        DiveVelocity = dir * 14.5f;
    }

    public void Update(float dt, Vector2 moveInput, List<Enemy> enemies, Action<Bullet> spawnBullet, Action<Particle> spawnParticle, Action playShootSfx, Vector2? mouseWorld = null, bool autoAim = true, bool isFiring = false)
    {
        if (DiveCooldown > 0f) DiveCooldown -= dt;
        if (MuzzleFlash > 0f) MuzzleFlash -= dt;

        if (IsDiving)
        {
            DiveTimer -= dt;
            Position += DiveVelocity;
            DiveVelocity *= 0.93f;
            if (DiveTimer <= 0f) IsDiving = false;
            IsMoving = true;
        }
        else
        {
            IsMoving = moveInput.LengthSquared() > 0.01f;
            if (IsMoving)
            {
                var dir = Vector2.Normalize(moveInput);
                Position += dir * Speed;
                WalkCycle += dt * 15f;
            }
            else
            {
                WalkCycle = 0f;
            }
        }

        // Clamp to world boundaries
        Position.X = Math.Clamp(Position.X, 30, Constants.WorldSize - 30);
        Position.Y = Math.Clamp(Position.Y, 30, Constants.WorldSize - 30);

        // Energy shield regen
        if (MaxShield > 0)
        {
            ShieldRegenTimer += dt;
            if (ShieldRegenTimer > 4.0f && Shield < MaxShield)
            {
                Shield = Math.Min(MaxShield, Shield + 18f * dt);
            }
        }

        // Cape cloth physics simulation
        if (CapeHistory.Count > 0)
        {
            var anchor = Position - new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * 8f;
            CapeHistory[0] = anchor;
            for (int i = 1; i < CapeHistory.Count; i++)
            {
                var prev = CapeHistory[i - 1];
                var cur = CapeHistory[i];
                var d = Vector2.Distance(prev, cur);
                var targetDist = 4.0f;
                if (d > targetDist)
                {
                    var factor = (d - targetDist) / d;
                    cur += (prev - cur) * factor;
                    CapeHistory[i] = cur;
                }
            }
        }

        // Aiming logic
        if (autoAim)
        {
            UpdateAim(enemies);
            UpdateShooting(dt, spawnBullet, spawnParticle, playShootSfx);
        }
        else
        {
            if (mouseWorld.HasValue)
            {
                var diff = mouseWorld.Value - Position;
                Angle = MathF.Atan2(diff.Y, diff.X);
            }
            UpdateShooting(dt, spawnBullet, spawnParticle, playShootSfx, forceShoot: isFiring);
        }

        // Guard Dog Drone logic
        if (HasGuardDog)
        {
            GuardDogAngle += dt * 2.8f;
            GuardDogCooldown -= dt;
            if (GuardDogCooldown <= 0f)
            {
                Enemy? closest = null;
                float minDist = 320f;
                foreach (var e in enemies)
                {
                    if (e.Hp <= 0) continue;
                    var d = Vector2.Distance(Position, e.Position);
                    if (d < minDist) { minDist = d; closest = e; }
                }

                if (closest != null)
                {
                    closest.Hp -= 35f;
                    GuardDogCooldown = 0.25f;
                    spawnParticle(new Particle
                    {
                        Position = closest.Position,
                        TracerStart = Position + new Vector2(MathF.Cos(GuardDogAngle), MathF.Sin(GuardDogAngle)) * 38f,
                        IsTracer = true,
                        Color = Constants.Cyan,
                        Life = 0.08f
                    });
                }
            }
        }
    }

    private void UpdateAim(List<Enemy> enemies)
    {
        float maxRange = 520f + CurrentWeapon.RangeBonus;
        Enemy? best = null;
        float highestScore = -1f;

        foreach (var e in enemies)
        {
            if (e.Hp <= 0) continue;
            var dist = Vector2.Distance(Position, e.Position);
            if (dist > maxRange) continue;

            float score = 1000f - dist;
            if (e.IsHvt) score += 800f;
            if (e.Type == EnemyType.Charger || e.Type == EnemyType.Hulk) score += 500f;
            if (e.Type == EnemyType.Titan || e.Type == EnemyType.FactoryStrider) score += 950f;

            if (score > highestScore)
            {
                highestScore = score;
                best = e;
            }
        }

        AimTarget = best;
        if (best != null)
        {
            var targetAngle = MathF.Atan2(best.Position.Y - Position.Y, best.Position.X - Position.X);
            var diff = MathF.Atan2(MathF.Sin(targetAngle - Angle), MathF.Cos(targetAngle - Angle));
            Angle += diff * 0.35f;
        }
    }

    private void UpdateShooting(float dt, Action<Bullet> spawnBullet, Action<Particle> spawnParticle, Action playShootSfx, bool forceShoot = false)
    {
        if (FireCooldown > 0f) FireCooldown -= dt;

        if (IsReloading)
        {
            ReloadTimer -= dt;
            if (ReloadTimer <= 0f)
            {
                IsReloading = false;
                Ammo = MaxAmmo;
            }
            return;
        }

        if (Ammo <= 0)
        {
            IsReloading = true;
            ReloadTimer = ReloadDuration;
            return;
        }

        if ((AimTarget != null || forceShoot) && FireCooldown <= 0f)
        {
            Shoot(spawnBullet, spawnParticle, playShootSfx);
        }
    }

    private void Shoot(Action<Bullet> spawnBullet, Action<Particle> spawnParticle, Action playShootSfx)
    {
        FireCooldown = CurrentWeapon.FireRate;
        Ammo--;
        MuzzleFlash = 0.05f;
        playShootSfx();

        // Eject brass casing
        var casingDir = new Vector2(MathF.Cos(Angle - MathF.PI / 2), MathF.Sin(Angle - MathF.PI / 2));
        var rnd = new Random();
        spawnParticle(new Particle
        {
            Position = Position + new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * 10f,
            Velocity = casingDir * ((float)rnd.NextDouble() * 3f + 2f),
            Radius = 2.5f,
            Color = Constants.Gold,
            IsCasing = true,
            Life = 0.5f
        });

        int pellets = CurrentWeapon.Pellets;
        float bulletSpeed = CurrentWeapon.IsLaser ? 24f : (CurrentWeapon.IsPlasma ? 14f : (CurrentWeapon.IsDominator ? 15f : 18f));

        BulletType bType = BulletType.Standard;
        if (CurrentWeapon.IsLaser) bType = BulletType.Laser;
        else if (CurrentWeapon.IsPlasma) bType = BulletType.Plasma;
        else if (CurrentWeapon.IsDominator) bType = BulletType.Dominator;

        for (int i = 0; i < pellets; i++)
        {
            float spread = ((float)rnd.NextDouble() - 0.5f) * (pellets > 1 ? (CurrentWeapon.Knockback > 0 ? 0.28f : 0.35f) : 0.035f);
            float shootAngle = Angle + spread;

            spawnBullet(new Bullet
            {
                Position = Position + new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * 22f,
                Velocity = new Vector2(MathF.Cos(shootAngle), MathF.Sin(shootAngle)) * bulletSpeed,
                Damage = CurrentWeapon.Damage,
                Pierce = CurrentWeapon.Pierce,
                Knockback = CurrentWeapon.Knockback,
                Type = bType,
                IsPlayerBullet = true,
                Life = CurrentWeapon.RangeBonus > 0 ? 1.6f : 1.1f
            });
        }
    }

    public void TakeDamage(float amount)
    {
        ShieldRegenTimer = 0f;
        if (Shield > 0)
        {
            if (Shield >= amount)
            {
                Shield -= amount;
                return;
            }
            else
            {
                amount -= Shield;
                Shield = 0f;
            }
        }
        Hp -= amount;
    }
}
