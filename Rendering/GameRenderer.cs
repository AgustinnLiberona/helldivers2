using System.Numerics;
using Raylib_cs;
using HelldiversDesktop.Models;

namespace HelldiversDesktop.Rendering;

public static class GameRenderer
{
    // ─── Palette shortcuts ──────────────────────────────────────────
    private static readonly Color Slate900  = new(15, 23, 42, 255);
    private static readonly Color Slate800  = new(30, 41, 59, 255);
    private static readonly Color Slate700  = new(51, 65, 85, 255);
    private static readonly Color Slate600  = new(71, 85, 105, 255);
    private static readonly Color Stone900  = new(28, 25, 23, 255);
    private static readonly Color Stone800  = new(41, 37, 36, 255);
    private static readonly Color Stone700  = new(68, 64, 60, 255);
    private static readonly Color Amber600  = new(217, 119, 6, 255);
    private static readonly Color Amber500  = new(245, 158, 11, 255);
    private static readonly Color Lime500   = new(132, 204, 22, 255);
    private static readonly Color Lime700   = new(77, 124, 15, 255);
    private static readonly Color Emerald800= new(6, 78, 59, 255);
    private static readonly Color Emerald600= new(5, 150, 105, 255);
    private static readonly Color Emerald400= new(52, 211, 153, 255);
    private static readonly Color Red700    = new(185, 28, 28, 255);
    private static readonly Color Red500    = new(239, 68, 68, 255);
    private static readonly Color Orange500 = new(249, 115, 22, 255);
    private static readonly Color ShadowCol = new(0, 0, 0, 90);

    // ─── WORLD ──────────────────────────────────────────────────────
    public static void DrawWorld(Camera2D camera, Campaign campaign, List<Vector2> terrainRocks,
        Vector2 extractionPos, bool extractionActive, bool pelicanLanded, float extractionTimer, Player player)
    {
        Color bgColor = campaign.Planet.Biome switch
        {
            "creek"    => new Color(6, 13, 16, 255),
            "hellmire" => new Color(23, 9, 6, 255),
            "crimsika" => new Color(17, 11, 8, 255),
            _          => new Color(10, 16, 27, 255)
        };
        Raylib.DrawRectangle(0, 0, (int)Constants.WorldSize, (int)Constants.WorldSize, bgColor);

        // Subtle tactical grid
        Color gridColor = campaign.Planet.Faction == Faction.Automatons
            ? new Color(56, 189, 248, 18) : new Color(239, 68, 68, 18);
        const int gs = 120;
        for (int x = 0; x < Constants.WorldSize; x += gs)
            Raylib.DrawLine(x, 0, x, (int)Constants.WorldSize, gridColor);
        for (int y = 0; y < Constants.WorldSize; y += gs)
            Raylib.DrawLine(0, y, (int)Constants.WorldSize, y, gridColor);

        // World border
        Color borderColor = campaign.Planet.Faction == Faction.Automatons ? Constants.Cyan : Constants.DemocracyRed;
        Raylib.DrawRectangleLinesEx(new Rectangle(0, 0, Constants.WorldSize, Constants.WorldSize), 6, borderColor);

        // Terrain rocks with inner highlight
        foreach (var rock in terrainRocks)
        {
            Raylib.DrawEllipse((int)rock.X, (int)(rock.Y + 4), 24, 14, ShadowCol);
            Raylib.DrawCircleV(rock, 22f, Stone900);
            Raylib.DrawCircleV(rock + new Vector2(-4, -4), 14f, new Color(38, 35, 33, 255));
            Raylib.DrawCircleLines((int)rock.X, (int)rock.Y, 22f, Stone800);
        }

        // Objectives: Outposts
        foreach (var outP in campaign.Outposts)
        {
            if (campaign.Planet.Faction == Faction.Terminids)
            {
                // Terminid nest – organic ring
                Color c = outP.Destroyed ? new Color(17, 24, 39, 200) : new Color(41, 18, 6, 255);
                Raylib.DrawCircleV(outP.Position, outP.Radius + 6, new Color(0, 0, 0, 60));
                Raylib.DrawCircleV(outP.Position, outP.Radius, c);
                if (!outP.Destroyed)
                {
                    Raylib.DrawCircleV(outP.Position, outP.Radius * 0.55f, Constants.OrangeHazard);
                    Raylib.DrawCircleV(outP.Position, outP.Radius * 0.3f, new Color(120, 53, 15, 255));
                    Raylib.DrawCircleLines((int)outP.Position.X, (int)outP.Position.Y, outP.Radius, Amber600);
                    Raylib.DrawCircleLines((int)outP.Position.X, (int)outP.Position.Y, outP.Radius * 0.55f, new Color(180, 83, 9, 255));
                }
                else
                {
                    Raylib.DrawCircleLines((int)outP.Position.X, (int)outP.Position.Y, outP.Radius, new Color(50, 50, 50, 255));
                }
            }
            else
            {
                // Automaton factory – angular fortification
                float hw = outP.Radius * 0.85f;
                Rectangle r = new(outP.Position.X - hw, outP.Position.Y - hw, hw * 2, hw * 2);
                Raylib.DrawRectangleRec(r, outP.Destroyed ? Slate800 : Slate900);
                Raylib.DrawRectangleLinesEx(r, outP.Destroyed ? 2 : 3, outP.Destroyed ? new Color(50, 50, 50, 255) : Red500);
                if (!outP.Destroyed)
                {
                    Raylib.DrawRectangle((int)outP.Position.X - 14, (int)outP.Position.Y - 6, 28, 12, Red700);
                    Raylib.DrawRectangleLines((int)outP.Position.X - 14, (int)outP.Position.Y - 6, 28, 12, Red500);
                }
            }
        }

        // Objectives: Silos / Probes
        foreach (var it in campaign.Interactables)
        {
            Color borderC = it.Completed ? Constants.LibertyGreen : (it.Active ? Constants.Cyan : Constants.Gold);
            Raylib.DrawCircleV(it.Position, it.Radius, it.Completed ? new Color(30, 41, 59, 180) : (it.Active ? new Color(56, 189, 248, 40) : new Color(17, 24, 39, 200)));
            Raylib.DrawCircleLines((int)it.Position.X, (int)it.Position.Y, it.Radius, borderC);

            if (it.Type == "icbm")
            {
                Raylib.DrawRectangle((int)it.Position.X - 10, (int)it.Position.Y - 30, 20, 56, Color.Gray);
                Raylib.DrawRectangleLines((int)it.Position.X - 10, (int)it.Position.Y - 30, 20, 56, Color.DarkGray);
                Raylib.DrawTriangle(
                    new Vector2(it.Position.X - 10, it.Position.Y - 30),
                    new Vector2(it.Position.X + 10, it.Position.Y - 30),
                    new Vector2(it.Position.X, it.Position.Y - 46),
                    it.Completed ? Constants.LibertyGreen : Constants.Gold);
                string pctTxt = it.Completed ? "OK" : $"{Math.Round((it.Progress / it.MaxProgress) * 100)}%";
                Raylib.DrawText(pctTxt, (int)it.Position.X - Raylib.MeasureText(pctTxt, 14) / 2, (int)it.Position.Y - 6, 14, Color.White);
            }
            else
            {
                string label = it.Completed ? "SONDA OK" : $"SONDA {Math.Round((it.Progress / it.MaxProgress) * 100)}%";
                Raylib.DrawText(label, (int)it.Position.X - Raylib.MeasureText(label, 12) / 2, (int)it.Position.Y - 6, 12, Constants.Gold);
            }
        }

        // Extraction Zone & 350m Perimeter
        if (extractionActive)
        {
            float waitRadius = 350f;
            Color perimColor = pelicanLanded ? new Color(34, 197, 94, 100) : new Color(56, 189, 248, 100);
            Raylib.DrawCircleLines((int)extractionPos.X, (int)extractionPos.Y, waitRadius, perimColor);
            // Dashed secondary perimeter
            for (float a = 0; a < MathF.PI * 2; a += 0.12f)
            {
                var p1 = extractionPos + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (waitRadius - 8);
                var p2 = extractionPos + new Vector2(MathF.Cos(a + 0.05f), MathF.Sin(a + 0.05f)) * (waitRadius - 8);
                Raylib.DrawLineEx(p1, p2, 1.5f, new Color((byte)perimColor.R, (byte)perimColor.G, (byte)perimColor.B, (byte)50));
            }
        }

        // Landing Pad – concentric rings
        Raylib.DrawCircleV(extractionPos, 68f, new Color(0, 0, 0, 50));
        Raylib.DrawCircleV(extractionPos, 65f, extractionActive ? new Color(15, 23, 42, 200) : new Color(255, 215, 0, 10));
        Raylib.DrawCircleLines((int)extractionPos.X, (int)extractionPos.Y, 65f, extractionActive ? Constants.Gold : new Color(80, 80, 80, 255));
        Raylib.DrawCircleLines((int)extractionPos.X, (int)extractionPos.Y, 45f, extractionActive ? new Color(255, 215, 0, 60) : new Color(60, 60, 60, 255));
        // H marker
        int hx = (int)extractionPos.X, hy = (int)extractionPos.Y;
        Color hc = extractionActive ? Constants.Gold : new Color(100, 100, 100, 255);
        Raylib.DrawRectangle(hx - 12, hy - 14, 4, 28, hc);
        Raylib.DrawRectangle(hx + 8,  hy - 14, 4, 28, hc);
        Raylib.DrawRectangle(hx - 8,  hy - 2,  20, 4, hc);

        // Pelican-1 Gunship
        if (pelicanLanded) DrawPelican(extractionPos);
    }

    private static void DrawPelican(Vector2 pos)
    {
        // Ground shadow
        Raylib.DrawEllipse((int)pos.X, (int)pos.Y + 8, 56, 36, new Color(0, 0, 0, 120));

        // Swept wings
        Raylib.DrawTriangle(
            new Vector2(pos.X - 64, pos.Y + 12),
            new Vector2(pos.X + 64, pos.Y + 12),
            new Vector2(pos.X, pos.Y - 16), Slate800);
        Raylib.DrawTriangle(
            new Vector2(pos.X - 62, pos.Y + 10),
            new Vector2(pos.X + 62, pos.Y + 10),
            new Vector2(pos.X, pos.Y - 14), Slate900);
        // Engine nacelles
        Raylib.DrawRectangle((int)pos.X - 60, (int)pos.Y + 4, 10, 10, Slate700);
        Raylib.DrawRectangle((int)pos.X + 50, (int)pos.Y + 4, 10, 10, Slate700);
        Raylib.DrawRectangle((int)pos.X - 58, (int)pos.Y + 6, 6, 6, Constants.Cyan);
        Raylib.DrawRectangle((int)pos.X + 52, (int)pos.Y + 6, 6, 6, Constants.Cyan);

        // Fuselage
        Raylib.DrawRectangle((int)pos.X - 18, (int)pos.Y - 40, 36, 62, Slate900);
        Raylib.DrawRectangleLinesEx(new Rectangle(pos.X - 18, pos.Y - 40, 36, 62), 1, Slate700);
        // Nose cone
        Raylib.DrawTriangle(
            new Vector2(pos.X - 18, pos.Y - 40),
            new Vector2(pos.X + 18, pos.Y - 40),
            new Vector2(pos.X, pos.Y - 54), Slate900);
        // Cockpit canopy
        Raylib.DrawTriangle(
            new Vector2(pos.X - 8, pos.Y - 34),
            new Vector2(pos.X + 8, pos.Y - 34),
            new Vector2(pos.X, pos.Y - 48), Constants.Gold);
        // Hull stripe
        Raylib.DrawRectangle((int)pos.X - 16, (int)pos.Y - 10, 32, 3, Constants.Gold);
        // Label
        Raylib.DrawText("SE", (int)pos.X - 6, (int)pos.Y - 6, 10, Constants.Gold);

        // Rear boarding ramp
        Raylib.DrawRectangle((int)pos.X - 14, (int)pos.Y + 22, 28, 20, Amber500);
        Raylib.DrawRectangleLinesEx(new Rectangle(pos.X - 14, pos.Y + 22, 28, 20), 2, Constants.LibertyGreen);
        // Chevrons on ramp
        Raylib.DrawText("▲", (int)pos.X - 5, (int)pos.Y + 26, 12, Constants.LibertyGreen);
    }

    // ─── TERMINID ENEMIES ───────────────────────────────────────────

    public static void DrawTerminid(Enemy e)
    {
        string spriteKey = EnemySpriteManager.GetSpriteKeyForEnemy(e);
        if (EnemySpriteManager.HasTexture(spriteKey))
        {
            EnemySpriteManager.DrawInGameEnemy(e);
            return;
        }

        Vector2 pos = e.Position;
        float angle = e.FacingAngle;
        float r = e.Radius;

        Rlgl.PushMatrix();
        Rlgl.Translatef(pos.X, pos.Y, 0);
        Rlgl.Rotatef(angle * (180f / MathF.PI), 0, 0, 1);

        // Ground shadow
        Raylib.DrawEllipse(0, 5, (int)(r * 1.1f), (int)(r * 0.6f), ShadowCol);

        if (e.Type == EnemyType.Charger || (e.IsHvt && e.Faction == Faction.Terminids))
            DrawCharger(e, r);
        else if (e.Type == EnemyType.Titan)
            DrawBileTitan(e, r);
        else if (e.Type == EnemyType.Hunter)
            DrawHunter(e, r);
        else if (e.Type == EnemyType.Spitter)
            DrawBileSpewer(e, r);
        else if (e.Type == EnemyType.Warrior)
            DrawWarrior(e, r);
        else
            DrawScavenger(e, r);

        Rlgl.PopMatrix();

        DrawEnemyOverlays(e);
    }

    private static void DrawEnemyOverlays(Enemy e)
    {
        // Health bar & HVT indicator above enemy
        if (e.Hp < e.MaxHp || e.IsHvt)
        {
            float barW = Math.Clamp(e.Radius * 1.5f, 26f, 56f);
            float barH = e.IsHvt ? 5f : 3.5f;
            float barX = e.Position.X - barW / 2f;
            float barY = e.Position.Y - e.Radius - (e.IsHvt ? 16f : 11f);

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

    private static void DrawCharger(Enemy e, float r)
    {
        // 6 segmented arthropod legs
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 3; i++)
            {
                float swing = MathF.Sin(e.WalkCycle + i * 1.6f) * 10f;
                float xOff = (i - 1) * 14f;
                // Upper segment
                Raylib.DrawLineEx(new Vector2(xOff, side * 14f), new Vector2(xOff + swing * 0.4f, side * 28f), 5f, Stone900);
                // Lower segment
                Raylib.DrawLineEx(new Vector2(xOff + swing * 0.4f, side * 28f), new Vector2(xOff + swing, side * 40f), 3.5f, Stone800);
                // Foot claw
                Raylib.DrawCircleV(new Vector2(xOff + swing, side * 40f), 2.5f, Stone700);
            }
        }

        // Kinetic trample trails when charging
        if (e.Charging)
        {
            for (int i = 0; i < 3; i++)
            {
                float trailX = -r * 0.8f - i * 14f;
                Raylib.DrawLineEx(new Vector2(trailX, -14f + i * 4f), new Vector2(trailX - 18f, -14f + i * 4f), 2.5f, new Color((byte)245, (byte)158, (byte)11, (byte)(160 - i * 45)));
                Raylib.DrawLineEx(new Vector2(trailX, 14f - i * 4f), new Vector2(trailX - 18f, 14f - i * 4f), 2.5f, new Color((byte)245, (byte)158, (byte)11, (byte)(160 - i * 45)));
            }
        }

        // WEAKPOINT: Fleshy orange abdomen (rear)
        Raylib.DrawEllipse((int)(-r * 0.7f), 0, (int)(r * 0.72f), (int)(r * 0.58f), Constants.OrangeHazard);
        // Pulsating veins on abdomen
        Raylib.DrawEllipseLines((int)(-r * 0.7f), 0, (int)(r * 0.72f), (int)(r * 0.58f), Amber600);
        Raylib.DrawEllipseLines((int)(-r * 0.65f), 0, (int)(r * 0.45f), (int)(r * 0.35f), new Color((byte)220, (byte)100, (byte)20, (byte)140));

        // Obsidian carapace thorax
        Raylib.DrawEllipse(2, 0, (int)(r * 0.65f), (int)(r * 0.78f), Stone900);
        // Chitin plate ridges
        for (int i = -2; i <= 2; i++)
        {
            Raylib.DrawLineEx(new Vector2(-r * 0.3f, i * 8f), new Vector2(r * 0.35f, i * 7f), 1.5f, Stone700);
        }
        Raylib.DrawEllipseLines(2, 0, (int)(r * 0.65f), (int)(r * 0.78f), Stone700);

        // Heavy frontal horn crest – layered
        Raylib.DrawTriangle(
            new Vector2(r * 0.15f, -r * 0.95f),
            new Vector2(r * 0.15f,  r * 0.95f),
            new Vector2(r + 22f, 0), new Color(8, 7, 6, 255));
        // Horn inner ridge
        Raylib.DrawTriangle(
            new Vector2(r * 0.25f, -r * 0.65f),
            new Vector2(r * 0.25f,  r * 0.65f),
            new Vector2(r + 14f, 0), new Color(18, 16, 14, 255));
        Raylib.DrawTriangleLines(
            new Vector2(r * 0.15f, -r * 0.95f),
            new Vector2(r * 0.15f,  r * 0.95f),
            new Vector2(r + 22f, 0), new Color(90, 85, 80, 255));

        // Charging kinetic shockwave around horn
        if (e.Charging)
        {
            Raylib.DrawLineEx(new Vector2(r + 18f, -20f), new Vector2(r + 28f, 0), 3f, Constants.OrangeHazard);
            Raylib.DrawLineEx(new Vector2(r + 18f,  20f), new Vector2(r + 28f, 0), 3f, Constants.OrangeHazard);
            Raylib.DrawLineEx(new Vector2(r + 24f, -26f), new Vector2(r + 34f, 0), 2f, Red500);
            Raylib.DrawLineEx(new Vector2(r + 24f,  26f), new Vector2(r + 34f, 0), 2f, Red500);
        }

        // Head with mandible gap
        Raylib.DrawCircle((int)(r * 0.5f), 0, 8, new Color(18, 16, 14, 255));

        // Burning eyes
        Color eyeC = e.Charging ? Red500 : Constants.OrangeHazard;
        Raylib.DrawCircle((int)(r * 0.6f), -6, 3.5f, eyeC);
        Raylib.DrawCircle((int)(r * 0.6f),  6, 3.5f, eyeC);
        // Eye glow
        Raylib.DrawCircle((int)(r * 0.6f), -6, 1.5f, Color.White);
        Raylib.DrawCircle((int)(r * 0.6f),  6, 1.5f, Color.White);
    }

    private static void DrawBileTitan(Enemy e, float r)
    {
        // 4 towering arachnid legs (pairs)
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 2; i++)
            {
                float swing = MathF.Sin(e.WalkCycle + i * 2.0f) * 20f;
                float xOff = (i - 0.5f) * 36f;
                // Upper femur
                Raylib.DrawLineEx(new Vector2(xOff, side * 22f), new Vector2(xOff + swing * 0.3f, side * 55f), 7f, Emerald800);
                // Lower tibia
                Raylib.DrawLineEx(new Vector2(xOff + swing * 0.3f, side * 55f), new Vector2(xOff + swing, side * 90f), 5f, new Color(4, 60, 45, 255));
                // Tarsal claw
                Raylib.DrawCircleV(new Vector2(xOff + swing, side * 90f), 4f, new Color(3, 40, 30, 255));
            }
        }

        // WEAKPOINT: Glowing venom sac underbelly
        Raylib.DrawEllipse(-16, 0, 42, 26, Constants.LibertyGreen);
        // Bile pulsation rings
        Raylib.DrawEllipseLines(-16, 0, 42, 26, Emerald400);
        Raylib.DrawEllipseLines(-16, 0, 30, 18, new Color(134, 239, 172, 120));
        Raylib.DrawEllipseLines(-16, 0, 20, 12, new Color(190, 255, 200, 80));

        // Spined thorax with segmented plates
        Raylib.DrawEllipse(14, 0, 50, 30, Emerald800);
        // Chitin segment lines
        for (int i = -3; i <= 3; i++)
            Raylib.DrawLineEx(new Vector2(-20, i * 7f), new Vector2(42, i * 6f), 1.2f, Emerald600);
        Raylib.DrawEllipseLines(14, 0, 50, 30, Emerald600);

        // Dorsal spines
        for (int i = 0; i < 5; i++)
        {
            float sx = -10 + i * 12;
            Raylib.DrawTriangle(
                new Vector2(sx - 3, -28), new Vector2(sx + 3, -28), new Vector2(sx, -38),
                new Color(3, 50, 38, 255));
            Raylib.DrawTriangle(
                new Vector2(sx - 3, 28), new Vector2(sx + 3, 28), new Vector2(sx, 38),
                new Color(3, 50, 38, 255));
        }

        // Skull head & mandibles
        Raylib.DrawEllipse(46, 0, 18, 14, new Color(3, 44, 34, 255));
        Raylib.DrawEllipseLines(46, 0, 18, 14, Emerald600);
        // Mandible pincers
        Raylib.DrawLineEx(new Vector2(58, -8), new Vector2(74, -14), 4f, new Color(2, 35, 26, 255));
        Raylib.DrawLineEx(new Vector2(58,  8), new Vector2(74,  14), 4f, new Color(2, 35, 26, 255));
        // Compound eyes
        Raylib.DrawCircle(52, -7, 4, Constants.LibertyGreen);
        Raylib.DrawCircle(52,  7, 4, Constants.LibertyGreen);
        Raylib.DrawCircle(52, -7, 2, Color.White);
        Raylib.DrawCircle(52,  7, 2, Color.White);
    }

    private static void DrawHunter(Enemy e, float r)
    {
        // Translucent light-blue/cyan wings (from reference image)
        float wing = MathF.Sin(e.WalkCycle * 3.5f) * 7f;
        Color wingC = new((byte)56, (byte)189, (byte)248, (byte)175);
        Raylib.DrawTriangle(new Vector2(-6, -5), new Vector2(-18, -26 - wing), new Vector2(4, -12), wingC);
        Raylib.DrawTriangle(new Vector2(-6, 5),  new Vector2(-18, 26 + wing),  new Vector2(4, 12), wingC);
        // Wing veins
        Raylib.DrawLineEx(new Vector2(-6, -5), new Vector2(-16, -24 - wing), 1f, new Color((byte)186, (byte)230, (byte)253, (byte)200));
        Raylib.DrawLineEx(new Vector2(-6, 5),  new Vector2(-16, 24 + wing), 1f, new Color((byte)186, (byte)230, (byte)253, (byte)200));

        // 4 jumping insect legs
        for (int side = -1; side <= 1; side += 2)
        {
            float lSwing = MathF.Sin(e.WalkCycle * 2f) * 5f;
            Raylib.DrawLineEx(new Vector2(-4, side * 6), new Vector2(-8 + lSwing, side * 18), 2.5f, Lime700);
            Raylib.DrawLineEx(new Vector2(4, side * 5),  new Vector2(8 + lSwing, side * 16),  2.5f, Lime700);
        }

        // Sleek lime-green body with darker green segments (reference image)
        Raylib.DrawEllipse(0, 0, (int)(r * 0.85f), (int)(r * 0.5f), Lime500);
        Raylib.DrawEllipseLines(0, 0, (int)(r * 0.85f), (int)(r * 0.5f), Lime700);
        // Abdominal segment rings
        Raylib.DrawLineEx(new Vector2(-r * 0.5f, -3), new Vector2(-r * 0.5f, 3), 1.5f, Lime700);
        Raylib.DrawLineEx(new Vector2(-r * 0.2f, -4), new Vector2(-r * 0.2f, 4), 1.5f, Lime700);
        Raylib.DrawLineEx(new Vector2(r * 0.1f, -4), new Vector2(r * 0.1f, 4), 1.5f, Lime700);

        // Mantis raptorial forearms (scythes)
        Raylib.DrawLineEx(new Vector2(r * 0.4f, -4), new Vector2(r + 8, -12), 2.5f, Lime700);
        Raylib.DrawLineEx(new Vector2(r + 8, -12), new Vector2(r + 14, -6), 2f, Lime500);
        Raylib.DrawLineEx(new Vector2(r * 0.4f, 4), new Vector2(r + 8, 12), 2.5f, Lime700);
        Raylib.DrawLineEx(new Vector2(r + 8, 12), new Vector2(r + 14, 6), 2f, Lime500);

        // Head
        Raylib.DrawCircle((int)(r * 0.55f), 0, 5, Lime700);
        // Feelers / antennae
        Raylib.DrawLineEx(new Vector2(r * 0.6f, -3), new Vector2(r + 12, -8), 1.5f, new Color((byte)190, (byte)242, (byte)100, (byte)220));
        Raylib.DrawLineEx(new Vector2(r * 0.6f, 3),  new Vector2(r + 12, 8),  1.5f, new Color((byte)190, (byte)242, (byte)100, (byte)220));
        // Large compound eyes
        Raylib.DrawCircle((int)(r * 0.65f), -3, 2, Color.Black);
        Raylib.DrawCircle((int)(r * 0.65f), 3,  2, Color.Black);
        Raylib.DrawCircle((int)(r * 0.67f), -3, 1, Color.White);
        Raylib.DrawCircle((int)(r * 0.67f), 3,  1, Color.White);
    }

    private static void DrawBileSpewer(Enemy e, float r)
    {
        // Short stumpy legs
        for (int side = -1; side <= 1; side += 2)
        {
            float lSwing = MathF.Sin(e.WalkCycle * 1.5f) * 5f;
            Raylib.DrawLineEx(new Vector2(-6, side * 10), new Vector2(-10 + lSwing, side * 22), 4f, Lime700);
            Raylib.DrawLineEx(new Vector2(6, side * 8),  new Vector2(10 + lSwing, side * 20),  3.5f, Lime700);
        }

        // Bloated translucent bile sac (rear body)
        Raylib.DrawEllipse(-6, 0, (int)(r * 0.85f), (int)(r * 0.72f), Lime500);
        // Glowing pustules / acid sacs (from reference image)
        Raylib.DrawCircle(-14, -6, 5, new Color((byte)234, (byte)179, (byte)8, (byte)220));
        Raylib.DrawCircle(-14,  6, 5, new Color((byte)234, (byte)179, (byte)8, (byte)220));
        Raylib.DrawCircle(-4, -10, 4, new Color((byte)234, (byte)179, (byte)8, (byte)220));
        Raylib.DrawCircle(-4,  10, 4, new Color((byte)234, (byte)179, (byte)8, (byte)220));
        Raylib.DrawCircle(-8,   0, 6, new Color((byte)190, (byte)242, (byte)100, (byte)200));

        Raylib.DrawEllipseLines(-6, 0, (int)(r * 0.85f), (int)(r * 0.72f), Lime700);

        // Dark chitinous head / nozzle
        Raylib.DrawEllipse((int)(r * 0.4f), 0, (int)(r * 0.4f), (int)(r * 0.45f), Stone900);
        Raylib.DrawEllipseLines((int)(r * 0.4f), 0, (int)(r * 0.4f), (int)(r * 0.45f), Stone700);
        // Bile nozzle barrel
        Raylib.DrawRectangle((int)(r * 0.6f), -4, 11, 8, Stone900);
        Raylib.DrawCircle((int)(r * 0.6f + 11), 0, 4, Lime500);
        // Dripping acid drop
        float drip = MathF.Sin(e.WalkCycle * 4f) * 3f;
        Raylib.DrawCircle((int)(r * 0.6f + 14 + drip), 0, 2f, new Color((byte)163, (byte)230, (byte)53, (byte)220));

        // Eyes
        Raylib.DrawCircle((int)(r * 0.5f), -5, 2.5f, Red500);
        Raylib.DrawCircle((int)(r * 0.5f),  5, 2.5f, Red500);
    }

    private static void DrawWarrior(Enemy e, float r)
    {
        // 4 legs
        for (int side = -1; side <= 1; side += 2)
        {
            float lSwing = MathF.Sin(e.WalkCycle * 1.8f) * 6f;
            Raylib.DrawLineEx(new Vector2(-4, side * 8), new Vector2(-8 + lSwing, side * 22), 3.5f, new Color(100, 45, 12, 255));
            Raylib.DrawLineEx(new Vector2(6, side * 7),  new Vector2(10 + lSwing, side * 20), 3f, new Color(100, 45, 12, 255));
        }

        // Hardened thorax
        Raylib.DrawEllipse(-2, 0, (int)(r * 0.7f), (int)(r * 0.6f), new Color(120, 53, 15, 255));
        Raylib.DrawEllipseLines(-2, 0, (int)(r * 0.7f), (int)(r * 0.6f), new Color(80, 35, 10, 255));
        // Segment ridges
        Raylib.DrawLineEx(new Vector2(-r * 0.4f, 0), new Vector2(r * 0.3f, 0), 1.5f, new Color(80, 35, 10, 255));

        // Head with bladed mandibles
        Raylib.DrawCircle((int)(r * 0.45f), 0, 7, Stone900);
        Raylib.DrawTriangle(
            new Vector2(r * 0.5f, -4), new Vector2(r * 0.5f, 4), new Vector2(r + 10, 0),
            new Color(60, 30, 8, 255));
        // Eyes
        Raylib.DrawCircle((int)(r * 0.55f), -4, 2, Constants.OrangeHazard);
        Raylib.DrawCircle((int)(r * 0.55f),  4, 2, Constants.OrangeHazard);
    }

    private static void DrawScavenger(Enemy e, float r)
    {
        // 4 spindly legs
        for (int side = -1; side <= 1; side += 2)
        {
            float lSwing = MathF.Sin(e.WalkCycle * 2.5f) * 4f;
            Raylib.DrawLineEx(new Vector2(-2, side * 5), new Vector2(-5 + lSwing, side * 14), 2f, new Color(160, 75, 25, 255));
            Raylib.DrawLineEx(new Vector2(4, side * 4),  new Vector2(7 + lSwing, side * 13),  2f, new Color(160, 75, 25, 255));
        }

        // Small amber body
        Raylib.DrawEllipse(0, 0, (int)(r * 0.8f), (int)(r * 0.55f), new Color(210, 115, 40, 255));
        Raylib.DrawEllipseLines(0, 0, (int)(r * 0.8f), (int)(r * 0.55f), new Color(160, 80, 25, 255));

        // Tiny head
        Raylib.DrawCircle((int)(r * 0.5f), 0, 4, Stone900);
        Raylib.DrawCircle((int)(r * 0.55f), -2, 1.5f, Constants.OrangeHazard);
        Raylib.DrawCircle((int)(r * 0.55f),  2, 1.5f, Constants.OrangeHazard);
    }

    // ─── AUTOMATON ENEMIES ──────────────────────────────────────────

    public static void DrawAutomaton(Enemy e)
    {
        string spriteKey = EnemySpriteManager.GetSpriteKeyForEnemy(e);
        if (EnemySpriteManager.HasTexture(spriteKey))
        {
            EnemySpriteManager.DrawInGameEnemy(e);
            return;
        }

        Vector2 pos = e.Position;
        float angle = e.FacingAngle;
        float r = e.Radius;

        Rlgl.PushMatrix();
        Rlgl.Translatef(pos.X, pos.Y, 0);
        Rlgl.Rotatef(angle * (180f / MathF.PI), 0, 0, 1);

        // Ground shadow
        Raylib.DrawEllipse(0, 6, (int)(r * 1.1f), (int)(r * 0.65f), ShadowCol);

        if (e.Type == EnemyType.Hulk || (e.IsHvt && e.Faction == Faction.Automatons))
            DrawHulk(e, r);
        else if (e.Type == EnemyType.Berserker)
            DrawBerserker(e, r);
        else if (e.Type == EnemyType.Devastator)
            DrawDevastator(e, r);
        else if (e.Type == EnemyType.FactoryStrider)
            DrawFactoryStrider(e, r);
        else if (e.Type == EnemyType.Commissar)
            DrawCommissar(e, r);
        else
            DrawTrooper(e, r);

        Rlgl.PopMatrix();

        DrawEnemyOverlays(e);
    }

    private static void DrawHulk(Enemy e, float r)
    {
        // Heavy hydraulic legs
        float lBob = MathF.Sin(e.WalkCycle) * 3f;
        Raylib.DrawRectangle(-20, -28 + (int)lBob, 20, 14, Slate700);
        Raylib.DrawRectangleLines(-20, -28 + (int)lBob, 20, 14, Slate600);
        Raylib.DrawRectangle(-20, 14 - (int)lBob, 20, 14, Slate700);
        Raylib.DrawRectangleLines(-20, 14 - (int)lBob, 20, 14, Slate600);
        // Knee pistons
        Raylib.DrawRectangle(-16, -22 + (int)lBob, 4, 6, new Color(120, 130, 140, 255));
        Raylib.DrawRectangle(-16, 18 - (int)lBob, 4, 6, new Color(120, 130, 140, 255));

        // WEAKPOINT: Glowing radiator heatsink (rear)
        int hsX = (int)(-r * 0.92f);
        // Heat aura
        Raylib.DrawRectangle(hsX - 4, -18, 22, 36, new Color((byte)239, (byte)68, (byte)68, (byte)60));
        Raylib.DrawRectangle(hsX, -16, 14, 32, Constants.OrangeHazard);
        Raylib.DrawRectangleLines(hsX, -16, 14, 32, Constants.Gold);
        // Heat vent lines
        for (int i = 0; i < 4; i++)
            Raylib.DrawRectangle(hsX + 2, -12 + i * 8, 10, 2, new Color((byte)255, (byte)240, (byte)180, (byte)220));

        // Carbon-steel torso
        Rectangle torso = new(-r * 0.6f, -r * 0.72f, r * 1.24f, r * 1.44f);
        Raylib.DrawRectangleRec(torso, Slate800);
        Raylib.DrawRectangleLinesEx(torso, 2, Slate600);
        // Armour detail lines
        Raylib.DrawLineEx(new Vector2(-r * 0.55f, -r * 0.2f), new Vector2(r * 0.6f, -r * 0.2f), 1f, Slate700);
        Raylib.DrawLineEx(new Vector2(-r * 0.55f, r * 0.2f), new Vector2(r * 0.6f, r * 0.2f), 1f, Slate700);

        // Central reactor core
        Raylib.DrawCircle(0, 0, 8, Slate900);
        Raylib.DrawCircleLines(0, 0, 8, Red500);

        // Horizontal crimson visor
        Raylib.DrawRectangle((int)(r * 0.2f), -7, 10, 14, Slate900);
        Raylib.DrawRectangle((int)(r * 0.24f), -4, 7, 8, Red500);
        Raylib.DrawRectangle((int)(r * 0.28f), -2, 4, 4, Color.White);

        // Left arm: Flamethrower
        Raylib.DrawRectangle(2, (int)(-r * 0.9f), 30, 10, Slate700);
        Raylib.DrawRectangleLines(2, (int)(-r * 0.9f), 30, 10, Slate600);
        Raylib.DrawRectangle(28, (int)(-r * 0.88f), 6, 6, Constants.OrangeHazard);

        // Right arm: Buzzsaw blade
        int sawY = (int)(r * 0.82f);
        Raylib.DrawRectangle(2, sawY, 22, 10, Slate700);
        Raylib.DrawCircle(20, sawY + 5, 12, new Color(140, 140, 150, 255));
        Raylib.DrawCircleLines(20, sawY + 5, 12, Red500);
        Raylib.DrawCircle(20, sawY + 5, 4, Slate700);
        // Rotating saw teeth marks
        float sawAngle = e.WalkCycle * 8f;
        for (int s = 0; s < 6; s++)
        {
            float a = sawAngle + s * (MathF.PI / 3f);
            var tp1 = new Vector2(20 + MathF.Cos(a) * 8f, sawY + 5 + MathF.Sin(a) * 8f);
            var tp2 = new Vector2(20 + MathF.Cos(a) * 14f, sawY + 5 + MathF.Sin(a) * 14f);
            Raylib.DrawLineEx(tp1, tp2, 1.5f, Color.White);
        }
    }

    private static void DrawBerserker(Enemy e, float r)
    {
        // Agile legs
        float lBob = MathF.Sin(e.WalkCycle * 1.5f) * 4f;
        Raylib.DrawRectangle(-8, -18 + (int)lBob, 12, 8, Slate700);
        Raylib.DrawRectangle(-8, 10 - (int)lBob, 12, 8, Slate700);

        // Compact torso
        Raylib.DrawRectangle(-10, -12, 22, 24, Slate800);
        Raylib.DrawRectangleLinesEx(new Rectangle(-10, -12, 22, 24), 1, Slate600);

        // Skull-shaped head
        Raylib.DrawCircle(8, 0, 10, Slate900);
        Raylib.DrawCircleLines(8, 0, 10, Slate600);
        // Menacing red slit eye
        Raylib.DrawRectangle(12, -3, 5, 6, Red500);
        Raylib.DrawRectangle(14, -1, 2, 2, Color.White);

        // Dual chainsaw arms – with teeth detail and blurred cutting arcs
        for (int side = -1; side <= 1; side += 2)
        {
            int sy = side * 18;
            Raylib.DrawRectangle(6, sy - 3, 28, 6, Red700);
            Raylib.DrawRectangleLines(6, sy - 3, 28, 6, new Color((byte)220, (byte)50, (byte)50, (byte)255));
            // Teeth marks
            for (int t = 0; t < 5; t++)
                Raylib.DrawRectangle(8 + t * 5, sy - 4, 2, 1, Color.White);
            // Blade tip
            Raylib.DrawTriangle(
                new Vector2(34, sy - 3), new Vector2(34, sy + 3), new Vector2(38, sy),
                Red700);
            // Spinning blade motion arc
            float sawSwing = MathF.Sin(e.WalkCycle * 10f) * 3f;
            Raylib.DrawLineEx(new Vector2(36, sy - 4), new Vector2(40 + sawSwing, sy + 4), 1.5f, new Color((byte)255, (byte)215, (byte)0, (byte)150));
        }
    }

    private static void DrawDevastator(Enemy e, float r)
    {
        // Mechanized legs
        float lBob = MathF.Sin(e.WalkCycle) * 3f;
        Raylib.DrawRectangle(-12, -20 + (int)lBob, 16, 10, Slate700);
        Raylib.DrawRectangle(-12, 10 - (int)lBob, 16, 10, Slate700);

        // Reinforced torso plate
        Raylib.DrawRectangle(-14, -16, 30, 32, Slate700);
        Raylib.DrawRectangleLinesEx(new Rectangle(-14, -16, 30, 32), 2, Slate900);
        // Armour chevron
        Raylib.DrawTriangle(
            new Vector2(-10, -10), new Vector2(-10, 10), new Vector2(8, 0),
            Slate800);

        // Cyclops sensor head
        Raylib.DrawCircle(12, 0, 8, Slate900);
        Raylib.DrawCircleLines(12, 0, 8, Slate600);
        Raylib.DrawCircle(14, 0, 4, Red500);
        Raylib.DrawCircle(15, 0, 1.5f, Color.White);

        // Shoulder-mounted 2x2 rocket pods (from Rocket Devastator in reference image)
        for (int side = -1; side <= 1; side += 2)
        {
            int py = side * 22;
            Rectangle podRec = new(-8, py - 9, 24, 18);
            Raylib.DrawRectangleRec(podRec, Slate900);
            Raylib.DrawRectangleLinesEx(podRec, 1.5f, Red700);

            // 4 rocket warhead tips (2x2 grid)
            Raylib.DrawCircle(12, py - 4, 2.5f, Red500);
            Raylib.DrawCircle(12, py + 4, 2.5f, Red500);
            Raylib.DrawCircle(6, py - 4, 2.5f, Red500);
            Raylib.DrawCircle(6, py + 4, 2.5f, Red500);

            Raylib.DrawCircle(12, py - 4, 1f, Constants.OrangeHazard);
            Raylib.DrawCircle(12, py + 4, 1f, Constants.OrangeHazard);
            Raylib.DrawCircle(6, py - 4, 1f, Constants.OrangeHazard);
            Raylib.DrawCircle(6, py + 4, 1f, Constants.OrangeHazard);
        }
    }

    private static void DrawFactoryStrider(Enemy e, float r)
    {
        // 4 massive walker legs
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 2; i++)
            {
                float swing = MathF.Sin(e.WalkCycle + i * MathF.PI) * 8f;
                float xOff = (i == 0 ? -30 : 20);
                Raylib.DrawLineEx(new Vector2(xOff, side * 28), new Vector2(xOff + swing, side * 65), 6f, Slate700);
                Raylib.DrawRectangle((int)(xOff + swing - 4), side * 62, 8, 8, Slate800);
            }
        }

        // Massive hull
        Raylib.DrawRectangle(-48, -38, 96, 76, Slate900);
        Raylib.DrawRectangleLinesEx(new Rectangle(-48, -38, 96, 76), 3, Slate600);
        // Hull plating details
        Raylib.DrawLineEx(new Vector2(-44, 0), new Vector2(44, 0), 1.5f, Slate700);
        Raylib.DrawLineEx(new Vector2(0, -34), new Vector2(0, 34), 1.5f, Slate700);

        // Central command eye
        Raylib.DrawCircle(0, 0, 14, Slate800);
        Raylib.DrawCircle(0, 0, 10, Red500);
        Raylib.DrawCircle(0, 0, 4, Color.White);

        // Heavy cannons
        for (int side = -1; side <= 1; side += 2)
        {
            Raylib.DrawRectangle(46, side * 12 - 5, 24, 10, Slate700);
            Raylib.DrawRectangleLines(46, side * 12 - 5, 24, 10, Slate600);
            Raylib.DrawCircle(70, side * 12, 4, Red500);
        }

        // Spawn bay hatch (bottom)
        Raylib.DrawRectangle(-20, -34, 40, 10, Slate700);
        Raylib.DrawRectangleLines(-20, -34, 40, 10, Red500);
    }

    private static void DrawTrooper(Enemy e, float r)
    {
        // Simple bipedal legs
        float lBob = MathF.Sin(e.WalkCycle * 1.4f) * 3f;
        Raylib.DrawRectangle(-6, -14 + (int)lBob, 8, 6, Slate700);
        Raylib.DrawRectangle(-6, 8 - (int)lBob, 8, 6, Slate700);

        // Lightweight torso
        Raylib.DrawRectangle(-8, -10, 18, 20, Slate700);
        Raylib.DrawRectangleLinesEx(new Rectangle(-8, -10, 18, 20), 1, Slate600);

        // Small visor head
        Raylib.DrawCircle(8, 0, 6, Slate900);
        Raylib.DrawRectangle(10, -2, 4, 4, Red500);
        Raylib.DrawRectangle(11, -1, 2, 2, Color.White);

        // Rifle arm
        Raylib.DrawRectangle(6, 6, 18, 4, Slate800);
        Raylib.DrawRectangle(22, 7, 4, 2, Red500);
    }

    private static void DrawCommissar(Enemy e, float r)
    {
        // Simple bipedal legs
        float lBob = MathF.Sin(e.WalkCycle * 1.5f) * 3f;
        Raylib.DrawRectangle(-6, -14 + (int)lBob, 8, 6, Slate700);
        Raylib.DrawRectangle(-6, 8 - (int)lBob, 8, 6, Slate700);

        // Crimson Officer Cape trailing on left
        float capeWave = MathF.Sin(e.WalkCycle * 2f) * 3f;
        Raylib.DrawTriangle(
            new Vector2(-4, -6),
            new Vector2(-18 + capeWave, -16),
            new Vector2(-12, -4), Red700);

        // Lightweight torso with brass/gold medal badge
        Raylib.DrawRectangle(-8, -10, 18, 20, Slate800);
        Raylib.DrawRectangleLinesEx(new Rectangle(-8, -10, 18, 20), 1, Red700);
        Raylib.DrawCircle(0, 0, 3, Constants.Gold);

        // Peaked officer cap on head
        Raylib.DrawCircle(8, 0, 6, Slate900);
        Raylib.DrawRectangle(6, -7, 10, 4, Red700);
        Raylib.DrawLineEx(new Vector2(6, -7), new Vector2(16, -7), 1.5f, Constants.Gold);

        // Glowing red mono-visor
        Raylib.DrawRectangle(10, -2, 4, 4, Red500);
        Raylib.DrawRectangle(11, -1, 2, 2, Color.White);

        // Left arm: Blaster pistol
        Raylib.DrawRectangle(6, -9, 12, 3, Slate700);
        Raylib.DrawRectangle(16, -9, 3, 2, Red500);

        // Right arm: Energy Power Blade / Sword
        Raylib.DrawRectangle(6, 7, 8, 3, Slate700);
        Raylib.DrawLineEx(new Vector2(14, 8), new Vector2(28, 12), 2.5f, Constants.OrangeHazard);
        Raylib.DrawLineEx(new Vector2(14, 8), new Vector2(28, 12), 1f, Color.White);
    }

    public static void DrawEnemyPreview(EnemyType type, Faction faction, Vector2 pos, float scale, float angle = 0f, float walkCycle = 0f)
    {
        var dummy = Enemy.Create(type, faction, pos, isHvt: false);
        dummy.IsHvt = false;
        dummy.FacingAngle = angle;
        dummy.WalkCycle = walkCycle;

        Rlgl.PushMatrix();
        Rlgl.Translatef(pos.X, pos.Y, 0);
        Rlgl.Scalef(scale, scale, 1f);
        Rlgl.Translatef(-pos.X, -pos.Y, 0);

        if (faction == Faction.Terminids)
            DrawTerminid(dummy);
        else
            DrawAutomaton(dummy);

        Rlgl.PopMatrix();
    }

    // ─── HELLDIVER PLAYER ───────────────────────────────────────────

    public static void DrawPlayer(Player player)
    {
        float r = player.Radius;

        // Trailing Cape (cloth simulation)
        if (player.CapeHistory.Count > 1)
        {
            for (int i = 0; i < player.CapeHistory.Count - 1; i++)
            {
                float t = (float)i / player.CapeHistory.Count;
                float width = 10f - t * 5f;
                byte alpha = (byte)(255 - (int)(t * 120));
                Raylib.DrawLineEx(player.CapeHistory[i], player.CapeHistory[i + 1], width, new Color((byte)255, (byte)215, (byte)0, alpha));
                Raylib.DrawLineEx(player.CapeHistory[i], player.CapeHistory[i + 1], width - 3f, new Color((byte)0, (byte)0, (byte)0, alpha));
            }
        }

        // Shadow
        Raylib.DrawEllipse((int)player.Position.X, (int)player.Position.Y + 5, (int)(r * 1.1f), (int)(r * 0.5f), ShadowCol);

        // Body (rotated frame)
        Rlgl.PushMatrix();
        Rlgl.Translatef(player.Position.X, player.Position.Y, 0);
        Rlgl.Rotatef(player.Angle * (180f / MathF.PI), 0, 0, 1);

        // Boots (animated with walk cycle)
        float bootSwing = player.IsMoving ? MathF.Sin(player.WalkCycle) * 4f : 0;
        Raylib.DrawRectangle(-6, -14 + (int)bootSwing, 8, 7, Slate700);
        Raylib.DrawRectangle(-6, 7 - (int)bootSwing, 8, 7, Slate700);

        // Armour body
        Raylib.DrawCircle(0, 0, r, Slate900);
        Raylib.DrawCircleLines(0, 0, r, Constants.Gold);
        // Chestplate detail
        Raylib.DrawCircle(0, 0, r - 4, new Color(20, 28, 50, 255));

        // Pauldrons (shoulder pads)
        Raylib.DrawRectangle(-5, (int)(-r - 2), 10, 8, Slate800);
        Raylib.DrawRectangleLines(-5, (int)(-r - 2), 10, 8, Slate600);
        Raylib.DrawRectangle(-5, (int)(r - 6), 10, 8, Slate800);
        Raylib.DrawRectangleLines(-5, (int)(r - 6), 10, 8, Slate600);

        // Super Earth emblem on chest
        Raylib.DrawCircle(0, 0, 4, new Color(40, 50, 70, 255));
        Raylib.DrawCircleLines(0, 0, 4, Constants.Gold);

        // Helmet + Golden visor
        Raylib.DrawCircle(4, 0, 10, Slate900);
        Raylib.DrawCircleLines(4, 0, 10, Slate700);
        // T-shaped visor with specular reflection
        Raylib.DrawRectangle(8, -5, 5, 10, Constants.Gold);
        Raylib.DrawRectangle(6, -2, 8, 4, Constants.Gold);
        Raylib.DrawRectangle(9, -4, 2, 3, Color.White); // glint

        // Weapon silhouette based on equipped weapon
        string wKey = player.CurrentWeapon.Key;
        if (wKey == "breaker" || wKey == "punisher")
        {
            // Shotgun: heavy barrel, pump, muzzle
            Raylib.DrawRectangle(6, 4, 18, 6, Slate700);
            Raylib.DrawRectangleLines(6, 4, 18, 6, Slate600);
            Raylib.DrawRectangle(13, 8, 6, 3, Stone900); // Pump
            Raylib.DrawRectangle(22, 5, 5, 4, Constants.OrangeHazard);
        }
        else if (wKey == "sickle")
        {
            // Laser rifle: sleek heatsink core
            Raylib.DrawRectangle(6, 5, 24, 4, Slate700);
            Raylib.DrawRectangle(12, 5, 7, 3, Constants.Cyan);
            Raylib.DrawRectangle(25, 5, 4, 3, Constants.Cyan);
        }
        else if (wKey == "dominator")
        {
            // Rocket rifle: bulky frame, drum
            Raylib.DrawRectangle(6, 3, 20, 8, Stone900);
            Raylib.DrawRectangleLines(6, 3, 20, 8, Slate600);
            Raylib.DrawCircle(11, 9, 3.5f, Constants.OrangeHazard);
            Raylib.DrawRectangle(22, 4, 5, 5, Constants.Gold);
        }
        else if (wKey == "diligence")
        {
            // DMR Sniper: long barrel, optic
            Raylib.DrawRectangle(6, 6, 26, 3, Slate700);
            Raylib.DrawRectangle(10, 2, 8, 3, Slate900); // Optic
            Raylib.DrawCircle(16, 3, 1.2f, Constants.Cyan);
        }
        else if (wKey == "scorcher")
        {
            // Plasma rifle: plasma capacitor
            Raylib.DrawRectangle(6, 4, 20, 6, Slate800);
            Raylib.DrawRectangle(12, 5, 6, 4, Constants.Cyan);
            Raylib.DrawCircle(24, 7, 2.5f, Constants.Cyan);
        }
        else
        {
            // Standard Liberator AR
            Raylib.DrawRectangle(6, 5, 22, 5, Slate700);
            Raylib.DrawRectangleLines(6, 5, 22, 5, Slate600);
            Raylib.DrawRectangle(24, 6, 5, 3, Constants.Gold);
        }

        // Muzzle flash
        if (player.MuzzleFlash > 0)
        {
            Raylib.DrawCircle(32, 7, 8, new Color((byte)255, (byte)230, (byte)100, (byte)210));
            Raylib.DrawCircle(32, 7, 4, Color.White);
        }

        Rlgl.PopMatrix();

        // Tactical Dive Motion Streaks & Dust
        if (player.IsDiving)
        {
            Vector2 diveDir = player.DiveVelocity.LengthSquared() > 0.01f ? Vector2.Normalize(player.DiveVelocity) : Vector2.Zero;
            for (int i = 1; i <= 3; i++)
            {
                Vector2 streakPos = player.Position - diveDir * (i * 12f);
                Raylib.DrawCircleV(streakPos, 6f - i * 1.5f, new Color((byte)255, (byte)215, (byte)0, (byte)(140 - i * 40)));
            }
        }

        // Energy Shield bubble (pulsating)
        if (player.Shield > 0)
        {
            float pulse = 1f + MathF.Sin(player.ShieldRegenTimer * 6f) * 0.04f;
            float shieldR = (r + 8f) * pulse;
            Raylib.DrawCircleLines((int)player.Position.X, (int)player.Position.Y, shieldR, Constants.Cyan);
            Raylib.DrawCircleLines((int)player.Position.X, (int)player.Position.Y, shieldR - 2, new Color(56, 189, 248, 60));
        }

        // Guard Dog drone
        if (player.HasGuardDog)
        {
            var dronePos = player.Position + new Vector2(MathF.Cos(player.GuardDogAngle), MathF.Sin(player.GuardDogAngle)) * 38f;
            Raylib.DrawEllipse((int)dronePos.X, (int)dronePos.Y + 3, 7, 4, ShadowCol);
            Raylib.DrawCircleV(dronePos, 7, Slate800);
            Raylib.DrawCircleV(dronePos, 5, Slate900);
            Raylib.DrawCircleV(dronePos, 2.5f, Constants.Cyan);
            Raylib.DrawCircleLines((int)dronePos.X, (int)dronePos.Y, 7, Slate600);
        }
    }

    // ─── TACTICAL CROSSHAIR ──────────────────────────────────────────
    public static void DrawCrosshair(Vector2 mouseWorld, bool hasTarget)
    {
        Color reticleCol = hasTarget ? Constants.DemocracyRed : Constants.Cyan;
        float r = hasTarget ? 15f : 12f;

        // Outer reticle circle
        Raylib.DrawCircleLines((int)mouseWorld.X, (int)mouseWorld.Y, r, reticleCol);
        // Center dot
        Raylib.DrawCircle((int)mouseWorld.X, (int)mouseWorld.Y, 2f, reticleCol);
        // 4 crosshair chevrons
        Raylib.DrawLine((int)(mouseWorld.X - r - 5), (int)mouseWorld.Y, (int)(mouseWorld.X - r + 2), (int)mouseWorld.Y, reticleCol);
        Raylib.DrawLine((int)(mouseWorld.X + r - 2), (int)mouseWorld.Y, (int)(mouseWorld.X + r + 5), (int)mouseWorld.Y, reticleCol);
        Raylib.DrawLine((int)mouseWorld.X, (int)(mouseWorld.Y - r - 5), (int)mouseWorld.X, (int)(mouseWorld.Y - r + 2), reticleCol);
        Raylib.DrawLine((int)mouseWorld.X, (int)(mouseWorld.Y + r - 2), (int)mouseWorld.X, (int)(mouseWorld.Y + r + 5), reticleCol);
    }
}
