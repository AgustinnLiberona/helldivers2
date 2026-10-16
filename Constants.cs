using System.Numerics;
using Raylib_cs;

namespace HelldiversDesktop;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    Armory,
    Stratagems,
    Hangar,
    Settings,
    Bestiary,
    Victory,
    GameOver
}

public enum Faction
{
    Terminids,
    Automatons
}

public enum MissionType
{
    Outposts,
    Icbm,
    Seismic,
    Assassinate
}

public enum EnemyType
{
    Scavenger,
    Hunter,
    Warrior,
    Spitter,
    Charger,
    Titan,
    Trooper,
    Commissar,
    Berserker,
    Devastator,
    Hulk,
    FactoryStrider
}

public enum BulletType
{
    Standard,
    Laser,
    Plasma,
    Dominator,
    Acid,
    BotLaser,
    Flame,
    Rocket
}

public static class Constants
{
    public const float WorldSize = 4500f;
    public const int DefaultScreenWidth = 1280;
    public const int DefaultScreenHeight = 720;
    public const int TargetFps = 144;

    // Palette Colors matching Super Earth aesthetic
    public static readonly Color Gold = new(255, 215, 0, 255);
    public static readonly Color SuperEarthYellow = new(234, 179, 8, 255);
    public static readonly Color Cyan = new(56, 189, 248, 255);
    public static readonly Color DemocracyRed = new(239, 68, 68, 255);
    public static readonly Color LibertyGreen = new(34, 197, 94, 255);
    public static readonly Color DarkSlate = new(15, 23, 42, 255);
    public static readonly Color TerminalBg = new(10, 15, 24, 255);
    public static readonly Color BorderColor = new(51, 65, 85, 255);
    public static readonly Color CardBg = new(13, 21, 34, 255);
    public static readonly Color OrangeHazard = new(249, 115, 22, 255);
}
