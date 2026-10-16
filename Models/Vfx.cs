using System.Numerics;
using Raylib_cs;

namespace HelldiversDesktop.Models;

public class Bullet
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Damage;
    public int Pierce = 1;
    public float Knockback = 0;
    public BulletType Type = BulletType.Standard;
    public bool IsPlayerBullet = true;
    public float Life = 1.2f;
}

public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius = 2f;
    public Color Color = Color.Yellow;
    public float Life = 0.5f;
    public float MaxLife = 0.5f;
    public bool IsTracer = false;
    public Vector2 TracerStart;
    public bool IsCasing = false;
    public bool IsScrap = false;
}

public class DamageNumber
{
    public Vector2 Position;
    public string Text = "";
    public Color Color = Color.White;
    public float Life = 0.55f;
    public float VelocityY = -2f;
    public bool IsCrit = false;
}

public class AcidPool
{
    public Vector2 Position;
    public float Radius = 45f;
    public float Life = 5.5f;
    public float MaxLife = 5.5f;
}

public class BattlefieldDebris
{
    public Vector2 Position;
    public float Radius = 25f;
    public bool IsBots = false;
    public float SmokeTimer = 4.5f;
}

public class SplatterDrop
{
    public Vector2 Offset;
    public float Radius;
}

public class SplatterDecal
{
    public Vector2 Position;
    public float Radius;
    public Color Color;
    public Color AccentColor;
    public bool IsBots;
    public bool IsAcid;
    public List<SplatterDrop> Drops = new();
}

public class AtmosphericMote
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Size;
    public float Alpha;
}
