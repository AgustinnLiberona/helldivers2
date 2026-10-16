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
    Automatons,
    Illuminate
}

public enum MissionType
{
    Outposts,
    Icbm,
    Seismic,
    Assassinate,
    EradicateHive,
    ExtractSamples,
    HoldTheLine,
    SabotageFactory
}

public enum EnemyType
{
    // Terminids
    Scavenger,
    Hunter,
    Warrior,
    Spitter,
    Charger,
    Titan,

    // Automatons & Cyborgs
    Trooper,
    Commissar,
    Berserker,
    Devastator,
    Hulk,
    FactoryStrider,
    Hound,
    Butcher,
    Warlord,

    // The Illuminate
    Watcher,
    Outcast,
    Tripod,
    Illusionist,
    CouncilMember
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
    Rocket,
    Railgun,
    Autocannon,
    QuasarPlasma,
    PsionicOrb,
    PsionicBeam
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
    public static readonly Color TacticalCyan = new(0, 228, 255, 255);
    public static readonly Color FlameOrange = new(255, 140, 20, 255);
    public static readonly Color AutomatonRed = new(255, 45, 45, 255);
    public static readonly Color GunmetalLight = new(78, 86, 98, 255);

    // Illuminate & Cyborg Palette
    public static readonly Color IlluminateViolet = new(168, 85, 247, 255);
    public static readonly Color IlluminateMagenta = new(217, 70, 239, 255);
    public static readonly Color IlluminateCyan = new(34, 211, 238, 255);
    public static readonly Color CyborgRust = new(180, 83, 9, 255);
}
