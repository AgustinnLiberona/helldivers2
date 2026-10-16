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
        "gunship",
        "hound",
        "butcher",
        "warlord",
        "watcher",
        "outcast",
        "tripod",
        "illusionist",
        "council_member"
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
        else if (faction == Faction.Automatons)
        {
            return type switch
            {
                EnemyType.Trooper => "trooper",
                EnemyType.Commissar => "commissar",
                EnemyType.Berserker => "berserker",
                EnemyType.Devastator => "rocket_devastator",
                EnemyType.Hulk => "hulk",
                EnemyType.FactoryStrider => "factory_strider",
                EnemyType.Hound => "hound",
                EnemyType.Butcher => "butcher",
                EnemyType.Warlord => "warlord",
                _ => "trooper"
            };
        }
        else // The Illuminate
        {
            return type switch
            {
                EnemyType.Watcher => "watcher",
                EnemyType.Outcast => "outcast",
                EnemyType.Tripod => "tripod",
                EnemyType.Illusionist => "illusionist",
                EnemyType.CouncilMember => "council_member",
                _ => "watcher"
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

    /// <summary>
    /// Escala y centra proporcionalmente cualquier sprite de enemigo dentro de un rectángulo delimitador (bounds),
    /// asegurando que el diseño completo se visualice sin desbordar ni cortarse con bordes de interfaz.
    /// </summary>
    public static void DrawSpriteFit(string key, Rectangle bounds, Color tint, bool flipH = false, float padding = 4f)
    {
        if (!_textures.TryGetValue(key, out var tex) || tex.Id == 0)
        {
            var entry = BestiaryRegistry.Entries.FirstOrDefault(b => b.SpriteKey == key || b.Id == key);
            if (entry != null)
            {
                Vector2 center = new(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
                float maxDim = MathF.Min(bounds.Width, bounds.Height) - padding * 2f;
                float dummyRadius = entry.EnemyType switch
                {
                    EnemyType.Titan or EnemyType.FactoryStrider or EnemyType.Warlord or EnemyType.CouncilMember => 38f,
                    EnemyType.Charger or EnemyType.Hulk or EnemyType.Tripod or EnemyType.Illusionist => 26f,
                    _ => 16f
                };
                float previewScale = maxDim / (dummyRadius * 2.3f);
                float time = (float)Raylib.GetTime();
                GameRenderer.DrawEnemyPreview(entry.EnemyType, entry.Faction, center, previewScale, flipH ? MathF.PI : 0f, time * 2.5f);
            }
            return;
        }

        float maxW = MathF.Max(1f, bounds.Width - padding * 2f);
        float maxH = MathF.Max(1f, bounds.Height - padding * 2f);
        float scale = MathF.Min(maxW / tex.Width, maxH / tex.Height);
        float drawW = tex.Width * scale;
        float drawH = tex.Height * scale;

        Vector2 centerTex = new(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Rectangle src = new(0, 0, tex.Width * (flipH ? -1 : 1), tex.Height);
        Rectangle dest = new(centerTex.X, centerTex.Y, drawW, drawH);
        Vector2 origin = new(drawW / 2f, drawH / 2f);

        Raylib.DrawTexturePro(tex, src, dest, origin, 0f, tint);
    }

    public static void DrawInGameEnemy(Enemy e)
    {
        string key = GetSpriteKeyForEnemy(e);
        if (!_textures.TryGetValue(key, out var tex) || tex.Id == 0)
        {
            if (e.Faction == Faction.Terminids)
                GameRenderer.DrawTerminid(e);
            else if (e.Faction == Faction.Illuminate)
                GameRenderer.DrawIlluminate(e);
            else
                GameRenderer.DrawAutomaton(e);
            return;
        }

        float r = e.Radius;
        Vector2 pos = e.Position;
        float time = (float)Raylib.GetTime();

        // 1. CICLO DE CAMINATA CINEMÁTICO (Walk Cycle) MULTI-CAPA A 144 FPS
        float animPhase = time * (e.Speed * 0.12f * 60f) + e.Id * 1.7f;
        float wobble = MathF.Sin(animPhase) * 10f;
        float scaleX = 1.0f + MathF.Sin(animPhase * 2f) * 0.10f;
        float scaleY = 1.0f - MathF.Sin(animPhase * 2f) * 0.10f;
        float strideHop = MathF.Abs(MathF.Sin(animPhase)) * -6f;

        // Ataque: Lunge de 12px hacia adelante y estiramiento
        Vector2 forward = new(MathF.Cos(e.FacingAngle), MathF.Sin(e.FacingAngle));
        Vector2 lungeVec = forward * e.AttackLunge;
        if (e.AttackLunge > 0f)
        {
            scaleX *= (1.0f + e.AttackLunge * 0.035f);
        }

        Vector2 drawPos = pos + new Vector2(0, strideHop) + lungeVec;
        bool flipH = MathF.Cos(e.FacingAngle) < 0;
        float tiltDeg = Math.Clamp(MathF.Sin(e.FacingAngle) * 14f, -14f, 14f) + wobble;

        // Sombra dinámica debajo (se achica al saltar y se agranda al pisar)
        float shadowScale = 1.0f - MathF.Abs(MathF.Sin(animPhase)) * 0.28f;
        Color shadowCol = new(0, 0, 0, (int)(110 * shadowScale));
        Raylib.DrawEllipse((int)pos.X, (int)(pos.Y + r * 0.72f), (int)(r * 1.15f * shadowScale), (int)(r * 0.55f * shadowScale), shadowCol);

        // 2. DIBUJO DEL SPRITE PRINCIPAL CON ANIMACIÓN PROCEDURAL (SQUASH & STRETCH + WOBBLE + SALTO)
        float spriteHeight = r * 2.55f * scaleY;
        float aspect = (float)tex.Width / tex.Height;
        float spriteWidth = (r * 2.55f * aspect) * scaleX;

        Rectangle src = new(0, 0, tex.Width * (flipH ? -1 : 1), tex.Height);
        Rectangle dest = new(drawPos.X, drawPos.Y, spriteWidth, spriteHeight);
        Vector2 origin = new(spriteWidth / 2f, spriteHeight / 2f);

        // HitFlash blanco puro durante impacto
        bool isHit = e.HitFlashTimer > 0f;
        Color tint = isHit ? new Color(255, 230, 230, 255) : Color.White;
        Raylib.DrawTexturePro(tex, src, dest, origin, tiltDeg, tint);

        if (isHit)
        {
            // Silueta blanca aditiva brillante para feedback visual contundente
            Raylib.BeginBlendMode(BlendMode.Additive);
            Raylib.DrawTexturePro(tex, src, dest, origin, tiltDeg, new Color(255, 255, 255, 175));
            Raylib.EndBlendMode();
        }

        // 3. DETALLES Y ACCESORIOS DE ÉLITE (POST-SPRITE)
        if (e.Faction == Faction.Terminids)
        {
            // Baba verde ácida goteando al atacar o a corta distancia
            if (e.InAttackRange || e.Type == EnemyType.Spitter)
            {
                Vector2 mouthPos = drawPos + forward * (r * 0.85f);
                float dropY = (time * 75f + e.Id * 10f) % 18f;
                Vector2 dropPos = mouthPos + new Vector2(0, dropY);
                Raylib.DrawRectangle((int)dropPos.X - 1, (int)dropPos.Y, 2, 4, ProceduralAssetRenderer.TerminidBile);
            }
        }
        else if (e.Faction == Faction.Automatons) // AUTÓMATAS
        {
            // Berserker / Butcher: Sierras o machetes giratorios
            bool isBerserker = e.Type == EnemyType.Berserker || key.Contains("berserker") || e.Type == EnemyType.Butcher;
            if (isBerserker)
            {
                float sawRot = time * 720f;
                float sawRadius = r * 0.65f;
                Vector2 sawL = drawPos + new Vector2(-r * 0.95f, -2f);
                Vector2 sawR = drawPos + new Vector2(r * 0.95f, -2f);

                // Disco de sierra con dientes pixelados
                Raylib.DrawPoly(sawL, 8, sawRadius, sawRot, new Color(80, 85, 95, 255));
                Raylib.DrawPoly(sawL, 8, sawRadius * 0.7f, -sawRot, ProceduralAssetRenderer.GunmetalLight);
                Raylib.DrawCircleV(sawL, 3f, ProceduralAssetRenderer.AutomatonRed);

                Raylib.DrawPoly(sawR, 8, sawRadius, -sawRot, new Color(80, 85, 95, 255));
                Raylib.DrawPoly(sawR, 8, sawRadius * 0.7f, sawRot, ProceduralAssetRenderer.GunmetalLight);
                Raylib.DrawCircleV(sawR, 3f, ProceduralAssetRenderer.AutomatonRed);

                // Chispas de fricción constantes
                for (int sp = 0; sp < 3; sp++)
                {
                    float spAng = (time * 18f + sp * 2.1f);
                    Vector2 spOffset = new(MathF.Cos(spAng) * (sawRadius + 4f), MathF.Sin(spAng) * 6f);
                    Raylib.DrawRectangle((int)(sawL.X + spOffset.X), (int)(sawL.Y + spOffset.Y), 2, 2, Constants.OrangeHazard);
                    Raylib.DrawRectangle((int)(sawR.X - spOffset.X), (int)(sawR.Y + spOffset.Y), 2, 2, ProceduralAssetRenderer.SuperEarthYellow);
                }
            }

            // Hulks, Devastators, Troopers, Warlord: Visor ocular rojo neón con halo aditivo y estela
            Vector2 eyePos = drawPos + new Vector2(flipH ? -r * 0.32f : r * 0.32f, -r * 0.38f);
            float eyeBlink = 0.85f + 0.15f * MathF.Sin(time * 16f + e.Id);

            Raylib.BeginBlendMode(BlendMode.Additive);
            // Halo rojo exterior
            Raylib.DrawCircleV(eyePos, 7.5f * eyeBlink, new Color(255, 30, 30, (int)(110 * eyeBlink)));
            // Núcleo de alta luminosidad
            Raylib.DrawCircleV(eyePos, 3.2f, new Color(255, 120, 120, 255));
            Raylib.DrawCircleV(eyePos, 1.4f, Color.White);
            // Pequeña estela de movimiento
            Raylib.DrawCircleV(eyePos - forward * 5f, 2.4f, new Color(255, 30, 30, 50));
            Raylib.EndBlendMode();
        }
        else // THE ILLUMINATE (LOS ILUMINADOS)
        {
            // Ojo psiónico magenta/violeta o filos de energía
            Vector2 eyePos = drawPos + new Vector2(flipH ? -r * 0.25f : r * 0.25f, -r * 0.30f);
            float psiBlink = 0.85f + 0.15f * MathF.Sin(time * 14f + e.Id);

            Raylib.BeginBlendMode(BlendMode.Additive);
            Raylib.DrawCircleV(eyePos, 8f * psiBlink, new Color(168, 85, 247, (int)(130 * psiBlink)));
            Raylib.DrawCircleV(eyePos, 4f, Constants.IlluminateMagenta);
            Raylib.DrawCircleV(eyePos, 1.8f, Color.White);

            // Outcast: Espadas psiónicas gemelas
            if (e.Type == EnemyType.Outcast)
            {
                Vector2 hiltL = drawPos + new Vector2(-r * 0.7f, 4f);
                Vector2 hiltR = drawPos + new Vector2(r * 0.7f, 4f);
                Raylib.DrawLineEx(hiltL, hiltL + forward * 22f, 3f, Constants.IlluminateMagenta);
                Raylib.DrawLineEx(hiltR, hiltR + forward * 22f, 3f, Constants.IlluminateMagenta);
            }
            Raylib.EndBlendMode();
        }

        // Special in-game effects para Charger & Hulk
        if (e.Type == EnemyType.Charger && e.Charging)
        {
            float trailDir = flipH ? 1f : -1f;
            Raylib.DrawLineEx(pos + new Vector2(trailDir * 18f, -12f), pos + new Vector2(trailDir * 42f, -12f), 3f, Constants.OrangeHazard);
            Raylib.DrawLineEx(pos + new Vector2(trailDir * 18f,  12f), pos + new Vector2(trailDir * 42f,  12f), 3f, Constants.OrangeHazard);
        }
        else if (e.Type == EnemyType.Hulk)
        {
            Vector2 nozzOff = flipH ? new Vector2(-r * 1.1f, 4f) : new Vector2(r * 1.1f, 4f);
            float pulse = 5f + MathF.Sin(e.WalkCycle * 4f) * 2f;
            Raylib.DrawCircleV(pos + nozzOff, pulse, new Color(249, 115, 22, 120));
        }

        // Health bar & HVT badge & Energy Shield
        DrawEnemyOverlays(e);
    }

    private static void DrawEnemyOverlays(Enemy e)
    {
        // Resplandor del Escudo de Energía Esférico de los Iluminados
        if (e.ShieldHp > 0f && e.MaxShieldHp > 0f)
        {
            float sRatio = Math.Clamp(e.ShieldHp / e.MaxShieldHp, 0f, 1f);
            float shieldR = e.Radius * 1.4f + MathF.Sin((float)Raylib.GetTime() * 8f + e.Id) * 2f;
            byte alpha = (byte)(80 + sRatio * 110);
            Raylib.DrawCircleLines((int)e.Position.X, (int)e.Position.Y, shieldR, new Color(168, 85, 247, (int)alpha));
            Raylib.DrawCircleLines((int)e.Position.X, (int)e.Position.Y, shieldR - 2f, new Color(34, 211, 238, (int)(alpha * 0.7f)));
            Raylib.DrawCircleV(e.Position, shieldR, new Color(168, 85, 247, (int)(alpha * 0.14f)));
        }

        if (e.Hp < e.MaxHp || (e.MaxShieldHp > 0 && e.ShieldHp < e.MaxShieldHp) || e.IsHvt)
        {
            float barW = Math.Clamp(e.Radius * 1.6f, 28f, 60f);
            float barH = e.IsHvt ? 5f : 3.5f;
            float barX = e.Position.X - barW / 2f;
            float barY = e.Position.Y - e.Radius * 1.35f - (e.IsHvt ? 16f : 10f);

            // Shield Bar (si la unidad tiene escudos)
            if (e.MaxShieldHp > 0f && e.ShieldHp > 0f)
            {
                float sPct = Math.Clamp(e.ShieldHp / e.MaxShieldHp, 0f, 1f);
                Raylib.DrawRectangle((int)barX, (int)barY - 4, (int)(barW * sPct), 3, Constants.IlluminateCyan);
                Raylib.DrawRectangleLines((int)barX, (int)barY - 4, (int)barW, 3, Constants.IlluminateViolet);
            }

            // Background
            Raylib.DrawRectangle((int)barX, (int)barY, (int)barW, (int)barH, new Color(15, 23, 42, 220));

            // Health Fill
            float hpPct = Math.Clamp(e.Hp / e.MaxHp, 0f, 1f);
            Color fillCol = e.IsHvt ? Constants.Gold : (e.Faction == Faction.Terminids ? Constants.OrangeHazard : (e.Faction == Faction.Illuminate ? Constants.IlluminateMagenta : Constants.DemocracyRed));
            Raylib.DrawRectangle((int)barX, (int)barY, (int)(barW * hpPct), (int)barH, fillCol);

            // Border
            Raylib.DrawRectangleLines((int)barX, (int)barY, (int)barW, (int)barH, e.IsHvt ? Constants.Gold : new Color(60, 75, 95, 220));

            // HVT Badge
            if (e.IsHvt)
            {
                string tag = "★ OBJETIVO";
                int tagW = Raylib.MeasureText(tag, 9);
                Raylib.DrawText(tag, (int)e.Position.X - tagW / 2, (int)(barY - (e.MaxShieldHp > 0 ? 17f : 12f)), 9, Constants.Gold);
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
