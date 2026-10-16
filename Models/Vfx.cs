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

public class ExplosionInstance
{
    public Vector2 Position;
    public float Timer = 0f;
    public float Duration = 1.8f;
    public string Type = "500kg"; // "500kg", "orbital", "grenade", "autocannon", "quasar"
    public float Radius = 320f;
    public int CurrentFrame => Math.Clamp((int)((Timer / Duration) * 12), 0, 11);
    public bool Finished => Timer >= Duration;
}

public class ShockwaveRing
{
    public Vector2 Position;
    public float Radius = 0f;
    public float MaxRadius = 350f;
    public float Speed = 600f;
    public float Life = 0.5f;
    public float MaxLife = 0.5f;
    public Color Color = Constants.Cyan;
}

public class MissionBannerNotification
{
    public string Title = "";
    public string Subtitle = "";
    public float Timer = 0f;
    public float Duration = 4.2f;
    public float MaxTimer { get => Duration; set => Duration = value; }
    public Color PrimaryColor = Constants.LibertyGreen;
    public Color AccentColor { get => PrimaryColor; set => PrimaryColor = value; }
    public bool Finished => Timer >= Duration;
    public bool Active => !Finished;
}

public class VictoryConfetti
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float AngularSpeed = 120f;
    public float RotSpeed { get => AngularSpeed; set => AngularSpeed = value; }
    public Color Color;
    public float Width = 8f;
    public float Height = 5f;
    public float Size { get => Width; set { Width = value; Height = value; } }
}

public class HellpodDrop
{
    public Vector2 TargetPos;
    public float Altitude = 1200f;
    public float Speed = 2400f;
    public float FallSpeed { get => Speed; set => Speed = value; }
    public bool Landed = false;
    public float FadeTimer = 2.0f;
    public float ImpactTimer { get => FadeTimer; set => FadeTimer = value; }
}

