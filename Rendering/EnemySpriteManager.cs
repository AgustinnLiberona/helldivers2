using System.Numerics;
using Raylib_cs;
using HelldiversDesktop.Models;

namespace HelldiversDesktop.Rendering;

public static class EnemySpriteManager
{
    private static readonly Dictionary<string, Texture2D> _textures = new();
    private static bool _initialized = false;

    private static readonly string[] AllSpriteKeys = new[]
    {
        "scavenger",
        "pouncer",
        "warrior",
        "hunter",
        "stalker",
        "hive_guard",
        "brood_commander",
        "bile_spewer",
        "nursing_spewer",
        "bile_spewer_puke",
        "charger",
        "bile_titan",
        "impaler",
        "shrieker",
        "trooper",
        "raider",
        "commissar",
        "berserker",
        "berserker_alt",
        "devastator",
        "heavy_devastator",
        "rocket_devastator",
        "scout_strider",
        "hulk",
        "tank",
        "dropship",
        "factory_strider",
        "gunship"
    };

    public static void Init()
    {
        if (_initialized) return;

        string[] searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Enemies"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Enemies"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "Enemies"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Assets", "Enemies")
        };

        string assetsDir = "";
        foreach (var p in searchPaths)
        {
            if (Directory.Exists(p) && File.Exists(Path.Combine(p, "scavenger.png")))
            {
                assetsDir = Path.GetFullPath(p);
                break;
            }
        }

        if (!string.IsNullOrEmpty(assetsDir))
        {
            foreach (var key in AllSpriteKeys)
            {
                string filePath = Path.Combine(assetsDir, $"{key}.png");
                if (File.Exists(filePath))
                {
                    Texture2D tex = Raylib.LoadTexture(filePath);
                    Raylib.SetTextureFilter(tex, TextureFilter.Point);
                    _textures[key] = tex;
                }
            }
        }

        _initialized = true;
    }

    public static bool HasTexture(string key)
    {
        return _textures.TryGetValue(key, out var tex) && tex.Id != 0;
    }

    public static Texture2D? GetTexture(string key)
    {
        if (_textures.TryGetValue(key, out var tex))
            return tex;
        return null;
    }

    public static string GetSpriteKeyForEnemy(EnemyType type, Faction faction)
    {
        if (faction == Faction.Terminids)
        {
            return type switch
            {
                EnemyType.Scavenger => "scavenger",
                EnemyType.Hunter => "hunter",
                EnemyType.Warrior => "warrior",
                EnemyType.Spitter => "bile_spewer",
                EnemyType.Charger => "charger",
                EnemyType.Titan => "bile_titan",
                _ => "warrior"
            };
        }
        else
        {
            return type switch
            {
                EnemyType.Trooper => "trooper",
                EnemyType.Commissar => "commissar",
                EnemyType.Berserker => "berserker",
                EnemyType.Devastator => "rocket_devastator",
                EnemyType.Hulk => "hulk",
                EnemyType.FactoryStrider => "factory_strider",
                _ => "trooper"
            };
        }
    }

    public static string GetSpriteKeyForEnemy(Enemy e)
    {
        if (!string.IsNullOrEmpty(e.SpriteKey))
            return e.SpriteKey;
        return GetSpriteKeyForEnemy(e.Type, e.Faction);
    }

    public static void DrawSpriteCentered(string key, Vector2 center, float targetHeight, Color tint, bool flipH = false)
    {
        if (!_textures.TryGetValue(key, out var tex) || tex.Id == 0) return;

        float aspect = (float)tex.Width / tex.Height;
        float targetWidth = targetHeight * aspect;

        Rectangle src = new(0, 0, tex.Width * (flipH ? -1 : 1), tex.Height);
        Rectangle dest = new(center.X, center.Y, targetWidth, targetHeight);
        Vector2 origin = new(targetWidth / 2f, targetHeight / 2f);

        Raylib.DrawTexturePro(tex, src, dest, origin, 0f, tint);
    }

    public static void DrawInGameEnemy(Enemy e)
    {
        string key = GetSpriteKeyForEnemy(e);
        if (!_textures.TryGetValue(key, out var tex) || tex.Id == 0)
        {
            return;
        }

        float r = e.Radius;
        Vector2 pos = e.Position;

        // Ground shadow
        Color shadowCol = new(0, 0, 0, 95);
        Raylib.DrawEllipse((int)pos.X, (int)(pos.Y + r * 0.72f), (int)(r * 1.15f), (int)(r * 0.55f), shadowCol);

        // Walking vertical bob
        float bob = MathF.Sin(e.WalkCycle * 1.6f) * 2.5f;
        Vector2 drawPos = pos + new Vector2(0, bob);

        // Facing direction: facing left if cos < 0, else right
        bool flipH = MathF.Cos(e.FacingAngle) < 0;

        // Subtle angle tilt in the direction of movement
        float tiltDeg = Math.Clamp(MathF.Sin(e.FacingAngle) * 12f, -12f, 12f);

        // Draw in-game sprite
        float spriteHeight = r * 2.55f;
        float aspect = (float)tex.Width / tex.Height;
        float spriteWidth = spriteHeight * aspect;

        Rectangle src = new(0, 0, tex.Width * (flipH ? -1 : 1), tex.Height);
        Rectangle dest = new(drawPos.X, drawPos.Y, spriteWidth, spriteHeight);
        Vector2 origin = new(spriteWidth / 2f, spriteHeight / 2f);

        // Flash red/white on hit
        Color tint = e.HitFlashTimer > 0 ? new Color(255, 120, 120, 255) : Color.White;

        Raylib.DrawTexturePro(tex, src, dest, origin, tiltDeg, tint);

        // Special in-game effects matching the reference image:
        if (e.Type == EnemyType.Charger && e.Charging)
        {
            // Kinetic charge shockwave particles
            float trailDir = flipH ? 1f : -1f;
            Raylib.DrawLineEx(pos + new Vector2(trailDir * 18f, -12f), pos + new Vector2(trailDir * 42f, -12f), 3f, Constants.OrangeHazard);
            Raylib.DrawLineEx(pos + new Vector2(trailDir * 18f,  12f), pos + new Vector2(trailDir * 42f,  12f), 3f, Constants.OrangeHazard);
        }
        else if (e.Type == EnemyType.Hulk)
        {
            // Flamethrower heat aura
            Vector2 nozzOff = flipH ? new Vector2(-r * 1.1f, 4f) : new Vector2(r * 1.1f, 4f);
            float pulse = 5f + MathF.Sin(e.WalkCycle * 4f) * 2f;
            Raylib.DrawCircleV(pos + nozzOff, pulse, new Color(249, 115, 22, 120));
        }

        // Health bar & HVT badge
        DrawEnemyOverlays(e);
    }

    private static void DrawEnemyOverlays(Enemy e)
    {
        if (e.Hp < e.MaxHp || e.IsHvt)
        {
            float barW = Math.Clamp(e.Radius * 1.6f, 28f, 60f);
            float barH = e.IsHvt ? 5f : 3.5f;
            float barX = e.Position.X - barW / 2f;
            float barY = e.Position.Y - e.Radius * 1.35f - (e.IsHvt ? 16f : 10f);

            // Background
            Raylib.DrawRectangle((int)barX, (int)barY, (int)barW, (int)barH, new Color(15, 23, 42, 220));

            // Health Fill
            float hpPct = Math.Clamp(e.Hp / e.MaxHp, 0f, 1f);
            Color fillCol = e.IsHvt ? Constants.Gold : (e.Faction == Faction.Terminids ? Constants.OrangeHazard : Constants.DemocracyRed);
            Raylib.DrawRectangle((int)barX, (int)barY, (int)(barW * hpPct), (int)barH, fillCol);

            // Border
            Raylib.DrawRectangleLines((int)barX, (int)barY, (int)barW, (int)barH, e.IsHvt ? Constants.Gold : new Color(60, 75, 95, 220));

            // HVT Badge
            if (e.IsHvt)
            {
                string tag = "★ OBJETIVO";
                int tagW = Raylib.MeasureText(tag, 9);
                Raylib.DrawText(tag, (int)e.Position.X - tagW / 2, (int)barY - 11, 9, Constants.Gold);
            }
        }
    }

    public static void Close()
    {
        foreach (var tex in _textures.Values)
        {
            if (tex.Id != 0)
                Raylib.UnloadTexture(tex);
        }
        _textures.Clear();
        _initialized = false;
    }
}
