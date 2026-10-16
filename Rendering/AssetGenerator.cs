using System;
using System.IO;
using System.Numerics;
using Raylib_cs;
using HelldiversDesktop.Models;

namespace HelldiversDesktop.Rendering
{
    /// <summary>
    /// Generador y Exportador Automático de Assets Pixel-Art 16-bit HD (.PNG reales) en C#.
    /// Genera y almacena en disco:
    /// 1. Tira de 12 frames de la Bomba Eagle 500kg en Assets/Vfx/eagle_500kg_sheet.png.
    /// 2. Esferas planetarias 3D con Cel Shading en Assets/Planets/{name}.png.
    /// 3. Tilemaps de superficie 16-bit en Assets/Planets/{name}_tile.png.
    /// Mantiene texturas cacheadas en GPU con TextureFilter.Point a 144 FPS.
    /// </summary>
    public static unsafe class AssetGenerator
    {
        private static bool _initialized = false;
        private static Texture2D _tex500kgSheet;
        private static readonly Dictionary<string, Texture2D> _planetTextures = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Texture2D> _planetTileTextures = new(StringComparer.OrdinalIgnoreCase);

        // Paleta oficial de 6 colores de explosión nuclear
        public static readonly Color ColWhite    = new(255, 255, 255, 255);
        public static readonly Color ColYellow   = new(255, 232, 0, 255);
        public static readonly Color ColOrange   = new(255, 119, 0, 255);
        public static readonly Color ColDarkRed  = new(180, 32, 42, 255);
        public static readonly Color ColAshGray  = new(74, 68, 88, 255);
        public static readonly Color ColCharcoal = new(30, 26, 36, 255);

        public static void Initialize()
        {
            if (_initialized) return;

            string[] searchPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Assets"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Assets")
            };

            string assetsDir = "";
            foreach (var p in searchPaths)
            {
                if (Directory.Exists(p))
                {
                    assetsDir = Path.GetFullPath(p);
                    break;
                }
            }

            if (string.IsNullOrEmpty(assetsDir))
            {
                assetsDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
                Directory.CreateDirectory(assetsDir);
            }

            string vfxDir = Path.Combine(assetsDir, "Vfx");
            string planetsDir = Path.Combine(assetsDir, "Planets");
            string weaponsDir = Path.Combine(assetsDir, "Weapons");

            Directory.CreateDirectory(vfxDir);
            Directory.CreateDirectory(planetsDir);
            Directory.CreateDirectory(weaponsDir);

            // 1. Generar Spritesheet 500kg
            string path500kg = Path.Combine(vfxDir, "eagle_500kg_sheet.png");
            if (!File.Exists(path500kg))
            {
                GenerateAndSave500kgSpritesheet(path500kg);
            }
            if (File.Exists(path500kg))
            {
                _tex500kgSheet = Raylib.LoadTexture(path500kg);
                Raylib.SetTextureFilter(_tex500kgSheet, TextureFilter.Point);
            }

            // 2. Generar Texturas de Planetas y Tilemaps
            string[] planets = { "Malevelon Creek", "Hellmire", "Crimsica", "Vandalon IV" };
            foreach (var p in planets)
            {
                string pSphere = Path.Combine(planetsDir, $"{p}.png");
                string pTile = Path.Combine(planetsDir, $"{p}_tile.png");

                if (!File.Exists(pSphere))
                {
                    GenerateAndSavePlanetSphere(p, pSphere);
                }
                if (!File.Exists(pTile))
                {
                    GenerateAndSavePlanetTile(p, pTile);
                }

                if (File.Exists(pSphere))
                {
                    Texture2D tex = Raylib.LoadTexture(pSphere);
                    Raylib.SetTextureFilter(tex, TextureFilter.Point);
                    _planetTextures[p] = tex;
                }
                if (File.Exists(pTile))
                {
                    Texture2D tile = Raylib.LoadTexture(pTile);
                    Raylib.SetTextureFilter(tile, TextureFilter.Point);
                    _planetTileTextures[p] = tile;
                }
            }

            _initialized = true;
        }

        // =========================================================================
        // 1. SPRITESHEET 12 FRAMES DE BOMBA EAGLE 500KG (96x96 cada frame)
        // =========================================================================
        private static void GenerateAndSave500kgSpritesheet(string filePath)
        {
            const int frameW = 96;
            const int frameH = 96;
            const int frameCount = 12;
            const int totalW = frameW * frameCount;

            Image sheet = Raylib.GenImageColor(totalW, frameH, new Color(0, 0, 0, 0));

            for (int f = 0; f < frameCount; f++)
            {
                int ox = f * frameW;
                int cx = ox + 48;
                int baseY = 86;

                float prog = (f + 0.5f) / (float)frameCount;

                if (f <= 1)
                {
                    // Frames 1-2: Domo cegador blanco/amarillo a ras de suelo + cráter negro
                    int domeR = 14 + f * 16;
                    // Cráter negro en suelo
                    DrawPixelEllipseOnImage(sheet, cx, baseY + 2, domeR + 10, 8, ColCharcoal);
                    // Domo exterior naranja/amarillo
                    DrawPixelCircleOnImage(sheet, cx, baseY, domeR, ColOrange);
                    DrawPixelCircleOnImage(sheet, cx, baseY, domeR - 4, ColYellow);
                    DrawPixelCircleOnImage(sheet, cx, baseY - 2, domeR - 9, ColWhite);
                }
                else if (f <= 5)
                {
                    // Frames 3-6: Tallo de fuego sube vertical mientras la cabeza del hongo nuclear se expande arriba
                    int capY = 38 - (f - 2) * 5;
                    int capRx = 16 + (f - 2) * 7;
                    int capRy = 11 + (f - 2) * 5;
                    int stemW = 8 + (f - 2) * 2;

                    // Anillo de polvo horizontal en la base
                    int ringW = 22 + (f - 2) * 8;
                    DrawPixelEllipseOnImage(sheet, cx, baseY, ringW, 5, ColAshGray);
                    DrawPixelEllipseOnImage(sheet, cx, baseY - 2, ringW - 4, 3, ColOrange);

                    // Tallo vertical
                    for (int y = capY + capRy / 2; y <= baseY; y++)
                    {
                        int halfW = stemW / 2 + ((y * 13) % 3);
                        for (int x = cx - halfW; x <= cx + halfW; x++)
                        {
                            int distFromCenter = Math.Abs(x - cx);
                            Color c = distFromCenter switch
                            {
                                <= 2 => ColWhite,
                                <= 5 => ColYellow,
                                <= 8 => ColOrange,
                                _ => ColDarkRed
                            };
                            Raylib.ImageDrawPixel(&sheet, x, y, c);
                        }
                    }

                    // Cabeza del Hongo Nuclear Pixelado (Rugoso / Celular)
                    for (int y = capY - capRy; y <= capY + capRy; y++)
                    {
                        for (int x = cx - capRx; x <= cx + capRx; x++)
                        {
                            float dx = (x - cx) / (float)capRx;
                            float dy = (y - capY) / (float)capRy;
                            float d = dx * dx + dy * dy;

                            // Rugosidad celular / dither
                            float noise = ((x ^ (y * 7)) % 9) / 80f;
                            float evalD = d + noise;

                            if (evalD <= 1.0f)
                            {
                                Color col;
                                if (evalD < 0.25f) col = ColWhite;
                                else if (evalD < 0.50f) col = ColYellow;
                                else if (evalD < 0.75f) col = ColOrange;
                                else if (evalD < 0.90f) col = ColDarkRed;
                                else col = ColCharcoal;

                                Raylib.ImageDrawPixel(&sheet, x, y, col);
                            }
                        }
                    }
                }
                else if (f <= 9)
                {
                    // Frames 7-10: El centro del hongo pasa de naranja ardiente a humo negro/rojo con brasas pixeladas
                    int capY = 24 - (f - 6) * 2;
                    int capRx = 38 + (f - 6) * 5;
                    int capRy = 22 + (f - 6) * 3;

                    // Dibujar lóbulos de humo
                    int[] puffOffsetsX = { -16, 0, 16, -8, 8 };
                    int[] puffOffsetsY = { 4, -4, 4, -10, -8 };

                    for (int p = 0; p < puffOffsetsX.Length; p++)
                    {
                        int px = cx + puffOffsetsX[p];
                        int py = capY + puffOffsetsY[p];
                        int pr = 14 + (f - 6) * 2;

                        for (int y = py - pr; y <= py + pr; y++)
                        {
                            for (int x = px - pr; x <= px + pr; x++)
                            {
                                int dsq = (x - px) * (x - px) + (y - py) * (y - py);
                                if (dsq <= pr * pr)
                                {
                                    float normD = MathF.Sqrt(dsq) / pr;
                                    Color c = normD switch
                                    {
                                        < 0.35f => (f == 6 || f == 7) ? ColOrange : ColDarkRed,
                                        < 0.70f => ColDarkRed,
                                        < 0.90f => ColAshGray,
                                        _ => ColCharcoal
                                    };
                                    Raylib.ImageDrawPixel(&sheet, x, y, c);
                                }
                            }
                        }
                    }

                    // Brasas pixeladas volando alrededor
                    Random rnd = new(f * 99);
                    for (int em = 0; em < 16; em++)
                    {
                        int ex = cx + rnd.Next(-capRx - 8, capRx + 8);
                        int ey = capY + rnd.Next(-capRy - 12, capRy + 12);
                        Color emberCol = rnd.Next(0, 2) == 0 ? ColYellow : ColOrange;
                        Raylib.ImageDrawPixel(&sheet, ex, ey, emberCol);
                    }
                }
                else
                {
                    // Frames 11-12: Disipación con Dithering (patrón de ajedrez transparente)
                    int capY = 16;
                    int capRx = 46;
                    int capRy = 24;

                    for (int y = capY - capRy; y <= capY + capRy; y++)
                    {
                        for (int x = cx - capRx; x <= cx + capRx; x++)
                        {
                            float dx = (x - cx) / (float)capRx;
                            float dy = (y - capY) / (float)capRy;
                            if (dx * dx + dy * dy <= 1.0f)
                            {
                                // Dithering ajedrezado
                                bool dither = ((x + y) % (f == 10 ? 2 : 3)) == 0;
                                if (dither)
                                {
                                    Color c = ((x * 3 + y * 7) % 5 == 0) ? ColAshGray : ColCharcoal;
                                    Raylib.ImageDrawPixel(&sheet, x, y, c);
                                }
                            }
                        }
                    }
                }
            }

            Raylib.ExportImage(sheet, filePath);
            Raylib.UnloadImage(sheet);
        }

        // =========================================================================
        // 2. ESFERAS PLANETARIAS PIXEL-ART 3D (128x128 con Cel Shading)
        // =========================================================================
        private static void GenerateAndSavePlanetSphere(string planetName, string filePath)
        {
            const int size = 128;
            const float R = 54f;
            const float cx = 64f;
            const float cy = 64f;

            Image img = Raylib.GenImageColor(size, size, new Color(0, 0, 0, 0));

            // Paletas por planeta
            Color atmoCol, darkTerrain, midTerrain, lightTerrain, oceanCol, cloudCol;
            switch (planetName)
            {
                case "Hellmire":
                    atmoCol     = new Color(255, 110, 15, 255);
                    darkTerrain = new Color(26, 14, 12, 255);
                    midTerrain  = new Color(68, 28, 16, 255);
                    lightTerrain= new Color(170, 55, 20, 255);
                    oceanCol    = new Color(255, 140, 20, 255); // Ríos de Magma incandescente
                    cloudCol    = new Color(85, 45, 35, 190);   // Nubes de ceniza volcánica
                    break;

                case "Crimsica":
                    atmoCol     = new Color(225, 30, 65, 255);
                    darkTerrain = new Color(34, 14, 22, 255);
                    midTerrain  = new Color(92, 26, 45, 255);
                    lightTerrain= new Color(185, 45, 75, 255);
                    oceanCol    = new Color(55, 18, 38, 255);
                    cloudCol    = new Color(245, 160, 180, 180);
                    break;

                case "Vandalon IV":
                    atmoCol     = new Color(180, 225, 255, 255);
                    darkTerrain = new Color(90, 125, 155, 255);
                    midTerrain  = new Color(145, 185, 215, 255);
                    lightTerrain= new Color(230, 245, 255, 255); // Glaciares y cumbres
                    oceanCol    = new Color(42, 68, 98, 255);
                    cloudCol    = new Color(255, 255, 255, 210);
                    break;

                default: // Malevelon Creek
                    atmoCol     = new Color(0, 210, 255, 255);
                    darkTerrain = new Color(10, 18, 26, 255);
                    midTerrain  = new Color(18, 38, 48, 255);
                    lightTerrain= new Color(0, 140, 180, 255); // Jungla bioluminiscente
                    oceanCol    = new Color(6, 12, 18, 255);
                    cloudCol    = new Color(140, 220, 255, 175);
                    break;
            }

            Vector3 lightDir = Vector3.Normalize(new Vector3(-0.55f, -0.45f, 0.70f));

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float distSq = dx * dx + dy * dy;

                    // Atmósfera brillante exterior (halo pixel-art)
                    if (distSq > R * R && distSq <= (R + 6f) * (R + 6f))
                    {
                        float dist = MathF.Sqrt(distSq);
                        float atmoAlpha = (1f - (dist - R) / 6f);
                        Color aCol = new(atmoCol.R, atmoCol.G, atmoCol.B, (byte)(atmoAlpha * 140f));
                        Raylib.ImageDrawPixel(&img, x, y, aCol);
                        continue;
                    }

                    if (distSq <= R * R)
                    {
                        // Coordenadas esféricas 3D: z = sqrt(R^2 - x^2 - y^2)
                        float z = MathF.Sqrt(R * R - distSq);
                        Vector3 normal = new(dx / R, dy / R, z / R);

                        // Cel Shading Direccional (4 niveles de luz cuantizados)
                        float dot = Vector3.Dot(normal, lightDir);
                        int celLevel = dot switch
                        {
                            > 0.60f => 3, // Brillo máximo
                            > 0.20f => 2, // Tono medio
                            > -0.15f => 1, // Sombra
                            _ => 0        // Noche / sombra oscura
                        };

                        // Mapeo esférico (latitud, longitud)
                        float lat = MathF.Asin(dy / R);
                        float lon = MathF.Atan2(dx, z);

                        // Ruido Fractal FBM para Continentes / Océanos
                        float elevation = Fbm2D(lon * 2.2f + 5.5f, lat * 2.2f + 3.3f);
                        Color surfaceCol = elevation switch
                        {
                            < 0.42f => oceanCol,
                            < 0.62f => darkTerrain,
                            < 0.80f => midTerrain,
                            _ => lightTerrain
                        };

                        // Aplicar Cel Shading
                        Color shadedCol = ApplyCelShade(surfaceCol, celLevel);

                        // Capa de nubes / tormentas con rotación y dithering
                        float cloudNoise = Fbm2D(lon * 3.2f + 1.2f, lat * 3.2f + 0.8f);
                        if (cloudNoise > 0.58f && celLevel > 0)
                        {
                            // Dithering ajedrezado para transparencia pixelada
                            if ((x + y) % 2 == 0)
                            {
                                shadedCol = ApplyCelShade(cloudCol, celLevel);
                            }
                        }

                        // Resplandor atmosférico en el borde de la silueta
                        float edgeFalloff = 1f - (z / R);
                        if (edgeFalloff > 0.75f && celLevel > 0)
                        {
                            shadedCol = BlendColor(shadedCol, atmoCol, (edgeFalloff - 0.75f) * 2f);
                        }

                        Raylib.ImageDrawPixel(&img, x, y, shadedCol);
                    }
                }
            }

            Raylib.ExportImage(img, filePath);
            Raylib.UnloadImage(img);
        }

        // =========================================================================
        // 3. TILEMAPS PIXEL-ART 16-BIT DE SUPERFICIE (128x128 con rocas, magma y flora)
        // =========================================================================
        private static void GenerateAndSavePlanetTile(string planetName, string filePath)
        {
            const int size = 128;
            Image img = Raylib.GenImageColor(size, size, new Color(15, 23, 42, 255));
            Random rng = new(planetName.GetHashCode());

            Color baseCol, stoneCol, crackCol, detailCol;
            switch (planetName)
            {
                case "Hellmire":
                    baseCol   = new Color(38, 18, 14, 255);
                    stoneCol  = new Color(55, 26, 18, 255);
                    crackCol  = new Color(255, 110, 15, 255); // Magma
                    detailCol = new Color(255, 210, 40, 255);
                    break;
                case "Crimsica":
                    baseCol   = new Color(42, 16, 24, 255);
                    stoneCol  = new Color(72, 22, 34, 255);
                    crackCol  = new Color(200, 35, 70, 255); // Cristales rojos
                    detailCol = new Color(140, 30, 85, 255);
                    break;
                case "Vandalon IV":
                    baseCol   = new Color(165, 195, 225, 255);
                    stoneCol  = new Color(130, 160, 190, 255);
                    crackCol  = new Color(85, 160, 240, 255); // Hielo especular
                    detailCol = new Color(240, 250, 255, 255);
                    break;
                default: // Malevelon Creek
                    baseCol   = new Color(12, 22, 30, 255);
                    stoneCol  = new Color(20, 36, 46, 255);
                    crackCol  = new Color(0, 190, 225, 255); // Flora bioluminiscente
                    detailCol = new Color(40, 95, 120, 255);
                    break;
            }

            // Fondo base
            Raylib.ImageClearBackground(&img, baseCol);

            // Texturizado Pixel-Art de rocas y elevaciones en bloques de 2x2 y 4x4
            for (int i = 0; i < 220; i++)
            {
                int rx = (rng.Next(0, 64)) * 2;
                int ry = (rng.Next(0, 64)) * 2;
                int rw = rng.Next(1, 4) * 2;
                int rh = rng.Next(1, 3) * 2;
                Color col = (i % 2 == 0) ? stoneCol : detailCol;
                for (int y = ry; y < Math.Min(size, ry + rh); y++)
                    for (int x = rx; x < Math.Min(size, rx + rw); x++)
                        Raylib.ImageDrawPixel(&img, x, y, col);
            }

            // Grietas de terreno y vetas minerales
            for (int cr = 0; cr < 10; cr++)
            {
                int cx = rng.Next(4, 60) * 2;
                int cy = rng.Next(4, 60) * 2;
                int len = rng.Next(4, 10);
                for (int step = 0; step < len; step++)
                {
                    int px = (cx + step * 2) % size;
                    int py = (cy + (rng.Next(-1, 2) * 2)) % size;
                    Raylib.ImageDrawPixel(&img, px, py, crackCol);
                    Raylib.ImageDrawPixel(&img, px + 1, py, detailCol);
                }
            }

            Raylib.ExportImage(img, filePath);
            Raylib.UnloadImage(img);
        }

        // =========================================================================
        // 4. MÉTODOS DE DIBUJO CINEMÁTICO EN PANTALLA (RENDERIZADO A 144 FPS)
        // =========================================================================

        /// <summary>
        /// Dibuja la explosión del hongo nuclear de 12 frames a escala x4 sin círculos lisos.
        /// </summary>
        public static void Draw500kgExplosionSprite(Vector2 worldPos, float progress)
        {
            if (!_initialized) Initialize();
            if (_tex500kgSheet.Id == 0) return;

            int frame = Math.Clamp((int)(progress * 12f), 0, 11);
            int frameW = 96;
            int frameH = 96;

            Rectangle src = new(frame * frameW, 0, frameW, frameH);
            float scale = 4.2f;
            Rectangle dst = new(worldPos.X, worldPos.Y, frameW * scale, frameH * scale);
            Vector2 origin = new(dst.Width * 0.5f, dst.Height * 0.88f); // Base anclada al suelo

            Raylib.DrawTexturePro(_tex500kgSheet, src, dst, origin, 0f, Color.White);
        }

        /// <summary>
        /// Dibuja la esfera planetaria 3D pixel-art animada en el puente de mando con naves orbitando y destellos.
        /// </summary>
        public static void DrawGalacticHoloPlanet(Vector2 center, float radius, string planetName, float time)
        {
            if (!_initialized) Initialize();
            if (!_planetTextures.TryGetValue(planetName, out Texture2D tex) || tex.Id == 0)
                tex = _planetTextures.Values.FirstOrDefault();

            float scale = (radius * 2.3f) / 128f;
            Rectangle src = new(0, 0, tex.Width, tex.Height);
            Rectangle dst = new(center.X, center.Y, tex.Width * scale, tex.Height * scale);
            Vector2 origin = new(dst.Width * 0.5f, dst.Height * 0.5f);

            // 1. Textura Esférica Pixel-Art
            Raylib.DrawTexturePro(tex, src, dst, origin, 0f, Color.White);

            // 2. Destellos orbitales ocasionales (bombardeos / combate en superficie)
            float flash = MathF.Sin(time * 14f + planetName.GetHashCode() % 10);
            if (flash > 0.82f)
            {
                Vector2 flashPos = center + new Vector2(
                    MathF.Cos(time * 3f) * (radius * 0.45f),
                    MathF.Sin(time * 4f) * (radius * 0.35f)
                );
                Raylib.DrawRectangle((int)flashPos.X - 1, (int)flashPos.Y - 1, 3, 3, ColYellow);
                Raylib.DrawRectangle((int)flashPos.X, (int)flashPos.Y, 1, 1, Color.White);
            }

            // 3. Destructores de la Super Tierra orbitando en formación alrededor del planeta
            for (int ship = 0; ship < 3; ship++)
            {
                float orbitAngle = time * 0.35f + ship * (MathF.PI * 2f / 3f);
                float orbitR = radius * 1.35f;
                Vector2 shipPos = center + new Vector2(MathF.Cos(orbitAngle) * orbitR, MathF.Sin(orbitAngle) * (orbitR * 0.42f));

                // Silueta pixelada de destructor Super Tierra (10x4 píxeles)
                Raylib.DrawRectangle((int)shipPos.X - 5, (int)shipPos.Y - 2, 10, 4, ProceduralAssetRenderer.GunmetalDark);
                Raylib.DrawRectangle((int)shipPos.X + 2, (int)shipPos.Y - 1, 4, 2, ProceduralAssetRenderer.GunmetalLight);
                Raylib.DrawRectangle((int)shipPos.X - 6, (int)shipPos.Y, 2, 2, ProceduralAssetRenderer.TacticalCyan); // Propulsor
            }
        }

        /// <summary>
        /// Dibuja el fondo cósmico cinemático a pantalla completa con el planeta gigante en movimiento,
        /// campo estelar multicapa, nebulosas, anillos planetarios y destructores en órbita baja.
        /// </summary>
        public static void DrawCinematicPlanetBackdrop(
            string planetName,
            float time,
            int screenW,
            int screenH,
            Vector2 parallaxOffset,
            float alpha = 1.0f,
            bool showOverlayVignette = true)
        {
            if (!_initialized) Initialize();
            if (!_planetTextures.TryGetValue(planetName, out Texture2D tex) || tex.Id == 0)
                tex = _planetTextures.Values.FirstOrDefault();

            // 1. Campo de Estrellas Profundo Multicapa con Parallax
            int seed = Math.Abs(planetName.GetHashCode());
            for (int i = 0; i < 90; i++)
            {
                float starX = (MathF.Abs(Hash2D(i * 17 + seed, 101)) * screenW + parallaxOffset.X * 0.02f) % screenW;
                float starY = (MathF.Abs(Hash2D(i * 31 + seed, 203)) * screenH + parallaxOffset.Y * 0.02f) % screenH;
                if (starX < 0) starX += screenW;
                if (starY < 0) starY += screenH;
                float twinkle = 0.5f + 0.5f * MathF.Sin(time * 3f + i * 2.1f);
                byte starAlpha = (byte)(Math.Clamp((60 + (int)(twinkle * 180)) * alpha, 0, 255));
                Color starCol = (i % 5 == 0) ? new Color(180, 220, 255, (int)starAlpha) :
                                (i % 7 == 0) ? new Color(255, 230, 180, (int)starAlpha) :
                                new Color(240, 245, 255, (int)starAlpha);
                int sz = (i % 12 == 0) ? 2 : 1;
                Raylib.DrawRectangle((int)starX, (int)starY, sz, sz, starCol);
            }

            // 2. Resplandor Nebular Atmosférico según el Planeta
            Color nebCol = planetName switch
            {
                "Hellmire" => new Color(140, 40, 15, (int)(55 * alpha)),
                "Crimsica" => new Color(120, 20, 60, (int)(50 * alpha)),
                "Vandalon IV" => new Color(20, 60, 120, (int)(55 * alpha)),
                _ => new Color(15, 75, 105, (int)(55 * alpha)) // Malevelon Creek
            };
            Raylib.DrawCircleGradient(new Vector2(screenW * 0.72f, screenH * 0.45f), screenW * 0.55f, nebCol, Color.Blank);

            // 3. El Planeta Gigante en Movimiento Lento
            float planetR = MathF.Min(screenW, screenH) * 0.36f;
            // Movimiento suave sinusoidal + parallax
            float driftX = MathF.Sin(time * 0.08f) * 16f;
            float driftY = MathF.Cos(time * 0.06f) * 12f;
            Vector2 pCenter = new(
                screenW * 0.74f + driftX + parallaxOffset.X * 0.04f,
                screenH * 0.42f + driftY + parallaxOffset.Y * 0.04f
            );

            // Halo atmosférico exterior
            Color atmCol = planetName switch
            {
                "Hellmire" => new Color(255, 120, 30, (int)(65 * alpha)),
                "Crimsica" => new Color(255, 60, 90, (int)(60 * alpha)),
                "Vandalon IV" => new Color(100, 180, 255, (int)(70 * alpha)),
                _ => new Color(60, 200, 240, (int)(75 * alpha))
            };
            Raylib.DrawCircleV(pCenter, planetR * 1.08f, atmCol);
            Raylib.DrawCircleV(pCenter, planetR * 1.03f, new Color(atmCol.R, atmCol.G, atmCol.B, (byte)(atmCol.A * 1.4f)));

            // Anillos planetarios (en perspectiva) para Malevelon y Vandalon
            bool hasRings = planetName == "Malevelon Creek" || planetName == "Vandalon IV";
            if (hasRings)
            {
                Color ringCol = planetName == "Malevelon Creek" 
                    ? new Color(80, 160, 190, (int)(110 * alpha)) 
                    : new Color(160, 190, 220, (int)(130 * alpha));
                float ringRx = planetR * 1.85f;
                float ringRy = planetR * 0.42f;
                for (int r = 0; r < 5; r++)
                {
                    Raylib.DrawEllipseLines((int)pCenter.X, (int)pCenter.Y, (int)(ringRx + r * 5), (int)(ringRy + r * 1.8f), ringCol);
                }
            }

            // Cuerpo del planeta
            if (tex.Id != 0)
            {
                float planetScale = (planetR * 2.05f) / tex.Width;
                Rectangle srcP = new(0, 0, tex.Width, tex.Height);
                Rectangle dstP = new(pCenter.X, pCenter.Y, tex.Width * planetScale, tex.Height * planetScale);
                Vector2 origP = new(dstP.Width * 0.5f, dstP.Height * 0.5f);
                Raylib.DrawTexturePro(tex, srcP, dstP, origP, 0f, new Color(255, 255, 255, (int)(255 * alpha)));
            }
            else
            {
                Raylib.DrawCircleV(pCenter, planetR, atmCol);
            }

            // Sombra esférica / terminador día-noche
            Raylib.DrawCircleGradient(
                new Vector2(pCenter.X + planetR * 0.35f, pCenter.Y + planetR * 0.30f),
                planetR * 0.95f,
                Color.Blank,
                new Color(5, 8, 14, (int)(210 * alpha))
            );

            // 4. Destellos de combate y bombardeos orbitales en la superficie
            float flash = MathF.Sin(time * 12f + seed % 13);
            if (flash > 0.84f)
            {
                float fAngle = time * 2.2f + seed;
                Vector2 fPos = pCenter + new Vector2(MathF.Cos(fAngle) * (planetR * 0.48f), MathF.Sin(fAngle) * (planetR * 0.36f));
                Raylib.DrawCircleV(fPos, 4f, new Color(255, 235, 120, (int)(230 * alpha)));
                Raylib.DrawCircleV(fPos, 2f, Color.White);
            }

            // 5. Flota de Destructores de la Super Tierra en órbita
            for (int s = 0; s < 4; s++)
            {
                float sAngle = time * 0.18f + s * (MathF.PI * 0.5f);
                float sOrbitR = planetR * (1.25f + s * 0.15f);
                Vector2 sPos = pCenter + new Vector2(MathF.Cos(sAngle) * sOrbitR, MathF.Sin(sAngle) * (sOrbitR * 0.35f));

                // Silueta estilizada de Super Destroyer (18x6 px)
                Raylib.DrawRectangle((int)sPos.X - 9, (int)sPos.Y - 3, 18, 6, ProceduralAssetRenderer.GunmetalDark);
                Raylib.DrawRectangle((int)sPos.X + 3, (int)sPos.Y - 2, 7, 4, ProceduralAssetRenderer.GunmetalLight);
                // Propulsores azules de iones
                Raylib.DrawRectangle((int)sPos.X - 11, (int)sPos.Y - 1, 3, 2, ProceduralAssetRenderer.TacticalCyan);
                // Luces de baliza
                if (MathF.Sin(time * 6f + s) > 0.2f)
                {
                    Raylib.DrawRectangle((int)sPos.X + 7, (int)sPos.Y - 4, 2, 2, Constants.DemocracyRed);
                }
            }

            // 6. Viñeta cinematográfica en los bordes para profundidad
            if (showOverlayVignette)
            {
                Raylib.DrawRectangleGradientV(0, 0, screenW, 80, new Color(4, 7, 12, (int)(220 * alpha)), Color.Blank);
                Raylib.DrawRectangleGradientV(0, screenH - 90, screenW, 90, Color.Blank, new Color(4, 7, 12, (int)(240 * alpha)));
                Raylib.DrawRectangleGradientH(0, 0, 120, screenH, new Color(4, 7, 12, (int)(200 * alpha)), Color.Blank);
                Raylib.DrawRectangleGradientH(screenW - 120, 0, 120, screenH, Color.Blank, new Color(4, 7, 12, (int)(200 * alpha)));
            }
        }

        // =========================================================================
        // HELPERS MATEMÁTICOS DE RUIDO FRACTAL Y COLOR
        // =========================================================================
        private static float Fbm2D(float x, float y)
        {
            float total = 0f;
            float freq = 1f;
            float amp = 1f;
            float max = 0f;

            for (int i = 0; i < 3; i++)
            {
                total += SimpleNoise(x * freq, y * freq) * amp;
                max += amp;
                amp *= 0.5f;
                freq *= 2.05f;
            }

            return total / max;
        }

        private static float SimpleNoise(float x, float y)
        {
            int xi = (int)MathF.Floor(x);
            int yi = (int)MathF.Floor(y);
            float xf = x - xi;
            float yf = y - yi;

            float d00 = Hash2D(xi, yi);
            float d10 = Hash2D(xi + 1, yi);
            float d01 = Hash2D(xi, yi + 1);
            float d11 = Hash2D(xi + 1, yi + 1);

            float u = xf * xf * (3.0f - 2.0f * xf);
            float v = yf * yf * (3.0f - 2.0f * yf);

            return (d00 * (1 - u) + d10 * u) * (1 - v) + (d01 * (1 - u) + d11 * u) * v;
        }

        private static float Hash2D(int x, int y)
        {
            int n = x * 374761393 + y * 668265263;
            n = (n ^ (n >> 13)) * 1274126177;
            return (n & 0x7fffffff) / (float)0x7fffffff;
        }

        private static Color ApplyCelShade(Color col, int celLevel)
        {
            float mult = celLevel switch
            {
                3 => 1.25f,
                2 => 1.0f,
                1 => 0.65f,
                _ => 0.28f
            };

            return new Color(
                (byte)Math.Clamp((int)(col.R * mult), 0, 255),
                (byte)Math.Clamp((int)(col.G * mult), 0, 255),
                (byte)Math.Clamp((int)(col.B * mult), 0, 255),
                col.A
            );
        }

        private static Color BlendColor(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Color(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t),
                a.A
            );
        }

        private static void DrawPixelCircleOnImage(Image img, int cx, int cy, int radius, Color col)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int px = cx + x;
                        int py = cy + y;
                        if (px >= 0 && px < img.Width && py >= 0 && py < img.Height)
                        {
                            Raylib.ImageDrawPixel(&img, px, py, col);
                        }
                    }
                }
            }
        }

        private static void DrawPixelEllipseOnImage(Image img, int cx, int cy, int rx, int ry, Color col)
        {
            for (int y = -ry; y <= ry; y++)
            {
                for (int x = -rx; x <= rx; x++)
                {
                    if ((x * x) / (float)(rx * rx) + (y * y) / (float)(ry * ry) <= 1.0f)
                    {
                        int px = cx + x;
                        int py = cy + y;
                        if (px >= 0 && px < img.Width && py >= 0 && py < img.Height)
                        {
                            Raylib.ImageDrawPixel(&img, px, py, col);
                        }
                    }
                }
            }
        }

        public static void Unload()
        {
            if (!_initialized) return;
            if (_tex500kgSheet.Id > 0) Raylib.UnloadTexture(_tex500kgSheet);
            foreach (var kvp in _planetTextures)
            {
                if (kvp.Value.Id > 0) Raylib.UnloadTexture(kvp.Value);
            }
            _planetTextures.Clear();
            foreach (var kvp in _planetTileTextures)
            {
                if (kvp.Value.Id > 0) Raylib.UnloadTexture(kvp.Value);
            }
            _planetTileTextures.Clear();
            _initialized = false;
        }
    }
}
