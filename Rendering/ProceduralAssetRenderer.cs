using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using HelldiversDesktop.Models;

namespace HelldiversDesktop.Rendering
{
    /// <summary>
    /// Generador y Animador Procedural de Pixel-Art 16-Bit HD para Raylib-cs.
    /// Incluye: Armas con fogonazo/recoil, Biomas de Planetas (Tilemaps + Clima),
    /// Estratagemas Cinemáticas (500kg, Orbital Laser, Hellpods, Torretas),
    /// Estructuras de Misión (Nidos, Fábricas, ICBM) y Pelican-1 VTOL.
    /// </summary>
    public static class ProceduralAssetRenderer
    {
        // Caché de texturas Pixel-Art pre-renderizadas a baja resolución y escaladas con Point Filter
        private static readonly Dictionary<string, Texture2D> _weaponTextures = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Texture2D> _biomeTiles = new(StringComparer.OrdinalIgnoreCase);
        private static Texture2D _pelicanTexture;
        private static Texture2D _hellpodTexture;
        private static bool _initialized = false;

        // Estado para física de capa (Verlet) del Helldiver
        private static readonly Vector2[] _capeNodes = new Vector2[6];

        // Paleta Oficial Super Tierra & 16-bit Shading
        public static readonly Color SuperEarthYellow = new(255, 232, 0, 255);
        public static readonly Color TacticalCyan = new(0, 228, 255, 255);
        public static readonly Color AutomatonRed = new(255, 45, 45, 255);
        public static readonly Color TerminidBile = new(165, 255, 30, 255);
        public static readonly Color GunmetalDark = new(32, 36, 42, 255);
        public static readonly Color GunmetalLight = new(78, 86, 98, 255);
        public static readonly Color OutlineBlack = new(12, 12, 16, 255);

        // =========================================================================
        // 1. INICIALIZACIÓN Y PRE-RENDERIZADO DE SPRITES PIXEL-ART EN CACHÉ
        // =========================================================================
        public static void Initialize()
        {
            if (_initialized) return;

            // Generar sprites Pixel-Art 32x16 para todo el Arsenal
            string[] weapons = {
                "Liberator", "Breaker", "Punisher", "Dominator",
                "Scythe", "Sickle", "Scorcher", "Diligence", "Quasar", "Autocannon", "Flamethrower", "Railgun"
            };

            foreach (var w in weapons)
            {
                _weaponTextures[w] = GenerateWeaponPixelSprite(w);
            }

            // Generar Tilemaps 128x128 Pixel-Art para los Planetas
            string[] planets = { "Malevelon Creek", "Hellmire", "Crimsica", "Vandalon IV", "Cyberstan", "Calypso", "Squ'th" };
            foreach (var p in planets)
            {
                _biomeTiles[p] = GeneratePlanetBiomeTile(p);
            }

            _pelicanTexture = GeneratePelicanSprite();
            _hellpodTexture = GenerateHellpodSprite();

            _initialized = true;
        }

        // =========================================================================
        // 2. DISEÑO PIXEL-ART DE ARMAS (CACHE + ANIMACIÓN DE DISPARO / RECOIL)
        // =========================================================================
        private static Texture2D GenerateWeaponPixelSprite(string weaponName)
        {
            // Lienzo expandido de 48x18 píxeles para alta fidelidad de silueta
            RenderTexture2D rt = Raylib.LoadRenderTexture(48, 18);
            Raylib.BeginTextureMode(rt);
            Raylib.ClearBackground(new Color(0, 0, 0, 0));

            // Colores auxiliares específicos
            Color polymerBlack = new(18, 20, 24, 255);
            Color polymerOlive = new(38, 48, 42, 255);
            Color steelGray = new(92, 102, 115, 255);
            Color brassGold = new(218, 165, 32, 255);
            Color flameOrange = new(255, 115, 0, 255);

            switch (weaponName)
            {
                case "Liberator": // AR-23 Rifle de Asalto Bullpup Oficial
                    // Silueta base exterior
                    DrawPixelRect(5, 5, 38, 8, OutlineBlack);
                    // Receptor Bullpup en polímero militar oscuro
                    DrawPixelRect(6, 6, 36, 6, polymerBlack);
                    DrawPixelRect(14, 6, 20, 5, polymerOlive);
                    // Franja dorada/amarilla reglamentaria de Super Tierra en el lateral
                    DrawPixelRect(16, 7, 16, 2, SuperEarthYellow);
                    // Riel superior Picatinny / Asa de transporte con visor
                    DrawPixelRect(12, 3, 20, 3, OutlineBlack);
                    DrawPixelRect(13, 4, 18, 2, GunmetalDark);
                    DrawPixelRect(20, 2, 4, 2, TacticalCyan); // Punto rojo holográfico
                    // Cargador curvo trasero (configuración Bullpup: detrás de la empuñadura)
                    DrawPixelRect(9, 11, 5, 6, OutlineBlack);
                    DrawPixelRect(10, 11, 4, 5, new Color(25, 28, 34, 255));
                    DrawPixelRect(10, 15, 4, 1, SuperEarthYellow); // Base amarilla
                    // Empuñadura ergonómica inclinada y guardamontes
                    DrawPixelRect(19, 10, 4, 6, OutlineBlack);
                    DrawPixelRect(20, 10, 3, 5, polymerBlack);
                    DrawPixelRect(23, 11, 1, 3, OutlineBlack); // Guardamontes
                    // Guardamanos ventilado delantero
                    DrawPixelRect(28, 7, 9, 4, GunmetalDark);
                    DrawPixelRect(30, 8, 2, 2, OutlineBlack);
                    DrawPixelRect(34, 8, 2, 2, OutlineBlack);
                    // Cañón externo y freno de boca birdcage ranurado
                    DrawPixelRect(37, 8, 6, 3, steelGray);
                    DrawPixelRect(43, 7, 3, 5, OutlineBlack);
                    DrawPixelRect(44, 8, 2, 3, GunmetalDark);
                    break;

                case "Breaker": // SG-225 Escopeta Automática Pesada con Tambor
                    // Silueta exterior
                    DrawPixelRect(4, 4, 38, 9, OutlineBlack);
                    DrawPixelRect(5, 5, 36, 7, polymerBlack);
                    // Cubierta térmica ranurada sobre el cañón
                    DrawPixelRect(16, 4, 20, 3, OutlineBlack);
                    DrawPixelRect(17, 5, 18, 2, GunmetalLight);
                    for (int v = 0; v < 4; v++) DrawPixelRect(19 + v * 4, 5, 2, 1, OutlineBlack);
                    // Culata esquelética rebatible de acero
                    DrawPixelRect(4, 6, 8, 2, steelGray);
                    DrawPixelRect(4, 8, 3, 4, OutlineBlack);
                    // Enorme tambor de munición cilíndrico bajo la recámara
                    DrawPixelRect(18, 10, 11, 7, OutlineBlack);
                    DrawPixelRect(19, 11, 9, 6, GunmetalDark);
                    DrawPixelRect(21, 12, 5, 4, new Color(50, 58, 70, 255));
                    DrawPixelRect(23, 13, 2, 2, SuperEarthYellow); // Centro del tambor
                    // Empuñadura
                    DrawPixelRect(13, 10, 4, 6, OutlineBlack);
                    DrawPixelRect(14, 10, 3, 5, polymerBlack);
                    // Doble cañón ancho calibre 12
                    DrawPixelRect(36, 6, 6, 5, steelGray);
                    DrawPixelRect(42, 5, 3, 7, OutlineBlack);
                    DrawPixelRect(43, 6, 2, 5, GunmetalDark);
                    break;

                case "Punisher": // SG-8 Escopeta de Bombeo Manual Clásica
                    // Culata fija completa sólida
                    DrawPixelRect(3, 6, 11, 7, OutlineBlack);
                    DrawPixelRect(4, 7, 9, 5, new Color(42, 44, 48, 255));
                    DrawPixelRect(3, 7, 2, 5, new Color(20, 22, 26, 255)); // Cantonera de goma
                    // Receptor principal con ventana de expulsión
                    DrawPixelRect(14, 6, 12, 6, OutlineBlack);
                    DrawPixelRect(15, 7, 10, 4, GunmetalDark);
                    DrawPixelRect(17, 7, 4, 2, steelGray); // Puerto de eyección
                    // Empuñadura tradicional
                    DrawPixelRect(12, 11, 4, 6, OutlineBlack);
                    DrawPixelRect(13, 11, 3, 4, polymerBlack);
                    // Cañón largo superior con banda ventilada
                    DrawPixelRect(26, 6, 17, 3, OutlineBlack);
                    DrawPixelRect(26, 7, 16, 2, steelGray);
                    DrawPixelRect(42, 5, 2, 2, brassGold); // Punto de mira de latón
                    // Tubo cargador inferior largo
                    DrawPixelRect(24, 9, 16, 3, OutlineBlack);
                    DrawPixelRect(25, 10, 14, 2, GunmetalDark);
                    // Guardamanos móvil de bombeo ranurado
                    DrawPixelRect(25, 9, 9, 5, OutlineBlack);
                    DrawPixelRect(26, 10, 7, 3, new Color(130, 65, 30, 255)); // Madera/polímero rugoso
                    for (int g = 0; g < 3; g++) DrawPixelRect(27 + g * 2, 10, 1, 3, OutlineBlack);
                    break;

                case "Sickle": // LAS-16 Carabina Láser con Aletas de Disipación
                    // Carcasa compacta futurista
                    DrawPixelRect(6, 4, 34, 8, OutlineBlack);
                    DrawPixelRect(7, 5, 32, 6, GunmetalDark);
                    DrawPixelRect(10, 4, 18, 2, SuperEarthYellow); // Cubierta superior dorada
                    // 6 Aletas de disipación térmica transversales
                    for (int f = 0; f < 6; f++)
                    {
                        DrawPixelRect(12 + f * 3, 2, 2, 3, steelGray);
                        DrawPixelRect(12 + f * 3, 3, 1, 2, TacticalCyan); // Resplandor térmico
                    }
                    // Batería / Célula de energía transparente
                    DrawPixelRect(14, 7, 10, 3, OutlineBlack);
                    DrawPixelRect(15, 8, 8, 2, TacticalCyan);
                    // Empuñadura y guardamontes cerrado
                    DrawPixelRect(11, 10, 4, 6, OutlineBlack);
                    DrawPixelRect(12, 10, 3, 5, polymerBlack);
                    // Boca de enfoque óptico con lente de cuarzo
                    DrawPixelRect(34, 6, 6, 5, OutlineBlack);
                    DrawPixelRect(35, 7, 5, 3, GunmetalLight);
                    DrawPixelRect(40, 7, 2, 3, Color.White); // Lente
                    break;

                case "Dominator": // JAR-5 Lanzador Giroscópico Pesado
                    // Chasis masivo y angulado estilo Bolter
                    DrawPixelRect(4, 3, 38, 10, OutlineBlack);
                    DrawPixelRect(5, 4, 36, 8, new Color(40, 46, 52, 255));
                    // Franjas de advertencia tácticas en el cajón
                    DrawPixelRect(10, 5, 14, 3, SuperEarthYellow);
                    DrawPixelRect(12, 5, 3, 3, OutlineBlack);
                    DrawPixelRect(18, 5, 3, 3, OutlineBlack);
                    // Cargador curvo frontal pesado
                    DrawPixelRect(20, 11, 7, 6, OutlineBlack);
                    DrawPixelRect(21, 12, 5, 5, new Color(130, 40, 40, 255)); // Caja blindada roja
                    // Empuñadura de gran calibre
                    DrawPixelRect(12, 11, 5, 6, OutlineBlack);
                    DrawPixelRect(13, 11, 4, 5, polymerBlack);
                    // Enorme boca de cañón cuadrada con compensador de retroceso
                    DrawPixelRect(34, 4, 10, 8, OutlineBlack);
                    DrawPixelRect(35, 5, 8, 6, GunmetalDark);
                    DrawPixelRect(38, 3, 3, 3, OutlineBlack); // Venteo superior
                    DrawPixelRect(43, 6, 2, 4, new Color(20, 20, 24, 255)); // Boca gigante
                    break;

                case "Diligence": // R-63 Diligence CS - Rifle de Francotirador
                    // Cuerpo largo y estilizado
                    DrawPixelRect(2, 6, 42, 6, OutlineBlack);
                    DrawPixelRect(3, 7, 40, 4, GunmetalDark);
                    DrawPixelRect(6, 8, 10, 2, GunmetalLight); // Culata con carrilera
                    // Mira telescópica óptica elevada de alta magnificación
                    DrawPixelRect(14, 2, 14, 4, OutlineBlack);
                    DrawPixelRect(15, 3, 12, 2, steelGray);
                    DrawPixelRect(14, 2, 3, 4, OutlineBlack); // Parasol delantero
                    DrawPixelRect(26, 3, 2, 2, TacticalCyan); // Lente antirreflejos
                    DrawPixelRect(18, 5, 2, 2, OutlineBlack); // Montura anular 1
                    DrawPixelRect(23, 5, 2, 2, OutlineBlack); // Montura anular 2
                    // Cargador recto de tirador
                    DrawPixelRect(18, 11, 4, 6, OutlineBlack);
                    DrawPixelRect(19, 11, 3, 5, steelGray);
                    // Empuñadura de precisión
                    DrawPixelRect(12, 10, 4, 6, OutlineBlack);
                    DrawPixelRect(13, 10, 3, 5, polymerBlack);
                    // Cañón largo flotante de competición y freno de boca táctico
                    DrawPixelRect(30, 7, 12, 3, steelGray);
                    DrawPixelRect(42, 6, 4, 5, OutlineBlack);
                    DrawPixelRect(43, 7, 3, 3, GunmetalDark);
                    // Bípode plegado bajo el guardamanos
                    DrawPixelRect(28, 10, 8, 2, steelGray);
                    DrawPixelRect(34, 11, 2, 3, steelGray);
                    break;

                case "Scorcher": // PLAS-1 Rifle de Plasma con Bobinas Magnéticas
                    DrawPixelRect(4, 4, 36, 8, OutlineBlack);
                    DrawPixelRect(5, 5, 34, 6, new Color(28, 36, 56, 255));
                    // 4 Bobinas toroidales de confinamiento de plasma brillante
                    for (int c = 0; c < 4; c++)
                    {
                        DrawPixelRect(14 + c * 5, 3, 3, 4, OutlineBlack);
                        DrawPixelRect(14 + c * 5, 4, 2, 2, TacticalCyan);
                        DrawPixelRect(15 + c * 5, 4, 1, 1, Color.White); // Centro brillante
                    }
                    // Batería de plasma angular inferior
                    DrawPixelRect(15, 11, 5, 6, OutlineBlack);
                    DrawPixelRect(16, 11, 4, 5, TacticalCyan);
                    DrawPixelRect(17, 13, 2, 2, Color.White);
                    // Empuñadura
                    DrawPixelRect(9, 10, 4, 6, OutlineBlack);
                    DrawPixelRect(10, 10, 3, 5, polymerBlack);
                    // Boquilla de descarga de plasma con difusor cerámico
                    DrawPixelRect(35, 5, 7, 6, OutlineBlack);
                    DrawPixelRect(36, 6, 5, 4, new Color(50, 75, 110, 255));
                    DrawPixelRect(41, 6, 2, 4, TacticalCyan);
                    break;

                case "Flamethrower": // FLAM-40 Lanzallamas Pesado
                    DrawPixelRect(4, 4, 38, 9, OutlineBlack);
                    DrawPixelRect(5, 5, 36, 7, new Color(55, 45, 35, 255));
                    // Tanque cilíndrico de napalm amarillo brillante
                    DrawPixelRect(14, 10, 13, 7, OutlineBlack);
                    DrawPixelRect(15, 11, 11, 5, SuperEarthYellow);
                    DrawPixelRect(17, 12, 7, 3, flameOrange);
                    DrawPixelRect(13, 11, 2, 3, AutomatonRed); // Válvula reguladora roja
                    // Manguera reforzada hacia el cañón
                    DrawPixelRect(10, 9, 4, 3, OutlineBlack);
                    // Empuñadura delantera y trasera
                    DrawPixelRect(8, 9, 4, 6, OutlineBlack);
                    DrawPixelRect(9, 10, 3, 5, polymerBlack);
                    // Protector térmico perforado de cañón
                    DrawPixelRect(28, 5, 10, 5, OutlineBlack);
                    DrawPixelRect(29, 6, 8, 3, steelGray);
                    DrawPixelRect(31, 6, 2, 2, OutlineBlack);
                    DrawPixelRect(35, 6, 2, 2, OutlineBlack);
                    // Soporte de llama piloto y chispero ignitor
                    DrawPixelRect(38, 6, 5, 4, GunmetalDark);
                    DrawPixelRect(42, 5, 2, 3, SuperEarthYellow);
                    DrawPixelRect(43, 6, 2, 2, Color.White); // Llama piloto
                    break;

                case "Autocannon": // AC-8 Cañón Automático Pesado de 20mm
                    // Cajón de mecanismos blindado
                    DrawPixelRect(4, 4, 40, 8, OutlineBlack);
                    DrawPixelRect(5, 5, 38, 6, GunmetalDark);
                    // Cargador superior de 5 rondas de 20mm con puntas de ojivas doradas visibles
                    DrawPixelRect(16, 1, 9, 5, OutlineBlack);
                    DrawPixelRect(17, 2, 7, 4, new Color(40, 48, 55, 255));
                    for (int b = 0; b < 3; b++) DrawPixelRect(18 + b * 2, 0, 1, 3, brassGold); // Balas de 20mm
                    // Empuñadura y gatillo doble
                    DrawPixelRect(11, 10, 5, 6, OutlineBlack);
                    DrawPixelRect(12, 10, 4, 5, polymerBlack);
                    // Enorme cañón acanalado extendido de 20mm
                    DrawPixelRect(26, 6, 15, 4, OutlineBlack);
                    DrawPixelRect(27, 7, 14, 2, steelGray);
                    // Freno de boca ranurado gigante estilo artillería
                    DrawPixelRect(40, 4, 6, 8, OutlineBlack);
                    DrawPixelRect(41, 5, 4, 6, GunmetalDark);
                    DrawPixelRect(42, 4, 2, 8, OutlineBlack); // Ranuras del freno
                    // Bípode frontal plegado
                    DrawPixelRect(32, 10, 6, 3, steelGray);
                    break;

                case "Railgun": // RS-422 Cañón de Riel Electromagnético
                    DrawPixelRect(3, 4, 42, 9, OutlineBlack);
                    DrawPixelRect(4, 5, 40, 7, GunmetalDark);
                    // Dos rieles superconductores paralelos
                    DrawPixelRect(14, 4, 28, 2, steelGray); // Riel superior
                    DrawPixelRect(14, 11, 28, 2, steelGray); // Riel inferior
                    // Canal de aceleración electromagnética (energía cian pulsante)
                    DrawPixelRect(16, 7, 26, 3, TacticalCyan);
                    for (int s = 0; s < 4; s++) DrawPixelRect(18 + s * 6, 8, 2, 1, Color.White);
                    // Batería de condensadores trasera con indicador LED
                    DrawPixelRect(5, 6, 8, 5, OutlineBlack);
                    DrawPixelRect(6, 7, 6, 3, new Color(20, 40, 30, 255));
                    DrawPixelRect(7, 8, 4, 1, TerminidBile); // LED verde de carga completa
                    // Empuñadura esqueletizada ergonómica
                    DrawPixelRect(11, 10, 4, 6, OutlineBlack);
                    DrawPixelRect(12, 10, 3, 5, polymerBlack);
                    // Anillos de confinamiento magnético externos
                    DrawPixelRect(24, 3, 2, 11, OutlineBlack);
                    DrawPixelRect(34, 3, 2, 11, OutlineBlack);
                    break;

                case "Quasar": // LAS-99 Cañón Quásar de Hombro
                    // Armazón masivo para apoyo al hombro
                    DrawPixelRect(2, 3, 42, 11, OutlineBlack);
                    DrawPixelRect(3, 4, 40, 9, new Color(34, 42, 54, 255));
                    // Contorno del apoyo al hombro acolchado
                    DrawPixelRect(6, 12, 10, 4, OutlineBlack);
                    DrawPixelRect(7, 13, 8, 3, new Color(20, 25, 30, 255));
                    // Enfriador / Radiador circular cuántico central
                    DrawPixelRect(16, 2, 12, 12, OutlineBlack);
                    DrawPixelRect(17, 3, 10, 10, GunmetalDark);
                    DrawPixelRect(20, 6, 4, 4, TacticalCyan);
                    DrawPixelRect(21, 7, 2, 2, Color.White);
                    // Franja amarilla Helldiver sobre el fuselaje
                    DrawPixelRect(26, 5, 12, 2, SuperEarthYellow);
                    // Doble empuñadura de control
                    DrawPixelRect(12, 10, 3, 5, polymerBlack);
                    DrawPixelRect(28, 12, 3, 4, polymerBlack);
                    // Gran campana emisora frontal con lente cóncava
                    DrawPixelRect(36, 4, 8, 9, OutlineBlack);
                    DrawPixelRect(37, 5, 6, 7, steelGray);
                    DrawPixelRect(42, 6, 2, 5, TacticalCyan);
                    break;

                case "Scythe": // LAS-5 Rifle Láser Continuo
                    DrawPixelRect(5, 5, 36, 7, OutlineBlack);
                    DrawPixelRect(6, 6, 34, 5, GunmetalDark);
                    DrawPixelRect(8, 4, 18, 2, SuperEarthYellow); // Carcasa amarilla
                    DrawPixelRect(14, 7, 12, 3, TacticalCyan); // Núcleo
                    DrawPixelRect(10, 10, 4, 6, OutlineBlack);
                    DrawPixelRect(11, 10, 3, 5, polymerBlack);
                    DrawPixelRect(36, 6, 5, 4, steelGray);
                    DrawPixelRect(40, 7, 2, 2, Color.White);
                    break;

                default: // Fallback
                    DrawPixelRect(6, 6, 34, 6, OutlineBlack);
                    DrawPixelRect(7, 7, 32, 4, GunmetalDark);
                    DrawPixelRect(12, 11, 4, 5, SuperEarthYellow);
                    break;
            }

            Raylib.EndTextureMode();
            Raylib.SetTextureFilter(rt.Texture, TextureFilter.Point);
            return rt.Texture;
        }

        /// <summary>
        /// Dibuja la vista previa del arma pixel-art en la interfaz (Armería / HUD táctico).
        /// </summary>
        public static void DrawWeaponPreview(string weaponKey, Vector2 centerPos, float scale = 2.0f, float angleDeg = 0f)
        {
            if (!_initialized) Initialize();
            string key = weaponKey;
            if (!_weaponTextures.TryGetValue(key, out Texture2D tex))
            {
                // Fallback capitalizado
                string cap = char.ToUpper(key[0]) + key.Substring(1).ToLower();
                if (!_weaponTextures.TryGetValue(cap, out tex))
                    tex = _weaponTextures["Liberator"];
            }

            Rectangle src = new(0, 0, tex.Width, -tex.Height);
            Rectangle dst = new(centerPos.X, centerPos.Y, tex.Width * scale, tex.Height * scale);
            Vector2 origin = new((tex.Width * scale) * 0.5f, (tex.Height * scale) * 0.5f);
            Raylib.DrawTexturePro(tex, src, dst, origin, angleDeg, Color.White);
        }

        /// <summary>
        /// Dibuja el escaparate táctico HD del arma en la Armería con blueprint holográfico,
        /// partículas de calor / energía, y animaciones de fogonazo o recarga.
        /// </summary>
        public static void DrawWeaponShowcase(string weaponKey, Vector2 center, float scale, float time, bool isEquipped)
        {
            if (!_initialized) Initialize();
            string key = weaponKey.ToLower();
            string lookup = key switch
            {
                "liberator" => "Liberator",
                "breaker" => "Breaker",
                "punisher" => "Punisher",
                "dominator" => "Dominator",
                "sickle" => "Sickle",
                "diligence" => "Diligence",
                "scorcher" => "Scorcher",
                "flamethrower" => "Flamethrower",
                "autocannon" => "Autocannon",
                "railgun" => "Railgun",
                "quasar" => "Quasar",
                _ => "Liberator"
            };

            if (!_weaponTextures.TryGetValue(lookup, out Texture2D tex))
                tex = _weaponTextures.Values.FirstOrDefault();

            // Fondo Blueprint táctico detrás del arma
            float boxW = 320f, boxH = 140f;
            Rectangle bgBox = new(center.X - boxW * 0.5f, center.Y - boxH * 0.5f, boxW, boxH);
            Raylib.DrawRectangleRec(bgBox, new Color(10, 16, 28, 220));
            Raylib.DrawRectangleLinesEx(bgBox, 1, isEquipped ? Constants.Gold : new Color(38, 56, 82, 255));

            // Cuadrícula holográfica de especificación militar
            for (int gx = (int)bgBox.X + 20; gx < (int)(bgBox.X + boxW); gx += 20)
                Raylib.DrawLine(gx, (int)bgBox.Y + 4, gx, (int)(bgBox.Y + boxH - 4), new Color(30, 48, 72, 80));
            for (int gy = (int)bgBox.Y + 20; gy < (int)(bgBox.Y + boxH); gy += 20)
                Raylib.DrawLine((int)bgBox.X + 4, gy, (int)(bgBox.X + boxW - 4), gy, new Color(30, 48, 72, 80));

            // Esquinas tácticas doradas/cyan
            int bx = (int)bgBox.X, by = (int)bgBox.Y, bw = (int)bgBox.Width, bh = (int)bgBox.Height;
            Color cornerCol = isEquipped ? Constants.Gold : Constants.Cyan;
            Raylib.DrawLine(bx, by, bx + 12, by, cornerCol);
            Raylib.DrawLine(bx, by, bx, by + 12, cornerCol);
            Raylib.DrawLine(bx + bw, by, bx + bw - 12, by, cornerCol);
            Raylib.DrawLine(bx + bw, by, bx + bw, by + 12, cornerCol);
            Raylib.DrawLine(bx, by + bh, bx + 12, by + bh, cornerCol);
            Raylib.DrawLine(bx, by + bh, bx, by + bh - 12, cornerCol);
            Raylib.DrawLine(bx + bw, by + bh, bx + bw - 12, by + bh, cornerCol);
            Raylib.DrawLine(bx + bw, by + bh, bx + bw, by + bh - 12, cornerCol);

            // Flotación suave y respiración del arma
            float hoverY = MathF.Sin(time * 2.2f) * 4f;
            Vector2 drawPos = center + new Vector2(0, hoverY);

            // Sombra holográfica debajo
            Raylib.DrawEllipse((int)center.X, (int)(center.Y + boxH * 0.38f), (int)(boxW * 0.35f), 10, new Color(0, 0, 0, 110));

            // Dibujo del arma pixel-art escalada
            if (tex.Id != 0)
            {
                Rectangle src = new(0, 0, tex.Width, -tex.Height);
                float drawW = tex.Width * scale;
                float drawH = tex.Height * scale;
                Rectangle dst = new(drawPos.X, drawPos.Y, drawW, drawH);
                Vector2 origin = new(drawW * 0.5f, drawH * 0.5f);
                Raylib.DrawTexturePro(tex, src, dst, origin, 0f, Color.White);

                // Efectos dinámicos según el tipo de arma:
                if (key == "flamethrower")
                {
                    // Llama piloto parpadeante en la boquilla
                    float flameSize = 4f + MathF.Sin(time * 30f) * 2f;
                    Vector2 nozzle = drawPos + new Vector2(drawW * 0.44f, -4f);
                    Raylib.DrawCircleV(nozzle, flameSize, new Color(255, 140, 20, 230));
                    Raylib.DrawCircleV(nozzle, flameSize * 0.5f, Color.White);
                }
                else if (key == "railgun")
                {
                    // Arcos eléctricos cyan entre los rieles
                    if (MathF.Sin(time * 15f) > 0.3f)
                    {
                        Vector2 railStart = drawPos + new Vector2(drawW * 0.15f, -6f);
                        Vector2 railEnd = drawPos + new Vector2(drawW * 0.48f, -6f);
                        Raylib.DrawLineEx(railStart, railEnd, 1.5f, TacticalCyan);
                        Raylib.DrawCircleV(railEnd, 3f, Color.White);
                    }
                }
                else if (key == "quasar" || key == "sickle")
                {
                    // Pulso térmico en los disipadores
                    float heatPulse = 0.5f + 0.5f * MathF.Sin(time * 4f);
                    Color heatCol = new(56, 189, 248, (int)(120 + heatPulse * 135));
                    Raylib.DrawCircleV(drawPos + new Vector2(drawW * 0.2f, 0), 6f * heatPulse, heatCol);
                }
            }

            // Etiqueta de arma equipada
            if (isEquipped)
            {
                string eqTxt = "[ EQUIPADA REGLAMENTARIA ]";
                Raylib.DrawText(eqTxt, (int)(center.X - Raylib.MeasureText(eqTxt, 10) * 0.5f), (int)(bgBox.Y + bgBox.Height - 16), 10, Constants.Gold);
            }
        }

        /// <summary>
        /// Dibuja el arma equipada en las manos del Helldiver con retroceso (Recoil),
        /// fogonazo animado de 3 frames (Muzzle Flash) y láser de apuntado.
        /// </summary>
        public static void DrawEquippedWeapon(
            Vector2 playerPos,
            float aimAngleRad,
            string weaponName,
            float recoilOffset,
            bool isFiring,
            float time)
        {
            if (!_initialized) Initialize();
            if (!_weaponTextures.TryGetValue(weaponName, out Texture2D tex))
                tex = _weaponTextures["Liberator"];

            float deg = aimAngleRad * (180f / MathF.PI);
            bool flipY = MathF.Cos(aimAngleRad) < 0;

            Vector2 forward = new(MathF.Cos(aimAngleRad), MathF.Sin(aimAngleRad));
            Vector2 weaponPos = playerPos + forward * (14f - recoilOffset * 6f);

            // Láser de mira táctica sutil
            Raylib.DrawLineEx(
                weaponPos + forward * 18f,
                weaponPos + forward * 260f,
                1.0f,
                new Color(255, 40, 40, 65)
            );

            Rectangle src = new(0, 0, tex.Width, flipY ? tex.Height : -tex.Height);
            Rectangle dst = new(weaponPos.X, weaponPos.Y, tex.Width * 1.6f, tex.Height * 1.6f);
            Vector2 origin = new(10f, dst.Height * 0.5f);

            Raylib.DrawTexturePro(tex, src, dst, origin, deg, Color.White);

            // Fogonazo Pixel-Art (Muzzle Flash) y eyección de casquillos si está disparando
            if (isFiring)
            {
                Vector2 muzzlePos = weaponPos + forward * 34f;
                bool isEnergy = weaponName.Contains("Scorcher", StringComparison.OrdinalIgnoreCase) 
                             || weaponName.Contains("Quasar", StringComparison.OrdinalIgnoreCase) 
                             || weaponName.Contains("Scythe", StringComparison.OrdinalIgnoreCase);
                Color flashColor = isEnergy ? TacticalCyan : SuperEarthYellow;

                float pulse = 7f + MathF.Sin(time * 60f) * 4f;
                Raylib.DrawPoly(muzzlePos, 4, pulse * 1.9f, deg + 45f, flashColor);
                Raylib.DrawCircleV(muzzlePos, pulse * 0.95f, Color.White);

                // Eyección de casquillos balísticos dorados
                if (!isEnergy)
                {
                    Vector2 right = new(-forward.Y, forward.X);
                    Vector2 casing = weaponPos - forward * 3f + right * (flipY ? -7f : 7f);
                    Raylib.DrawRectangle((int)casing.X, (int)casing.Y, 3, 2, new Color(245, 210, 60, 255));
                }
            }
        }

        // =========================================================================
        // 3. DISEÑO DE PLANETAS, BIOMAS Y CLIMA DINÁMICO (TILEMAPS + TORNADOS)
        // =========================================================================
        private static Texture2D GeneratePlanetBiomeTile(string planetName)
        {
            // Tile repetible de 128x128 píxeles en estilo Pixel-Art 16-bit
            RenderTexture2D rt = Raylib.LoadRenderTexture(128, 128);
            Raylib.BeginTextureMode(rt);

            Random rng = new(planetName.GetHashCode());

            Color baseCol, accent1, accent2, hazardCol;
            switch (planetName)
            {
                case "Malevelon Creek": // Jungla nocturna azulada y bioluminiscente
                    baseCol = new Color(14, 24, 34, 255);
                    accent1 = new Color(20, 38, 48, 255);
                    accent2 = new Color(10, 18, 26, 255);
                    hazardCol = new Color(0, 190, 220, 180); // Flora bioluminiscente
                    break;

                case "Hellmire": // Desierto volcánico con grietas de magma
                    baseCol = new Color(48, 24, 16, 255);
                    accent1 = new Color(68, 34, 20, 255);
                    accent2 = new Color(32, 14, 10, 255);
                    hazardCol = new Color(255, 110, 15, 255); // Grietas de lava
                    break;

                case "Crimsica": // Tundra carmesí alienígena
                    baseCol = new Color(46, 18, 26, 255);
                    accent1 = new Color(75, 22, 35, 255);
                    accent2 = new Color(28, 12, 18, 255);
                    hazardCol = new Color(220, 40, 75, 255); // Cristales rojos
                    break;

                case "Cyberstan": // Mundo fortaleza industrial de los Cyborgs/Autómatas
                    baseCol = new Color(24, 26, 32, 255);
                    accent1 = new Color(38, 42, 52, 255);
                    accent2 = new Color(18, 16, 22, 255);
                    hazardCol = new Color(255, 120, 20, 255); // Ríos de escoria y fundición
                    break;

                case "Calypso": // Ruinas místicas de los Iluminados
                    baseCol = new Color(26, 18, 42, 255);
                    accent1 = new Color(44, 26, 70, 255);
                    accent2 = new Color(16, 10, 28, 255);
                    hazardCol = new Color(217, 70, 239, 255); // Cristales psiónicos magenta
                    break;

                case "Squ'th": // Ciudadela ancestral Iluminada con mármol y energía cian
                    baseCol = new Color(18, 30, 44, 255);
                    accent1 = new Color(28, 48, 68, 255);
                    accent2 = new Color(12, 20, 30, 255);
                    hazardCol = new Color(34, 211, 238, 255); // Conductos de energía alienígena
                    break;

                default: // Vandalon IV - Glaciar Ártico
                    baseCol = new Color(175, 205, 230, 255);
                    accent1 = new Color(205, 230, 250, 255);
                    accent2 = new Color(135, 165, 195, 255);
                    hazardCol = new Color(90, 180, 255, 255); // Hielo especular
                    break;
            }

            Raylib.ClearBackground(baseCol);

            // Texturizado pixel-art de rocas, dunas y grietas (bloques de 2x2 y 4x4)
            for (int i = 0; i < 180; i++)
            {
                int px = (rng.Next(0, 64)) * 2;
                int py = (rng.Next(0, 64)) * 2;
                int w = rng.Next(1, 4) * 2;
                int h = rng.Next(1, 3) * 2;
                Raylib.DrawRectangle(px, py, w, h, (i % 2 == 0) ? accent1 : accent2);
            }

            // Detalles especiales del bioma (charcos, grietas de lava, flora o cristales)
            for (int i = 0; i < 8; i++)
            {
                int hx = rng.Next(4, 60) * 2;
                int hy = rng.Next(4, 60) * 2;
                Raylib.DrawRectangle(hx, hy, 6, 2, hazardCol);
                Raylib.DrawRectangle(hx + 2, hy - 2, 2, 6, hazardCol);
            }

            Raylib.EndTextureMode();
            Raylib.SetTextureFilter(rt.Texture, TextureFilter.Point);
            return rt.Texture;
        }

        /// <summary>
        /// Dibuja el suelo del planeta embaldosado y los efectos climáticos activos en pantalla.
        /// </summary>
        public static void DrawPlanetGroundAndWeather(
            string planetName,
            Camera2D camera,
            int screenW,
            int screenH,
            float time)
        {
            if (!_initialized) Initialize();
            if (!_biomeTiles.TryGetValue(planetName, out Texture2D tile))
                tile = _biomeTiles["Malevelon Creek"];

            int tileScaledSize = 256; // 128px escalado x2 manteniendo Pixel-Art puro
            Vector2 topLeft = Raylib.GetScreenToWorld2D(Vector2.Zero, camera);
            Vector2 bottomRight = Raylib.GetScreenToWorld2D(new Vector2(screenW, screenH), camera);

            int startX = ((int)MathF.Floor(topLeft.X / tileScaledSize) - 1) * tileScaledSize;
            int startY = ((int)MathF.Floor(topLeft.Y / tileScaledSize) - 1) * tileScaledSize;

            for (int x = startX; x < bottomRight.X + tileScaledSize; x += tileScaledSize)
            {
                for (int y = startY; y < bottomRight.Y + tileScaledSize; y += tileScaledSize)
                {
                    Raylib.DrawTexturePro(
                        tile,
                        new Rectangle(0, 0, tile.Width, tile.Height),
                        new Rectangle(x, y, tileScaledSize, tileScaledSize),
                        Vector2.Zero,
                        0f,
                        Color.White
                    );
                }
            }

            // Clima Dinámico en Espacio de Mundo (Lluvia, Ventisca, Tornados de Fuego)
            DrawBiomeWeatherEffects(planetName, topLeft, bottomRight, time);
        }

        private static void DrawBiomeWeatherEffects(string planetName, Vector2 min, Vector2 max, float time)
        {
            if (planetName == "Malevelon Creek")
            {
                // Lluvia diagonal azulada y relámpagos ocasionales
                for (int i = 0; i < 70; i++)
                {
                    float rx = min.X + ((i * 197f + time * 450f) % (max.X - min.X));
                    float ry = min.Y + ((i * 311f + time * 900f) % (max.Y - min.Y));
                    Raylib.DrawLineEx(
                        new Vector2(rx, ry),
                        new Vector2(rx - 8, ry + 22),
                        2f,
                        new Color(120, 200, 255, 110)
                    );
                }
            }
            else if (planetName == "Hellmire")
            {
                // Tornado de Fuego Autónomo que recorre el mapa
                Vector2 tornadoPos = new(
                    2250f + MathF.Cos(time * 0.25f) * 900f,
                    2250f + MathF.Sin(time * 0.18f) * 900f
                );

                for (int layer = 0; layer < 8; layer++)
                {
                    float angle = time * (6f + layer) + layer * 0.8f;
                    Vector2 offset = new(MathF.Cos(angle) * (layer * 6f), MathF.Sin(angle) * (layer * 6f) - layer * 10f);
                    float radius = 18f + layer * 7f;
                    Color fireCol = layer % 2 == 0
                        ? new Color(255, 90, 10, 170)
                        : new Color(255, 220, 40, 150);

                    Raylib.DrawPoly(tornadoPos + offset, 6, radius, time * 180f, fireCol);
                }
            }
            else if (planetName == "Vandalon IV")
            {
                // Copos de nieve pixelados en ventisca
                for (int i = 0; i < 80; i++)
                {
                    float sx = min.X + ((i * 251f + time * 260f) % (max.X - min.X));
                    float sy = min.Y + ((i * 173f + time * 320f + MathF.Sin(time * 3f + i) * 30f) % (max.Y - min.Y));
                    Raylib.DrawRectangle((int)sx, (int)sy, 4, 4, new Color(255, 255, 255, 180));
                }
            }
            else if (planetName == "Cyberstan")
            {
                // Lluvia de cenizas industriales y brasas de fundición ardientes
                for (int i = 0; i < 75; i++)
                {
                    float sx = min.X + ((i * 211f + time * 130f) % (max.X - min.X));
                    float sy = min.Y + ((i * 317f + time * 290f) % (max.Y - min.Y));
                    Color cCol = (i % 2 == 0) ? new Color(255, 120, 20, 210) : new Color(180, 80, 25, 160);
                    Raylib.DrawRectangle((int)sx, (int)sy, 3, 3, cCol);
                }
            }
            else if (planetName == "Calypso")
            {
                // Partículas psiónicas violetas levitando en ingravidez
                for (int i = 0; i < 60; i++)
                {
                    float sx = min.X + ((i * 183f + MathF.Sin(time * 2f + i) * 60f) % (max.X - min.X));
                    float rawY = (i * 241f - time * 90f) % (max.Y - min.Y);
                    if (rawY < 0) rawY += (max.Y - min.Y);
                    float sy = min.Y + rawY;
                    Color pCol = (i % 2 == 0) ? new Color(217, 70, 239, 180) : new Color(168, 85, 247, 150);
                    Raylib.DrawCircle((int)sx, (int)sy, 2.5f, pCol);
                    Raylib.DrawCircle((int)sx, (int)sy, 1f, Color.White);
                }
            }
            else if (planetName == "Squ'th")
            {
                // Motes dimensionales cian y distorsiones cuánticas
                for (int i = 0; i < 50; i++)
                {
                    float sx = min.X + ((i * 277f + time * 80f) % (max.X - min.X));
                    float sy = min.Y + ((i * 191f + MathF.Sin(time * 3f + i) * 45f) % (max.Y - min.Y));
                    Raylib.DrawRectangle((int)sx, (int)sy, 4, 2, new Color(34, 211, 238, 175));
                }
            }
        }

        // =========================================================================
        // 4. DISEÑO DE ESTRATAGEMAS, BEACONS, EXPLOSIÓN 500KG, LÁSER Y TORRETAS
        // =========================================================================

        /// <summary>
        /// Dibuja la baliza de estratagema activa con su haz vertical holográfico y anillos ascendentes.
        /// </summary>
        public static void DrawStratagemBeacon(Vector2 pos, bool isOffensiveRed, float timeRemaining, float time)
        {
            Color beamColor = isOffensiveRed ? AutomatonRed : TacticalCyan;

            // Sombra y base de la esfera en el suelo
            Raylib.DrawCircleV(pos, 10f + MathF.Sin(time * 20f) * 3f, new Color(beamColor.R, beamColor.G, beamColor.B, (byte)80));
            Raylib.DrawRectangle((int)pos.X - 4, (int)pos.Y - 4, 8, 8, OutlineBlack);
            Raylib.DrawRectangle((int)pos.X - 2, (int)pos.Y - 2, 4, 4, beamColor);

            // Rayo vertical hacia la atmósfera
            Raylib.DrawRectangleGradientH((int)pos.X - 8, (int)pos.Y - 900, 8, 900, Color.Blank, new Color(beamColor.R, beamColor.G, beamColor.B, (byte)160));
            Raylib.DrawRectangleGradientH((int)pos.X, (int)pos.Y - 900, 8, 900, new Color(beamColor.R, beamColor.G, beamColor.B, (byte)160), Color.Blank);
            Raylib.DrawRectangle((int)pos.X - 2, (int)pos.Y - 900, 4, 900, Color.White);

            // Anillos de pulso subiendo por el haz
            for (int i = 0; i < 4; i++)
            {
                float ringY = pos.Y - ((time * 220f + i * 120f) % 480f);
                Raylib.DrawRectangle((int)pos.X - 10, (int)ringY, 20, 3, beamColor);
            }

            // Temporizador Táctico
            Raylib.DrawText($"INBOUND: {timeRemaining:0.0}s", (int)pos.X + 14, (int)pos.Y - 24, 14, beamColor);
        }

        /// <summary>
        /// Dibuja la Explosión Nuclear Pixel-Art de la Bomba Eagle 500kg (progress de 0.0 a 1.0).
        /// </summary>
        public static void DrawEagle500kgExplosion(Vector2 pos, float progress)
        {
            if (progress <= 0f || progress >= 1f) return;

            float inv = 1f - progress;
            float maxRadius = 320f;
            float currentRadius = MathF.Sqrt(progress) * maxRadius;

            // 1. Cráter quemado en la base
            Raylib.DrawCircleV(pos, currentRadius * 0.55f, new Color(18, 16, 14, (int)(220 * inv)));

            // 2. Onda de choque ultrasónica exterior
            Raylib.DrawRing(
                pos,
                currentRadius * 0.92f,
                currentRadius,
                0,
                360,
                32,
                new Color(255, 240, 180, (int)(255 * inv))
            );

            // 3. Columna y Hongo Nuclear Pixelado por capas
            int blocks = 16;
            for (int i = 0; i < blocks; i++)
            {
                float angle = (i / (float)blocks) * MathF.PI * 2f + progress * 2f;
                float dist = currentRadius * 0.45f;
                Vector2 cloudPos = pos + new Vector2(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist * 0.7f - progress * 90f);

                // Sombra de humo negro exterior
                Raylib.DrawPoly(cloudPos, 6, 48f * inv, angle * 50f, new Color(35, 30, 30, (int)(220 * inv)));
                // Fuego naranja intermedio
                Raylib.DrawPoly(cloudPos, 6, 36f * inv, -angle * 40f, new Color(255, 100, 15, (int)(240 * inv)));
            }

            // 4. Domo central incandescente
            Vector2 capCenter = pos - new Vector2(0, progress * 110f);
            Raylib.DrawCircleV(capCenter, currentRadius * 0.45f * inv, new Color(255, 180, 30, (int)(250 * inv)));
            Raylib.DrawCircleV(capCenter, currentRadius * 0.28f * inv, new Color(255, 255, 255, (int)(255 * inv)));
        }

        /// <summary>
        /// Dibuja el Láser Orbital continuo con rastro de magma y chispas.
        /// </summary>
        public static void DrawOrbitalLaser(Vector2 targetPos, float time)
        {
            float jitter = MathF.Sin(time * 75f) * 3f;
            int bx = (int)(targetPos.X + jitter);
            int by = (int)targetPos.Y;

            // Suelo fundido bajo el láser
            Raylib.DrawCircleV(targetPos, 32f + MathF.Sin(time * 30f) * 6f, new Color(255, 80, 0, 140));
            Raylib.DrawCircleV(targetPos, 16f, Color.White);

            // Haz orbital multicapa
            Raylib.DrawRectangle(bx - 22, by - 1200, 44, 1200, new Color(255, 60, 0, 70));
            Raylib.DrawRectangle(bx - 12, by - 1200, 24, 1200, new Color(255, 190, 20, 190));
            Raylib.DrawRectangle(bx - 5, by - 1200, 10, 1200, Color.White);
        }

        /// <summary>
        /// Dibuja la insignia táctica oficial Pixel-Art HD para una Estratagema.
        /// Usado en el selector de Estratagemas, ranuras activas del puente y el HUD inferior de combate.
        /// </summary>
        public static void DrawStratagemBadge(string stratId, Vector2 center, float size, bool isEquipped, bool isSelected = false)
        {
            float hs = size / 2f;
            Rectangle box = new(center.X - hs, center.Y - hs, size, size);

            // Fondo del marco militar
            Raylib.DrawRectangleRec(box, new Color(10, 15, 24, 250));

            // Borde exterior táctico
            Color borderColor = isSelected 
                ? SuperEarthYellow 
                : (isEquipped ? Constants.LibertyGreen : new Color(50, 65, 88, 220));
            Raylib.DrawRectangleLinesEx(box, isSelected ? 2 : 1, borderColor);

            // Muescas de esquina
            float notch = Math.Clamp(size * 0.14f, 3f, 6f);
            Raylib.DrawLine((int)box.X, (int)box.Y, (int)(box.X + notch), (int)box.Y, SuperEarthYellow);
            Raylib.DrawLine((int)box.X, (int)box.Y, (int)box.X, (int)(box.Y + notch), SuperEarthYellow);
            Raylib.DrawLine((int)(box.X + box.Width), (int)(box.Y + box.Height), (int)(box.X + box.Width - notch), (int)(box.Y + box.Height), SuperEarthYellow);
            Raylib.DrawLine((int)(box.X + box.Width), (int)(box.Y + box.Height), (int)(box.X + box.Width), (int)(box.Y + box.Height - notch), SuperEarthYellow);

            float s = size / 40f; // Factor de escala para el arte interior

            // Iconografía Específica de la Estratagema
            string key = stratId.ToLowerInvariant();
            if (key.Contains("500kg"))
            {
                // Bomba Eagle de 500kg: Misil con ojiva roja y aletas estabilizadoras
                Vector2 tip = center + new Vector2(0, 12f * s);
                Vector2 noseL = center + new Vector2(-6f * s, 4f * s);
                Vector2 noseR = center + new Vector2(6f * s, 4f * s);
                Raylib.DrawTriangle(tip, noseL, noseR, AutomatonRed);

                Rectangle body = new(center.X - 5f * s, center.Y - 8f * s, 10f * s, 12f * s);
                Raylib.DrawRectangleRec(body, SuperEarthYellow);
                Raylib.DrawRectangleLinesEx(body, 1, OutlineBlack);

                // Aletas traseras
                Raylib.DrawLineEx(center + new Vector2(-5f * s, -8f * s), center + new Vector2(-11f * s, -12f * s), 2.5f * s, GunmetalDark);
                Raylib.DrawLineEx(center + new Vector2(5f * s, -8f * s), center + new Vector2(11f * s, -12f * s), 2.5f * s, GunmetalDark);
            }
            else if (key.Contains("gatling"))
            {
                // Torreta Gatling: Trípode con 3 cañones convergentes y tambor
                Raylib.DrawPoly(center + new Vector2(0, 4f * s), 3, 10f * s, 0f, GunmetalDark);
                Raylib.DrawCircleV(center, 6f * s, SuperEarthYellow);

                // Cañones frontales
                Raylib.DrawLineEx(center - new Vector2(4f * s, 0), center + new Vector2(-4f * s, -13f * s), 2.5f * s, GunmetalLight);
                Raylib.DrawLineEx(center, center + new Vector2(0, -14f * s), 2.5f * s, GunmetalLight);
                Raylib.DrawLineEx(center + new Vector2(4f * s, 0), center + new Vector2(4f * s, -13f * s), 2.5f * s, GunmetalLight);

                // Anillo de unión
                Raylib.DrawLineEx(center + new Vector2(-6f * s, -10f * s), center + new Vector2(6f * s, -10f * s), 2f * s, SuperEarthYellow);
            }
            else if (key.Contains("autocannon"))
            {
                // Torreta Cañón Automático: Cañones dobles pesados con freno en T
                Raylib.DrawPoly(center + new Vector2(0, 4f * s), 4, 9f * s, 45f, GunmetalDark);
                Raylib.DrawCircleV(center + new Vector2(0, 2f * s), 5f * s, SuperEarthYellow);

                // Cañones dobles
                Raylib.DrawLineEx(center - new Vector2(3f * s, 0), center + new Vector2(-3f * s, -15f * s), 3f * s, GunmetalLight);
                Raylib.DrawLineEx(center + new Vector2(3f * s, 0), center + new Vector2(3f * s, -15f * s), 3f * s, GunmetalLight);

                // Frenos de boca en T
                Raylib.DrawLineEx(center + new Vector2(-6f * s, -15f * s), center + new Vector2(0f, -15f * s), 2.5f * s, OutlineBlack);
                Raylib.DrawLineEx(center + new Vector2(0f, -15f * s), center + new Vector2(6f * s, -15f * s), 2.5f * s, OutlineBlack);
            }
            else if (key.Contains("laser"))
            {
                // Láser Orbital: Silueta de destructor y haz térmico vertical
                Raylib.DrawTriangle(
                    center + new Vector2(0, -14f * s),
                    center + new Vector2(-12f * s, -8f * s),
                    center + new Vector2(12f * s, -8f * s),
                    GunmetalDark
                );
                // Haz orbital concentrado
                Raylib.DrawLineEx(center + new Vector2(0, -8f * s), center + new Vector2(0, 12f * s), 3f * s, TacticalCyan);
                Raylib.DrawCircleV(center + new Vector2(0, 12f * s), 4f * s, SuperEarthYellow);
                Raylib.DrawCircleLines((int)center.X, (int)(center.Y + 12f * s), 7f * s, TacticalCyan);
            }
            else if (key.Contains("gas"))
            {
                // Ataque de Gas Orbital: Cilindro con vapores químicos
                Rectangle can = new(center.X - 5f * s, center.Y - 2f * s, 10f * s, 14f * s);
                Raylib.DrawRectangleRec(can, GunmetalDark);
                Raylib.DrawRectangleLinesEx(can, 1, TerminidBile);

                // Gotas / vapores tóxicos ascendiendo
                Raylib.DrawCircleV(center + new Vector2(-4f * s, -8f * s), 3f * s, TerminidBile);
                Raylib.DrawCircleV(center + new Vector2(4f * s, -10f * s), 3.5f * s, TerminidBile);
                Raylib.DrawCircleV(center + new Vector2(0, -5f * s), 2.5f * s, Color.White);
            }
            else if (key.Contains("precision"))
            {
                // Ataque Orbital de Precisión: Retícula balística y dardo penetrador
                Raylib.DrawCircleLines((int)center.X, (int)center.Y, 11f * s, AutomatonRed);
                Raylib.DrawLineEx(center - new Vector2(14f * s, 0), center + new Vector2(14f * s, 0), 1.5f * s, AutomatonRed);
                Raylib.DrawLineEx(center - new Vector2(0, 14f * s), center + new Vector2(0, 14f * s), 1.5f * s, AutomatonRed);
                Raylib.DrawCircleV(center, 3.5f * s, SuperEarthYellow);
            }
            else if (key.Contains("guard") || key.Contains("rover"))
            {
                // Dron Guard Dog Rover: Chasis hexagonal con lente láser
                Raylib.DrawPoly(center, 6, 9f * s, 30f, GunmetalDark);
                Raylib.DrawPoly(center, 6, 6f * s, 30f, SuperEarthYellow);
                Raylib.DrawCircleV(center, 3.5f * s, TacticalCyan);
                Raylib.DrawLineEx(center, center + new Vector2(8f * s, 8f * s), 2f * s, TacticalCyan);
            }
            else
            {
                // Emblema estándar Super Tierra
                Raylib.DrawPoly(center, 5, 8f * s, 0f, SuperEarthYellow);
                Raylib.DrawCircleLines((int)center.X, (int)center.Y, 11f * s, TacticalCyan);
            }
        }

        /// <summary>
        /// Dibuja la ilustración táctica militar de alta fidelidad pixel-art para la estratagema en el visor de la terminal.
        /// </summary>
        public static void DrawStratagemIllustration(string stratId, Rectangle rect, float time)
        {
            // Fondo de pantalla táctica militar con borde reforzado
            Raylib.DrawRectangleRec(rect, new Color(10, 16, 28, 250));
            Raylib.DrawRectangleLinesEx(rect, 2, Constants.Gold);

            // Esquinas reforzadas
            int rx = (int)rect.X, ry = (int)rect.Y, rw = (int)rect.Width, rh = (int)rect.Height;
            Raylib.DrawLine(rx, ry, rx + 16, ry, Constants.Gold);
            Raylib.DrawLine(rx, ry, rx, ry + 16, Constants.Gold);
            Raylib.DrawLine(rx + rw, ry, rx + rw - 16, ry, Constants.Gold);
            Raylib.DrawLine(rx + rw, ry, rx + rw, ry + 16, Constants.Gold);
            Raylib.DrawLine(rx, ry + rh, rx + 16, ry + rh, Constants.Gold);
            Raylib.DrawLine(rx, ry + rh, rx, ry + rh - 16, Constants.Gold);
            Raylib.DrawLine(rx + rw, ry + rh, rx + rw - 16, ry + rh, Constants.Gold);
            Raylib.DrawLine(rx + rw, ry + rh, rx + rw, ry + rh - 16, Constants.Gold);

            // Cuadrícula holográfica táctica
            for (int x = rx + 30; x < rx + rw; x += 30)
                Raylib.DrawLine(x, ry + 2, x, ry + rh - 2, new Color(30, 48, 72, 50));
            for (int y = ry + 25; y < ry + rh; y += 25)
                Raylib.DrawLine(rx + 2, y, rx + rw - 2, y, new Color(30, 48, 72, 50));

            Vector2 center = new(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f);
            string key = stratId.ToLowerInvariant();

            if (key.Contains("500kg"))
            {
                // --- BOMBA EAGLE 500KG: VUELO DE CAZA Y HONGO DE DEVASTACIÓN ---
                Raylib.DrawRectangle(rx + 10, ry + rh - 35, rw - 20, 30, new Color(18, 22, 32, 255));
                Vector2 jetPos = new(rect.X + 85, rect.Y + 52 + MathF.Sin(time * 3f) * 4f);
                DrawEagle1Jet(jetPos, time);

                Vector2 bombPos = new(center.X - 10, center.Y - 5 + MathF.Sin(time * 4f) * 6f);
                Raylib.DrawLineEx(bombPos, bombPos - new Vector2(22, 16), 7f, OutlineBlack);
                Raylib.DrawLineEx(bombPos, bombPos - new Vector2(20, 14), 5f, SuperEarthYellow);
                Raylib.DrawCircleV(bombPos, 4.5f, AutomatonRed);
                Raylib.DrawLineEx(bombPos - new Vector2(16, 12), bombPos - new Vector2(22, 20), 2.5f, GunmetalDark);
                Raylib.DrawLineEx(bombPos - new Vector2(16, 12), bombPos - new Vector2(20, 6), 2.5f, GunmetalDark);

                Vector2 blastPos = new(rect.X + rect.Width - 95, rect.Y + rect.Height - 35);
                float pulse = 1f + MathF.Sin(time * 5f) * 0.08f;
                Raylib.DrawCircleV(blastPos, 38f * pulse, new Color(255, 90, 10, 200));
                Raylib.DrawCircleV(blastPos - new Vector2(0, 24), 28f * pulse, new Color(255, 175, 20, 220));
                Raylib.DrawCircleV(blastPos - new Vector2(0, 48), 22f * pulse, new Color(255, 240, 120, 240));
                Raylib.DrawRectangle((int)blastPos.X - 14, (int)blastPos.Y - 45, 28, 45, new Color(40, 25, 20, 210));
                float wave = (time * 60f) % 50f;
                Raylib.DrawCircleLines((int)blastPos.X, (int)blastPos.Y, wave, new Color(255, 200, 50, (int)(220 * (1f - wave / 50f))));

                Raylib.DrawText("EAGLE-1 // CARGA: 500KG HE-NAPALM // ÁREA TOTAL", rx + 16, ry + rh - 22, 11, Constants.Gold);
            }
            else if (key.Contains("gatling"))
            {
                // --- TORRETA GATLING A/G-16: MINIGUN MECANIZADA 360° ---
                Vector2 sentryPos = center + new Vector2(0, 12);
                DrawSentryTurret(sentryPos, -0.4f, "gatling", 1.0f, true, time);

                Vector2 muzzle = sentryPos + new Vector2(MathF.Cos(-0.4f), MathF.Sin(-0.4f)) * 48f;
                for (int t = 0; t < 5; t++)
                {
                    float dist = 40f + t * 32f + (time * 280f) % 35f;
                    Vector2 bullet = muzzle + new Vector2(MathF.Cos(-0.4f), MathF.Sin(-0.4f)) * dist;
                    Raylib.DrawLineEx(bullet, bullet + new Vector2(MathF.Cos(-0.4f), MathF.Sin(-0.4f)) * 14f, 2.5f, SuperEarthYellow);
                }
                Raylib.DrawLineEx(muzzle, muzzle + new Vector2(MathF.Cos(-0.4f), MathF.Sin(-0.4f)) * 200f, 1f, new Color(255, 30, 30, 90));

                Raylib.DrawText("TORRETA CENTINELA A/G-16 // 6 CAÑONES ROTATIVOS // 1,200 RPM", rx + 16, ry + rh - 22, 11, Constants.Cyan);
            }
            else if (key.Contains("autocannon"))
            {
                // --- TORRETA AUTOCAÑÓN A/AC-8: ARTILLERÍA PESADA ANTI-VEHÍCULO ---
                Vector2 sentryPos = center + new Vector2(0, 12);
                DrawSentryTurret(sentryPos, -0.3f, "autocannon", 1.0f, true, time);

                Vector2 muzzle = sentryPos + new Vector2(MathF.Cos(-0.3f), MathF.Sin(-0.3f)) * 52f;
                for (int b = 0; b < 2; b++)
                {
                    float dist = 50f + b * 75f + (time * 180f) % 60f;
                    Vector2 shell = muzzle + new Vector2(MathF.Cos(-0.3f), MathF.Sin(-0.3f)) * dist;
                    Raylib.DrawCircleV(shell, 3.5f, SuperEarthYellow);
                    Raylib.DrawLineEx(shell - new Vector2(16, 5), shell, 3f, new Color(255, 120, 20, 200));
                }

                Raylib.DrawText("TORRETA AUTOCAÑÓN A/AC-8 // CALIBRE 20MM HE // BLINDADOS", rx + 16, ry + rh - 22, 11, Constants.Gold);
            }
            else if (key.Contains("laser"))
            {
                // --- LÁSER ORBITAL: DESTRUCTOR DISPARANDO HAZ TÉRMICO CONTINUO ---
                Vector2 shipPos = new(center.X, rect.Y + 28);
                Raylib.DrawTriangle(shipPos, shipPos + new Vector2(-60, -18), shipPos + new Vector2(60, -18), GunmetalDark);
                Raylib.DrawRectangle((int)shipPos.X - 18, (int)shipPos.Y - 24, 36, 12, GunmetalLight);

                float beamWidth = 8f + MathF.Sin(time * 35f) * 3f;
                Vector2 beamStart = shipPos;
                Vector2 beamEnd = new(center.X, rect.Y + rect.Height - 30);
                Raylib.DrawLineEx(beamStart, beamEnd, beamWidth + 12f, new Color(0, 220, 255, 80));
                Raylib.DrawLineEx(beamStart, beamEnd, beamWidth + 5f, TacticalCyan);
                Raylib.DrawLineEx(beamStart, beamEnd, beamWidth * 0.5f, Color.White);

                Raylib.DrawCircleV(beamEnd, 24f + MathF.Sin(time * 25f) * 6f, new Color(255, 110, 20, 220));
                Raylib.DrawCircleV(beamEnd, 12f, Color.White);
                float shock = (time * 40f) % 30f;
                Raylib.DrawCircleLines((int)beamEnd.X, (int)beamEnd.Y, shock, TacticalCyan);

                Raylib.DrawText("DISPARO DE HAZ ORBITAL TÉRMICO // ENERGÍA DIRIGIDA // 20s", rx + 16, ry + rh - 22, 11, TacticalCyan);
            }
            else if (key.Contains("gas"))
            {
                // --- ATAQUE DE GAS ORBITAL: NUBE CORROSIVA CAÚSTICA ---
                Vector2 bombPos = new(center.X, rect.Y + rect.Height - 45);
                Raylib.DrawRectangle((int)bombPos.X - 8, (int)bombPos.Y - 14, 16, 22, GunmetalDark);
                Raylib.DrawRectangleLines((int)bombPos.X - 8, (int)bombPos.Y - 14, 16, 22, TerminidBile);

                for (int g = 0; g < 14; g++)
                {
                    float angle = g * 0.45f + time * 1.5f;
                    float r = 25f + (g % 4) * 16f;
                    Vector2 gPos = bombPos + new Vector2(MathF.Cos(angle) * r * 1.4f, MathF.Sin(angle) * r * 0.7f - 18f);
                    float size = 18f + MathF.Sin(time * 3f + g) * 5f;
                    Color gasCol = (g % 2 == 0) ? new Color(140, 230, 20, 120) : new Color(190, 255, 40, 95);
                    Raylib.DrawCircleV(gPos, size, gasCol);
                }

                Raylib.DrawCircleV(bombPos - new Vector2(0, 36), 14f, new Color(20, 25, 10, 220));
                Raylib.DrawText("☣️", (int)bombPos.X - 10, (int)bombPos.Y - 45, 16, TerminidBile);

                Raylib.DrawText("CORROSIVO QUÍMICO ORBITAL // DAÑO BIOLÓGICO Y MECÁNICO ÁREA", rx + 16, ry + rh - 22, 11, TerminidBile);
            }
            else if (key.Contains("precision"))
            {
                // --- GOLPE ORBITAL DE PRECISIÓN: PENETRADOR CINÉTICO A MACH 10 ---
                Raylib.DrawCircleLines((int)center.X, (int)center.Y, 44f, AutomatonRed);
                Raylib.DrawCircleLines((int)center.X, (int)center.Y, 22f, Constants.Gold);
                Raylib.DrawLineEx(center - new Vector2(55, 0), center + new Vector2(55, 0), 1.5f, AutomatonRed);
                Raylib.DrawLineEx(center - new Vector2(0, 50), center + new Vector2(0, 50), 1.5f, AutomatonRed);

                Vector2 projPos = center - new Vector2(0, 10 + MathF.Sin(time * 8f) * 12f);
                Raylib.DrawLineEx(projPos - new Vector2(0, 48), projPos, 7f, OutlineBlack);
                Raylib.DrawLineEx(projPos - new Vector2(0, 44), projPos, 4f, SuperEarthYellow);
                Raylib.DrawCircleV(projPos, 3.5f, Color.White);
                Raylib.DrawTriangle(projPos, projPos - new Vector2(-18, 30), projPos - new Vector2(18, 30), new Color(255, 200, 50, 70));

                Raylib.DrawText("PROYECTIL CINÉTICO DE TUNGSTENO // DISPARO DIRECTO // INSTANTÁNEO", rx + 16, ry + rh - 22, 11, AutomatonRed);
            }
            else if (key.Contains("guard") || key.Contains("rover"))
            {
                // --- DRON COMPAÑERO GUARD DOG ROVER: VUELO Y LÁSER QUIRÚRGICO ---
                Vector2 dronePos = center + new Vector2(0, -10 + MathF.Sin(time * 3.5f) * 7f);

                Raylib.DrawPoly(dronePos, 6, 26f, 30f, GunmetalDark);
                Raylib.DrawPolyLines(dronePos, 6, 26f, 30f, OutlineBlack);
                Raylib.DrawPoly(dronePos, 6, 18f, 30f, SuperEarthYellow);
                Raylib.DrawCircleV(dronePos, 8f, TacticalCyan);
                Raylib.DrawCircleV(dronePos, 4f, Color.White);

                Vector2 jetL = dronePos + new Vector2(-16, 14);
                Vector2 jetR = dronePos + new Vector2(16, 14);
                Raylib.DrawLineEx(jetL, jetL + new Vector2(0, 14f + MathF.Sin(time * 40f) * 5f), 3f, TacticalCyan);
                Raylib.DrawLineEx(jetR, jetR + new Vector2(0, 14f + MathF.Cos(time * 40f) * 5f), 3f, TacticalCyan);

                Vector2 laserTarget = new(rect.X + rect.Width - 60, rect.Y + rect.Height - 35);
                Raylib.DrawLineEx(dronePos + new Vector2(6, 6), laserTarget, 2.5f, TacticalCyan);
                Raylib.DrawCircleV(laserTarget, 5f + MathF.Sin(time * 20f) * 2f, Color.White);

                Raylib.DrawText("AX/LAS-5 'GUARD DOG' ROVER // DRON DE COMBATE AUTÓNOMO 360°", rx + 16, ry + rh - 22, 11, TacticalCyan);
            }
            else
            {
                Raylib.DrawPoly(center, 5, 28f, 0f, SuperEarthYellow);
                Raylib.DrawCircleLines((int)center.X, (int)center.Y, 40f, TacticalCyan);
                Raylib.DrawText("PROTOCOLO DE TRANSMISIÓN DE SUPER TIERRA", rx + 16, ry + rh - 22, 11, Constants.Gold);
            }
        }

        /// <summary>
        /// Dibuja la nave Super Destroyer de Super Tierra ("SES Heraldo de la Democracia") en el Hangar
        /// con sus módulos visuales conectados a las mejoras:
        /// - Blindaje de placas de grafeno en el casco (armorLevel).
        /// - Turbinas de salto hiperespacial FTL con escape pulsante (speedLevel).
        /// - Baterías de artillería pesada orbital en la proa (damageLevel).
        /// - Cúpula del proyector de escudos reflectores SH-20 (shieldLevel).
        /// </summary>
        /// <summary>
        /// Dibuja la nave Super Destroyer de Super Tierra ("SES Heraldo de la Democracia") en el Hangar
        /// con su imponente silueta longitudinal de navío de guerra inspirada en el diseño táctico militar:
        /// - Morro alargado con antena de sensores y torretas dobles de artillería pesada en barbetas dorsales elevadas.
        /// - Torre de mando del puente con miradores ámbar y mástil de comunicaciones.
        /// - Sección media con maquinaria expuesta, conductos, tuberías y doble chevron amarillo // de Super Tierra.
        /// - Compuertas ventrales del hangar de cazas Eagle/Pelican y tubos de lanzamiento de Hellpods.
        /// - 4 campanas de propulsión FTL de plasma cian gemelas en la popa con conos de escape energéticos.
        /// - Respuesta visual en tiempo real a las mejoras de la nave (blindaje, velocidad, artillería, escudos).
        /// </summary>
        public static void DrawSuperDestroyer(
            Vector2 center,
            float scale,
            float time,
            int armorLevel,
            int speedLevel,
            int damageLevel,
            int shieldLevel)
        {
            float shipLen = 380f * scale;
            float shipH = 78f * scale;
            float halfH = shipH * 0.5f;

            Vector2 bow = center + new Vector2(shipLen * 0.50f, 0);
            Vector2 stern = center - new Vector2(shipLen * 0.50f, 0);

            // 1. Resplandor del Escudo Deflector SH-20 (Domo esférico con malla hexagonal)
            if (shieldLevel > 0)
            {
                float shieldRadiusX = (shipLen * 0.55f) + MathF.Sin(time * 3f) * 6f;
                float shieldRadiusY = (shipH * 1.55f) + MathF.Sin(time * 3f) * 4f;
                byte shieldAlpha = (byte)Math.Clamp(55 + shieldLevel * 24 + MathF.Sin(time * 4f) * 25, 45, 190);
                Raylib.DrawEllipseLines((int)center.X, (int)center.Y, shieldRadiusX, shieldRadiusY, new Color(34, 211, 238, (int)shieldAlpha));
                Raylib.DrawEllipseLines((int)center.X, (int)center.Y, shieldRadiusX - 4f, shieldRadiusY - 3f, new Color(34, 211, 238, (int)(shieldAlpha / 2)));
                Raylib.DrawEllipse((int)center.X, (int)center.Y, shieldRadiusX, shieldRadiusY, new Color(34, 211, 238, (int)(shieldAlpha / 6)));
            }

            // 2. Popa y Bloque Motor: 4 Campanas de Propulsión FTL de Plasma Cian (2 superiores, 2 inferiores)
            float flameLen = (36f + speedLevel * 14f) * scale + MathF.Sin(time * 50f) * 7f;
            float[] thrusterYOffsets = { -halfH * 0.65f, -halfH * 0.22f, halfH * 0.22f, halfH * 0.65f };

            // Bloque estructural de toberas de popa
            Rectangle sternBulkhead = new(stern.X - 10 * scale, center.Y - halfH * 0.78f, 22 * scale, shipH * 0.78f);
            Raylib.DrawRectangleRec(sternBulkhead, GunmetalDark);
            Raylib.DrawRectangleLinesEx(sternBulkhead, 1.5f * scale, OutlineBlack);

            for (int t = 0; t < thrusterYOffsets.Length; t++)
            {
                Vector2 nozzlePos = new(stern.X + 2 * scale, center.Y + thrusterYOffsets[t]);
                float nozzleRadius = 7.5f * scale;

                // Llamas de plasma FTL de alta energía
                Raylib.DrawTriangle(
                    nozzlePos,
                    nozzlePos - new Vector2(flameLen, -nozzleRadius * 1.25f),
                    nozzlePos - new Vector2(flameLen, nozzleRadius * 1.25f),
                    new Color(0, 190, 255, 185)
                );
                Raylib.DrawTriangle(
                    nozzlePos,
                    nozzlePos - new Vector2(flameLen * 0.65f, -nozzleRadius * 0.6f),
                    nozzlePos - new Vector2(flameLen * 0.65f, nozzleRadius * 0.6f),
                    Color.White
                );

                // Disco de diamante de choque de plasma
                Raylib.DrawCircleV(nozzlePos - new Vector2(flameLen * 0.35f, 0), 3.5f * scale, Color.White);

                // Campana metálica exterior del propulsor
                Raylib.DrawCircleV(nozzlePos, nozzleRadius, OutlineBlack);
                Raylib.DrawCircleV(nozzlePos, nozzleRadius - 1.8f * scale, GunmetalLight);
                Raylib.DrawCircleV(nozzlePos, nozzleRadius - 3.2f * scale, TacticalCyan);
                Raylib.DrawCircleV(nozzlePos, nozzleRadius - 5.0f * scale, Color.White);
            }

            // 3. Casco Longitudinal Naval Principal
            // Silueta de batalla: Proa afilada, sección media con blindaje angular, cubierta dorsal y panza ventral
            Color mainHullDark = new(22, 28, 38, 255);
            Color mainHullMid = new(34, 44, 58, 255);
            Color armorPlateColor = new(46, 58, 76, 255);

            // Casco base compuesto por polígonos navales
            Vector2 prowTip = bow + new Vector2(18 * scale, 0); // Espolón de proa
            Vector2 prowTop = bow + new Vector2(-shipLen * 0.18f, -halfH * 0.52f);
            Vector2 prowBot = bow + new Vector2(-shipLen * 0.18f, halfH * 0.52f);

            Vector2 midTop = center + new Vector2(shipLen * 0.05f, -halfH * 0.72f);
            Vector2 midBot = center + new Vector2(shipLen * 0.05f, halfH * 0.72f);

            Vector2 aftTop = stern + new Vector2(shipLen * 0.20f, -halfH * 0.85f);
            Vector2 aftBot = stern + new Vector2(shipLen * 0.20f, halfH * 0.85f);

            // Capa inferior del casco (Sombra y volumen)
            Raylib.DrawTriangle(prowTip, prowTop, prowBot, OutlineBlack);
            Raylib.DrawTriangle(prowTop, midTop, midBot, OutlineBlack);
            Raylib.DrawTriangle(prowTop, midBot, prowBot, OutlineBlack);
            Raylib.DrawTriangle(midTop, aftTop, aftBot, OutlineBlack);
            Raylib.DrawTriangle(midTop, aftBot, midBot, OutlineBlack);

            // Relleno del casco militar de aleación
            Raylib.DrawTriangle(prowTip - new Vector2(2, 0), prowTop + new Vector2(0, 2), prowBot - new Vector2(0, 2), mainHullDark);
            Raylib.DrawTriangle(prowTop + new Vector2(0, 2), midTop + new Vector2(0, 2), midBot - new Vector2(0, 2), mainHullMid);
            Raylib.DrawTriangle(prowTop + new Vector2(0, 2), midBot - new Vector2(0, 2), prowBot - new Vector2(0, 2), mainHullDark);
            Raylib.DrawTriangle(midTop + new Vector2(0, 2), aftTop + new Vector2(0, 2), aftBot - new Vector2(0, 2), mainHullMid);
            Raylib.DrawTriangle(midTop + new Vector2(0, 2), aftBot - new Vector2(0, 2), midBot - new Vector2(0, 2), mainHullDark);

            // 4. Maquinaria Expuesta y Greebles en la Sección Media (Tubos de refrigerante, cables, rejillas)
            Rectangle greebleRec = new(center.X - 45 * scale, center.Y - 14 * scale, 85 * scale, 28 * scale);
            Raylib.DrawRectangleRec(greebleRec, new Color(14, 18, 26, 255));
            Raylib.DrawRectangleLinesEx(greebleRec, 1f * scale, OutlineBlack);

            // Tuberías de energía cian pulsante
            float tubePulse = 0.6f + 0.4f * MathF.Sin(time * 5f);
            Raylib.DrawLineEx(new Vector2(greebleRec.X + 4, center.Y - 6 * scale), new Vector2(greebleRec.X + greebleRec.Width - 4, center.Y - 6 * scale), 2f * scale, new Color(34, 211, 238, (int)(180 * tubePulse)));
            Raylib.DrawLineEx(new Vector2(greebleRec.X + 4, center.Y + 6 * scale), new Vector2(greebleRec.X + greebleRec.Width - 4, center.Y + 6 * scale), 2f * scale, new Color(34, 211, 238, (int)(180 * tubePulse)));

            // Rejillas verticales de disipación térmica
            for (float gx = greebleRec.X + 10 * scale; gx < greebleRec.X + greebleRec.Width - 8 * scale; gx += 7 * scale)
            {
                Raylib.DrawLineEx(new Vector2(gx, greebleRec.Y + 3), new Vector2(gx, greebleRec.Y + greebleRec.Height - 3), 1.5f * scale, GunmetalLight);
            }

            // 5. Los Dos Icónicos Chevrons Amarillos de Super Tierra en el Fuselaje (//)
            float chevX = center.X + 55 * scale;
            float chevY = center.Y;
            float chevW = 9 * scale;
            float chevH = 34 * scale;

            for (int ch = 0; ch < 2; ch++)
            {
                float cxOff = chevX + ch * 16 * scale;
                Vector2 c1 = new(cxOff + 10 * scale, chevY - chevH * 0.5f);
                Vector2 c2 = new(cxOff + 10 * scale + chevW, chevY - chevH * 0.5f);
                Vector2 c3 = new(cxOff - 6 * scale + chevW, chevY + chevH * 0.5f);
                Vector2 c4 = new(cxOff - 6 * scale, chevY + chevH * 0.5f);

                Raylib.DrawTriangle(c1, c4, c2, SuperEarthYellow);
                Raylib.DrawTriangle(c2, c4, c3, SuperEarthYellow);
                Raylib.DrawLineEx(c1, c2, 1f, OutlineBlack);
                Raylib.DrawLineEx(c3, c4, 1f, OutlineBlack);
            }

            // Franjas amarillas longitudinales de advertencia militar
            Raylib.DrawLineEx(prowTop + new Vector2(10 * scale, 5 * scale), prowTip - new Vector2(18 * scale, 0), 3f * scale, SuperEarthYellow);
            Raylib.DrawLineEx(prowBot + new Vector2(10 * scale, -5 * scale), prowTip - new Vector2(18 * scale, 0), 3f * scale, SuperEarthYellow);

            // 6. Blindaje de Placas de Grafeno (Upgrade ArmorLevel)
            if (armorLevel > 0)
            {
                int plates = Math.Min(armorLevel, 5);
                for (int al = 0; al < plates; al++)
                {
                    float offset = al * 14 * scale;
                    Rectangle plateRec = new(center.X - 110 * scale + offset * 1.5f, center.Y - halfH * 0.55f, 18 * scale, shipH * 0.55f);
                    Raylib.DrawRectangleRec(plateRec, armorPlateColor);
                    Raylib.DrawRectangleLinesEx(plateRec, 1f * scale, OutlineBlack);
                    // Remaches metálicos en las placas
                    Raylib.DrawCircle((int)plateRec.X + 3, (int)plateRec.Y + 3, 1.2f * scale, SuperEarthYellow);
                    Raylib.DrawCircle((int)plateRec.X + 3, (int)(plateRec.Y + plateRec.Height - 3), 1.2f * scale, SuperEarthYellow);
                }
            }

            // 7. Torre Elevada del Puente de Mando (Command Bridge Superstructure)
            Rectangle bridgeTower = new(center.X - 115 * scale, center.Y - halfH * 0.98f, 48 * scale, 28 * scale);
            Raylib.DrawRectangleRec(bridgeTower, new Color(38, 48, 64, 255));
            Raylib.DrawRectangleLinesEx(bridgeTower, 1.5f * scale, OutlineBlack);

            // Nivel superior de observación con ventanales ámbar iluminados
            Rectangle bridgeWindows = new(bridgeTower.X + 16 * scale, bridgeTower.Y + 6 * scale, 28 * scale, 6 * scale);
            Raylib.DrawRectangleRec(bridgeWindows, new Color(255, 215, 0, 240));
            Raylib.DrawRectangleLinesEx(bridgeWindows, 1f, OutlineBlack);

            // Mástil de comunicaciones y radar dorsal
            Vector2 mastBase = new(bridgeTower.X + 10 * scale, bridgeTower.Y);
            Vector2 mastTip = mastBase - new Vector2(0, 16 * scale);
            Raylib.DrawLineEx(mastBase, mastTip, 2.5f * scale, GunmetalLight);
            Raylib.DrawLineEx(mastTip - new Vector2(5 * scale, 0), mastTip + new Vector2(5 * scale, 0), 1.5f * scale, SuperEarthYellow);

            // Baliza estroboscópica blanca en lo alto del mástil
            if (MathF.Sin(time * 8f) > 0.35f)
            {
                Raylib.DrawCircleV(mastTip, 3.5f * scale, Color.White);
                Raylib.DrawCircleV(mastTip, 7f * scale, new Color(255, 255, 255, 120));
            }

            // 8. Baterías de Artillería Pesada Orbital en Barbetas Dorsales Elevadas (Upgrade DamageLevel)
            int turretCount = 2 + Math.Min(damageLevel, 2);
            for (int tc = 0; tc < turretCount; tc++)
            {
                Vector2 barbetteBase = bow - new Vector2((48 + tc * 38) * scale, halfH * 0.42f);
                float gunLen = (22f + damageLevel * 3f) * scale;
                float gunElev = -0.15f - (tc * 0.05f); // Ligera elevación hacia adelante y arriba

                // Barbeta cilíndrica blindada
                Raylib.DrawRectangle((int)barbetteBase.X - 9, (int)barbetteBase.Y - 5, (int)(18 * scale), (int)(10 * scale), OutlineBlack);
                Raylib.DrawRectangle((int)barbetteBase.X - 7, (int)barbetteBase.Y - 4, (int)(14 * scale), (int)(8 * scale), GunmetalLight);

                // Cúpula giratoria de la torreta
                Raylib.DrawCircleV(barbetteBase - new Vector2(0, 4 * scale), 7.5f * scale, GunmetalDark);
                Raylib.DrawCircleLines((int)barbetteBase.X, (int)(barbetteBase.Y - 4 * scale), 7.5f * scale, OutlineBlack);
                Raylib.DrawCircleV(barbetteBase - new Vector2(0, 4 * scale), 4.5f * scale, SuperEarthYellow);

                // Doble cañón orbital naval de tungsteno
                Vector2 muzzle1 = barbetteBase + new Vector2(MathF.Cos(gunElev), MathF.Sin(gunElev)) * gunLen - new Vector2(0, 2.5f * scale);
                Vector2 muzzle2 = barbetteBase + new Vector2(MathF.Cos(gunElev), MathF.Sin(gunElev)) * gunLen + new Vector2(0, 2.5f * scale);
                Raylib.DrawLineEx(barbetteBase - new Vector2(0, 2.5f * scale), muzzle1, 3.5f * scale, OutlineBlack);
                Raylib.DrawLineEx(barbetteBase - new Vector2(0, 2.5f * scale), muzzle1, 2.0f * scale, GunmetalLight);
                Raylib.DrawLineEx(barbetteBase + new Vector2(0, 2.5f * scale), muzzle2, 3.5f * scale, OutlineBlack);
                Raylib.DrawLineEx(barbetteBase + new Vector2(0, 2.5f * scale), muzzle2, 2.0f * scale, GunmetalLight);

                // Frenos de boca dobles
                Raylib.DrawRectangle((int)muzzle1.X - 2, (int)muzzle1.Y - 3, 4, 6, OutlineBlack);
                Raylib.DrawRectangle((int)muzzle2.X - 2, (int)muzzle2.Y - 3, 4, 6, OutlineBlack);
            }

            // 9. Bahía Ventral del Hangar de Cazas y Tubos de Hellpods
            Rectangle ventralHangar = new(center.X - 25 * scale, center.Y + halfH * 0.44f, 75 * scale, 16 * scale);
            Raylib.DrawRectangleRec(ventralHangar, new Color(14, 18, 26, 255));
            Raylib.DrawRectangleLinesEx(ventralHangar, 1.2f * scale, SuperEarthYellow);

            // Compuertas de eyección de Hellpods con indicadores LED cian
            for (int hp = 0; hp < 5; hp++)
            {
                float tubeX = ventralHangar.X + 10 * scale + hp * 13 * scale;
                Raylib.DrawRectangle((int)tubeX, (int)ventralHangar.Y + 3, 7, 10, GunmetalDark);
                Raylib.DrawRectangleLines((int)tubeX, (int)ventralHangar.Y + 3, 7, 10, OutlineBlack);
                Raylib.DrawCircle((int)tubeX + 3, (int)ventralHangar.Y + 8, 1.8f * scale, TacticalCyan);
            }

            // 10. Aguja de Sensores de Proa Frontal (Needle Prow Sensor Boom)
            Raylib.DrawLineEx(prowTip, prowTip + new Vector2(30 * scale, 0), 2.5f * scale, GunmetalLight);
            Raylib.DrawCircleV(prowTip + new Vector2(30 * scale, 0), 2.5f * scale, TacticalCyan);

            // 11. Luces de Navegación Reglamentarias Babor / Estribor
            Raylib.DrawCircleV(midTop, 3.5f * scale, AutomatonRed); // Rojo babor
            Raylib.DrawCircleV(midBot, 3.5f * scale, TerminidBile); // Verde estribor

            // 12. Rótulo Militar Oficial de Super Tierra
            string shipName = "SES HERALDO DE LA DEMOCRACIA";
            Raylib.DrawText(shipName, (int)(center.X - Raylib.MeasureText(shipName, 13) * 0.5f), (int)(center.Y + halfH + 20 * scale), 13, Constants.Gold);
            string shipSub = "DESTRUCTOR PESADO DE CLASE LIBERTAD // REGISTRO SES-HD01 // FLOTA DE SUPER TIERRA";
            Raylib.DrawText(shipSub, (int)(center.X - Raylib.MeasureText(shipSub, 9) * 0.5f), (int)(center.Y + halfH + 37 * scale), 9, Constants.Cyan);
        }

        /// <summary>
        /// Dibuja la secuencia cinemática de entrega de estratagema en combate (Vuelo de Eagle-1, Hellpod hipersónica o Baliza).
        /// </summary>
        public static void DrawInboundStratagemDelivery(
            Vector2 targetPos,
            string stratType,
            float timer,
            float maxTimer,
            float time)
        {
            bool isTurret = stratType.Contains("sentry") || stratType.Contains("gatling") || stratType.Contains("autocannon");
            bool isEagle = stratType.Contains("500kg") || stratType.Contains("precision");

            // 1. Baliza táctica holográfica en el suelo
            bool isRed = !isTurret;
            DrawStratagemBeacon(targetPos, isRed, timer, time);

            // 2. Despliegue en Hellpod para Torretas / Suministros (últimos 1.2 segundos)
            if (isTurret && timer < 1.2f)
            {
                float podProg = 1f - Math.Clamp(timer / 1.2f, 0f, 1f);
                float alt = MathF.Pow(1f - podProg, 2.5f) * 650f;
                Vector2 podPos = targetPos - new Vector2(alt * 0.32f, alt);

                // Estela de fuego y humo de reentrada atmosférica
                for (int sp = 0; sp < 4; sp++)
                {
                    float flameLen = (sp + 1) * 20f;
                    Vector2 flameOff = new Vector2(flameLen * 0.32f, flameLen);
                    Raylib.DrawLineEx(podPos, podPos - flameOff, 6f - sp, (sp % 2 == 0) ? new Color(255, 120, 20, 220) : SuperEarthYellow);
                }

                // Sombra de aproximación en el suelo
                float sAlpha = Math.Clamp(podProg * 220f, 0, 220);
                Raylib.DrawEllipse((int)targetPos.X, (int)targetPos.Y, 26f * podProg, 14f * podProg, new Color(0, 0, 0, (int)sAlpha));

                // Cápsula Hellpod descendiendo
                DrawHellpod(podPos, 1.2f, 0f);

                // Ondas de polvo al tocar suelo
                if (podProg > 0.85f)
                {
                    float dustR = (podProg - 0.85f) * 160f;
                    Raylib.DrawCircleLines((int)targetPos.X, (int)targetPos.Y, dustR, new Color(240, 220, 180, (int)(180 * (1f - podProg) / 0.15f)));
                }
            }

            // 3. Pasada supersónica de Eagle-1 para Bombas / Ataques Aéreos (últimos 0.85 segundos)
            if (isEagle && timer < 0.85f)
            {
                float jetProg = 1f - Math.Clamp(timer / 0.85f, 0f, 1f);
                Vector2 jetPos = targetPos + new Vector2((jetProg - 0.5f) * 2200f, (jetProg - 0.5f) * 800f - 180f);

                DrawEagle1Jet(jetPos, time);

                // Si ya soltó la bomba (entre 0.35 y 0.95)
                if (jetProg > 0.35f && jetProg < 0.95f)
                {
                    float bombProg = (jetProg - 0.35f) / 0.6f;
                    float bAlt = MathF.Pow(1f - bombProg, 2f) * 320f;
                    Vector2 bombPos = targetPos - new Vector2(bAlt * 0.4f, bAlt);

                    Raylib.DrawLineEx(bombPos, bombPos - new Vector2(18f, 28f), 7f, OutlineBlack);
                    Raylib.DrawLineEx(bombPos, bombPos - new Vector2(16f, 26f), 5f, SuperEarthYellow);
                    Raylib.DrawCircleV(bombPos, 4f, AutomatonRed);
                }
            }
        }

        /// <summary>
        /// Dibuja la cápsula orbital Hellpod de Super Tierra.
        /// Si time > 0 o está aterrizada, despliega los 4 pétalos estabilizadores abiertos con chispas y vapor.
        /// </summary>
        public static void DrawHellpod(Vector2 pos, float scale = 1.0f, float time = 0f)
        {
            if (!_initialized) Initialize();

            // Si está aterrizada en tierra (time > 0): dibujar cráter de impacto quemado y los 4 pétalos abiertos
            if (time > 0f)
            {
                // Cráter quemado en el suelo
                Raylib.DrawCircleV(pos, 22f * scale, new Color(18, 16, 14, 210));
                Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, 24f * scale, new Color(45, 38, 30, 180));

                // 4 Pétalos estabilizadores abiertos en cruz diagonal (X)
                Vector2[] petalDirs = {
                    new(-18f * scale, -14f * scale),
                    new(18f * scale, -14f * scale),
                    new(-20f * scale, 12f * scale),
                    new(20f * scale, 12f * scale)
                };

                foreach (var pd in petalDirs)
                {
                    Vector2 pEnd = pos + pd;
                    Raylib.DrawLineEx(pos, pEnd, 5.5f * scale, OutlineBlack);
                    Raylib.DrawLineEx(pos, pEnd, 3.5f * scale, GunmetalLight);
                    Raylib.DrawLineEx(pos + pd * 0.4f, pEnd, 2f * scale, SuperEarthYellow);
                }

                // Fuste central cilíndrico abierto con interior iluminado
                Rectangle podCore = new(pos.X - 10 * scale, pos.Y - 14 * scale, 20 * scale, 28 * scale);
                Raylib.DrawRectangleRec(podCore, GunmetalDark);
                Raylib.DrawRectangleLinesEx(podCore, 1.5f * scale, OutlineBlack);
                Raylib.DrawRectangle((int)podCore.X + 3, (int)podCore.Y + 4, (int)podCore.Width - 6, 8, TacticalCyan); // Rack de suministro

                // Vaho / vapor de enfriamiento térmico
                float steamY = MathF.Sin(time * 6f) * 6f;
                Raylib.DrawCircleV(pos - new Vector2(0, 16 * scale + steamY), 6f * scale, new Color(220, 240, 255, 90));
            }
            else
            {
                // Cápsula cerrada en vuelo o descenso
                if (_hellpodTexture.Id != 0)
                {
                    Rectangle src = new(0, 0, _hellpodTexture.Width, _hellpodTexture.Height);
                    Rectangle dst = new(pos.X, pos.Y, 32 * scale, 48 * scale);
                    Vector2 origin = new(16 * scale, 40 * scale);
                    Raylib.DrawTexturePro(_hellpodTexture, src, dst, origin, 0f, Color.White);
                }
            }
        }

        /// <summary>
        /// Dibuja el caza táctico supersónico Eagle-1 inspirado en el diseño de cazabombardero de ataque:
        /// Ala delta afilada, timones dobles divergentes, cabina dorada reflectante, puntas de ala amarillas
        /// y postquemadores duales de plasma cian con conos de Mach.
        /// </summary>
        public static void DrawEagle1Jet(Vector2 jetPos, float time)
        {
            // Sombra en el suelo del caza volando a baja cota
            Raylib.DrawPoly(jetPos + new Vector2(0, 190f), 3, 44f, 65f, new Color(0, 0, 0, 75));

            // Estelas gemelas de condensación en las puntas de las alas
            Raylib.DrawLineEx(jetPos - new Vector2(28, -18), jetPos - new Vector2(380, -90), 3f, new Color(255, 255, 255, 140));
            Raylib.DrawLineEx(jetPos - new Vector2(-18, 28), jetPos - new Vector2(-340, 130), 3f, new Color(255, 255, 255, 140));

            // Fuselaje del Eagle-1 (Ala delta con morro estilizado y canards frontales)
            Vector2 nose = jetPos + new Vector2(46f, 16f);
            Vector2 wingLeft = jetPos - new Vector2(38f, 44f);
            Vector2 wingRight = jetPos - new Vector2(22f, -44f);
            Vector2 tail = jetPos - new Vector2(50f, 0f);

            // Contorno y fuselaje
            Raylib.DrawTriangle(nose + new Vector2(2, 0), wingLeft - new Vector2(2, 2), wingRight + new Vector2(2, 2), OutlineBlack);
            Raylib.DrawTriangle(nose, wingLeft, wingRight, GunmetalDark);
            Raylib.DrawTriangle(nose - new Vector2(6, 0), wingLeft + new Vector2(10, 6), wingRight - new Vector2(6, 10), GunmetalLight);

            // Acentos amarillos de Super Tierra en las puntas de ala y alerones
            Raylib.DrawLineEx(wingLeft, wingLeft + new Vector2(28, 12), 3.5f, SuperEarthYellow);
            Raylib.DrawLineEx(wingRight, wingRight + new Vector2(28, -12), 3.5f, SuperEarthYellow);

            // Canards aerodinámicos en la nariz
            Raylib.DrawTriangle(nose - new Vector2(14, 4), nose - new Vector2(24, 16), nose - new Vector2(26, 4), GunmetalDark);
            Raylib.DrawTriangle(nose - new Vector2(14, -4), nose - new Vector2(24, -16), nose - new Vector2(26, -4), GunmetalDark);

            // Timones verticales gemelos cantados hacia afuera
            Raylib.DrawLineEx(tail + new Vector2(8, -16), tail + new Vector2(-12, -28), 3f, GunmetalLight);
            Raylib.DrawLineEx(tail + new Vector2(8, 16), tail + new Vector2(-12, 28), 3f, GunmetalLight);
            Raylib.DrawCircleV(tail + new Vector2(-12, -28), 2f, SuperEarthYellow);
            Raylib.DrawCircleV(tail + new Vector2(-12, 28), 2f, SuperEarthYellow);

            // Cabina reflectante dorada aerodinámica
            Raylib.DrawEllipse((int)jetPos.X + 10, (int)jetPos.Y + 4, 11f, 5.5f, SuperEarthYellow);
            Raylib.DrawEllipse((int)jetPos.X + 11, (int)jetPos.Y + 4, 8f, 3.2f, Color.White);

            // Llamas de postcombustión de turbinas gemelas con diamantes de choque
            Vector2 engL = tail + new Vector2(-6f, -9f);
            Vector2 engR = tail + new Vector2(-6f, 9f);
            float afterburnerLen = 32f + MathF.Sin(time * 65f) * 10f;

            Raylib.DrawLineEx(engL, engL - new Vector2(afterburnerLen, 0), 4.5f, TacticalCyan);
            Raylib.DrawLineEx(engL, engL - new Vector2(afterburnerLen * 0.55f, 0), 2.5f, Color.White);
            Raylib.DrawLineEx(engR, engR - new Vector2(afterburnerLen, 0), 4.5f, TacticalCyan);
            Raylib.DrawLineEx(engR, engR - new Vector2(afterburnerLen * 0.55f, 0), 2.5f, Color.White);
        }

        /// <summary>
        /// Dibuja Torretas Centinela (Gatling, Autocannon) con ingeniería mecánica multi-pieza a 144 FPS:
        /// Trípode articulado con espolones de terreno, anillo dentado de rotación 360°, tambor/cañones gemelos,
        /// retroceso mecánico, eyección de casquillos y display holográfico de munición.
        /// </summary>
        public static void DrawSentryTurret(
            Vector2 pos,
            float turretAngleRad,
            string turretType,
            float ammoRatio,
            bool isFiring,
            float time = 0f)
        {
            if (time == 0f) time = (float)Raylib.GetTime();

            // 1. Sombra elíptica profunda en el suelo
            Raylib.DrawEllipse((int)pos.X, (int)pos.Y + 7, 28, 15, new Color(0, 0, 0, 115));

            // 2. Base Trípode con 3 patas hidráulicas articuladas
            float[] legAngles = { MathF.PI * 0.5f, MathF.PI * 1.16f, MathF.PI * 1.84f };
            foreach (float la in legAngles)
            {
                Vector2 legDir = new(MathF.Cos(la), MathF.Sin(la));
                Vector2 legEnd = pos + legDir * 22f;

                // Fémur mecánico de aleación oscura
                Raylib.DrawLineEx(pos, legEnd, 6.5f, OutlineBlack);
                Raylib.DrawLineEx(pos, legEnd, 4.2f, GunmetalDark);

                // Placa blindada protectora en la rodilla
                Raylib.DrawLineEx(pos + legDir * 8f, pos + legDir * 17f, 3.2f, SuperEarthYellow);

                // Espolón de anclaje clavado en el terreno
                Raylib.DrawRectangle((int)legEnd.X - 4, (int)legEnd.Y - 3, 8, 5, OutlineBlack);
                Raylib.DrawRectangle((int)legEnd.X - 3, (int)legEnd.Y - 2, 6, 3, GunmetalLight);
            }

            // Anillo dentado de rotación 360°
            Raylib.DrawCircleV(pos, 12f, OutlineBlack);
            Raylib.DrawCircleV(pos, 10f, GunmetalDark);
            Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, 10f, GunmetalLight);

            // 3. Cabezal de Artillería Giratorio
            Vector2 forward = new(MathF.Cos(turretAngleRad), MathF.Sin(turretAngleRad));
            Vector2 right = new(-forward.Y, forward.X);

            // Chasis central blindado
            Raylib.DrawCircleV(pos, 8.5f, GunmetalDark);
            Raylib.DrawCircleV(pos - forward * 2f, 6.5f, SuperEarthYellow);
            Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, 8.5f, OutlineBlack);

            // Sensor óptico de adquisición de objetivos (LED frontal)
            Color ledColor = isFiring ? AutomatonRed : (ammoRatio > 0.05f ? TacticalCyan : new Color(60, 60, 60, 255));
            Raylib.DrawCircleV(pos + forward * 5f, 2.5f, ledColor);

            // Láser de apuntado balístico táctico
            if (ammoRatio > 0f)
            {
                Color laserBeamCol = isFiring ? new Color(255, 45, 45, 175) : new Color(0, 228, 255, 75);
                Raylib.DrawLineEx(pos + forward * 12f, pos + forward * 200f, 1.2f, laserBeamCol);
            }

            // Radar táctico de escaneo perimetral en la parte trasera
            Vector2 radarPos = pos - forward * 7f;
            float rAngle = time * 4f;
            Vector2 dishDir = new(MathF.Cos(rAngle), MathF.Sin(rAngle));
            Raylib.DrawCircleV(radarPos, 3f, GunmetalLight);
            Raylib.DrawLineEx(radarPos - dishDir * 4f, radarPos + dishDir * 4f, 2f, SuperEarthYellow);
            Raylib.DrawCircleV(radarPos + dishDir * 4f, 1.5f, TacticalCyan);

            bool isAutocannon = turretType.Equals("Autocannon", StringComparison.OrdinalIgnoreCase) || turretType.Contains("auto");

            if (isAutocannon)
            {
                // --- TORRETA CAÑÓN AUTOMÁTICO A/AC-8 ---
                // Escudos balísticos inclinados a los flancos
                Vector2 shieldL1 = pos - right * 10f - forward * 8f;
                Vector2 shieldL2 = pos - right * 11f + forward * 10f;
                Raylib.DrawLineEx(shieldL1, shieldL2, 4.5f, OutlineBlack);
                Raylib.DrawLineEx(shieldL1, shieldL2, 2.5f, GunmetalDark);
                Raylib.DrawLineEx(pos - right * 10f - forward * 3f, pos - right * 11f + forward * 6f, 2f, SuperEarthYellow);

                Vector2 shieldR1 = pos + right * 10f - forward * 8f;
                Vector2 shieldR2 = pos + right * 11f + forward * 10f;
                Raylib.DrawLineEx(shieldR1, shieldR2, 4.5f, OutlineBlack);
                Raylib.DrawLineEx(shieldR1, shieldR2, 2.5f, GunmetalDark);
                Raylib.DrawLineEx(pos + right * 10f - forward * 3f, pos + right * 11f + forward * 6f, 2f, SuperEarthYellow);

                // Retroceso mecánico en los cañones gemelos
                float recoilL = isFiring ? MathF.Max(0f, MathF.Sin(time * 26f)) * 6f : 0f;
                float recoilR = isFiring ? MathF.Max(0f, -MathF.Sin(time * 26f)) * 6f : 0f;

                // Cañón izquierdo
                Vector2 bLeft = pos - right * 4.5f;
                Vector2 tipLeft = bLeft + forward * (32f - recoilL);
                Raylib.DrawLineEx(bLeft - forward * 4f, tipLeft, 5.5f, OutlineBlack);
                Raylib.DrawLineEx(bLeft, tipLeft, 3f, GunmetalLight);
                // Freno de boca ranurado en T
                Raylib.DrawLineEx(tipLeft - right * 3.5f, tipLeft + right * 3.5f, 3.5f, OutlineBlack);
                Raylib.DrawLineEx(tipLeft - right * 3.5f, tipLeft + right * 3.5f, 2f, GunmetalDark);

                // Cañón derecho
                Vector2 bRight = pos + right * 4.5f;
                Vector2 tipRight = bRight + forward * (32f - recoilR);
                Raylib.DrawLineEx(bRight - forward * 4f, tipRight, 5.5f, OutlineBlack);
                Raylib.DrawLineEx(bRight, tipRight, 3f, GunmetalLight);
                // Freno de boca ranurado en T
                Raylib.DrawLineEx(tipRight - right * 3.5f, tipRight + right * 3.5f, 3.5f, OutlineBlack);
                Raylib.DrawLineEx(tipRight - right * 3.5f, tipRight + right * 3.5f, 2f, GunmetalDark);

                if (isFiring)
                {
                    Vector2 muzzleFlashPos = pos + forward * 36f;
                    Raylib.DrawPoly(muzzleFlashPos, 6, 12f, time * 140f, SuperEarthYellow);
                    Raylib.DrawCircleV(muzzleFlashPos, 6f, Color.White);
                }
            }
            else
            {
                // --- TORRETA GATLING A/G-16 ---
                // Tambor de munición pesado lateral
                Vector2 drumPos = pos - right * 9f - forward * 3f;
                Raylib.DrawRectangle((int)drumPos.X - 5, (int)drumPos.Y - 5, 10, 10, GunmetalDark);
                Raylib.DrawRectangleLines((int)drumPos.X - 5, (int)drumPos.Y - 5, 10, 10, SuperEarthYellow);
                Raylib.DrawLineEx(drumPos, pos, 3f, new Color(210, 175, 55, 255)); // Cinta de balas

                // Haz rotativo de 6 cañones
                float spin = isFiring ? time * 48f : time * 3f;
                Vector2 rotorCenter = pos + forward * 22f;
                Raylib.DrawCircleV(rotorCenter, 5f, GunmetalDark);
                Raylib.DrawCircleLines((int)rotorCenter.X, (int)rotorCenter.Y, 5f, OutlineBlack);

                for (int b = 0; b < 6; b++)
                {
                    float bAng = spin + b * (MathF.PI / 3f);
                    Vector2 bOff = right * (MathF.Cos(bAng) * 4f);
                    Raylib.DrawLineEx(pos + bOff, rotorCenter + bOff, 2.2f, GunmetalLight);
                }

                if (isFiring)
                {
                    Vector2 muzzleFlashPos = rotorCenter + forward * 5f;
                    Raylib.DrawPoly(muzzleFlashPos, 5, 11f, time * 180f, SuperEarthYellow);
                    Raylib.DrawCircleV(muzzleFlashPos, 5f, Color.White);

                    // Casquillos de latón eyectándose
                    for (int c = 0; c < 2; c++)
                    {
                        Vector2 casing = pos + right * 8f + new Vector2(MathF.Sin(time * 35f + c) * 6f, MathF.Cos(time * 35f + c) * 6f);
                        Raylib.DrawRectangle((int)casing.X, (int)casing.Y, 3, 2, new Color(240, 205, 60, 255));
                    }
                }
            }

            // 4. Barra de Munición Holográfica Flotante
            Rectangle hudRec = new(pos.X - 18, pos.Y + 22, 36, 6);
            Raylib.DrawRectangleRec(hudRec, new Color(10, 15, 25, 220));
            Raylib.DrawRectangleLinesEx(hudRec, 1, OutlineBlack);
            float clampedAmmo = Math.Clamp(ammoRatio, 0f, 1f);
            Color ammoCol = clampedAmmo > 0.25f ? SuperEarthYellow : AutomatonRed;
            Raylib.DrawRectangle((int)hudRec.X + 1, (int)hudRec.Y + 1, (int)(34 * clampedAmmo), 4, ammoCol);
        }

        // =========================================================================
        // 5. ESTRUCTURAS DE MISIÓN (NIDOS, FÁBRICAS AUTÓMATAS, SILO ICBM) Y PELICAN-1
        // =========================================================================
        public static void DrawEnemyOutpost(Vector2 pos, Faction faction, float hpRatio, float time)
        {
            if (faction == Faction.Terminids)
            {
                // Cráter orgánico de Quitina y Huevos Pulsantes
                float pulse = 1f + MathF.Sin(time * 5f) * 0.06f;
                Raylib.DrawPoly(pos, 8, 48f * pulse, 22f, new Color(45, 32, 18, 255));
                Raylib.DrawPoly(pos, 8, 34f, 0f, new Color(20, 14, 8, 255));
                // Brillo de esporas y huevos de bilis
                Raylib.DrawCircleV(pos + new Vector2(-12, -8), 7f * pulse, TerminidBile);
                Raylib.DrawCircleV(pos + new Vector2(14, 10), 6f * pulse, TerminidBile);
                Raylib.DrawCircleV(pos, 14f, new Color(255, 140, 20, 90));
            }
            else
            {
                // Fábrica Autómata Blindada con Ventilas Rojas Ardientes
                Rectangle body = new(pos.X - 44, pos.Y - 36, 88, 72);
                Raylib.DrawRectangleRec(body, OutlineBlack);
                Raylib.DrawRectangle((int)body.X + 4, (int)body.Y + 4, 80, 64, GunmetalDark);
                // Franjas de advertencia y ventilas de fundición
                Raylib.DrawRectangle((int)pos.X - 30, (int)pos.Y - 26, 22, 12, AutomatonRed);
                Raylib.DrawRectangle((int)pos.X + 8, (int)pos.Y - 26, 22, 12, AutomatonRed);
                // Puerta frontal brillante
                byte glow = (byte)(180 + MathF.Sin(time * 8f) * 70);
                Raylib.DrawRectangle((int)pos.X - 18, (int)pos.Y + 10, 36, 22, new Color((byte)255, (byte)30, (byte)30, glow));
            }

            // Barra de salud de la estructura
            Raylib.DrawRectangle((int)pos.X - 36, (int)pos.Y - 52, 72, 7, OutlineBlack);
            Raylib.DrawRectangle((int)pos.X - 34, (int)pos.Y - 50, (int)(68 * Math.Clamp(hpRatio, 0f, 1f)), 3, AutomatonRed);
        }

        private static Texture2D GeneratePelicanSprite()
        {
            RenderTexture2D rt = Raylib.LoadRenderTexture(96, 64);
            Raylib.BeginTextureMode(rt);
            Raylib.ClearBackground(Color.Blank);

            Color oliveDark = new(42, 54, 40, 255);
            Color oliveMid  = new(58, 74, 55, 255);
            Color oliveLight= new(80, 102, 76, 255);

            // 1. Alas y góndolas VTOL (Alas superior e inferior)
            DrawPixelRect(24, 2, 28, 60, OutlineBlack);
            DrawPixelRect(26, 4, 24, 56, oliveDark);
            DrawPixelRect(28, 6, 20, 52, oliveMid);

            // Turbinas VTOL giratorias en los extremos de ala
            DrawPixelRect(30, 4, 16, 10, GunmetalDark);
            DrawPixelRect(32, 6, 12, 6, TacticalCyan); // Anillo de sustentación superior
            DrawPixelRect(34, 8, 8, 2, Color.White);

            DrawPixelRect(30, 50, 16, 10, GunmetalDark);
            DrawPixelRect(32, 52, 12, 6, TacticalCyan); // Anillo de sustentación inferior
            DrawPixelRect(34, 54, 8, 2, Color.White);

            // Franjas amarillas tácticas en las alas
            DrawPixelRect(26, 18, 24, 3, SuperEarthYellow);
            DrawPixelRect(26, 43, 24, 3, SuperEarthYellow);

            // 2. Fuselaje Principal Blindado (De popa izquierda a proa derecha)
            DrawPixelRect(10, 16, 72, 32, OutlineBlack);
            DrawPixelRect(12, 18, 68, 28, oliveDark);
            DrawPixelRect(16, 20, 60, 24, oliveMid);
            DrawPixelRect(22, 22, 48, 20, oliveLight);

            // Panel dorsal central y blindaje de techo
            DrawPixelRect(28, 26, 36, 12, GunmetalDark);
            DrawPixelRect(32, 28, 28, 8, GunmetalLight);

            // 3. Cabina de Cristal Multifacetada en el morro (Derecha)
            DrawPixelRect(70, 22, 16, 20, OutlineBlack);
            DrawPixelRect(72, 24, 12, 16, TacticalCyan);
            // Reflejo especular blanco en la cúpula
            DrawPixelRect(74, 26, 4, 12, Color.White);
            DrawPixelRect(80, 28, 4, 8, new Color(180, 245, 255, 255));

            // Torretas de cañón automático de morro (Chin Gun)
            DrawPixelRect(84, 30, 8, 4, GunmetalDark);
            DrawPixelRect(88, 31, 4, 2, OutlineBlack);

            // 4. Rampa de Abordaje Trasera (Izquierda)
            DrawPixelRect(6, 22, 8, 20, OutlineBlack);
            DrawPixelRect(8, 24, 6, 16, GunmetalDark);
            DrawPixelRect(10, 26, 4, 12, SuperEarthYellow); // Luces de guía de la rampa

            // 5. Patines de aterrizaje reforzados (Skids)
            DrawPixelRect(18, 14, 46, 3, OutlineBlack);
            DrawPixelRect(18, 47, 46, 3, OutlineBlack);

            Raylib.EndTextureMode();
            Raylib.SetTextureFilter(rt.Texture, TextureFilter.Point);
            return rt.Texture;
        }

        private static Texture2D GenerateHellpodSprite()
        {
            RenderTexture2D rt = Raylib.LoadRenderTexture(28, 44);
            Raylib.BeginTextureMode(rt);
            Raylib.ClearBackground(Color.Blank);

            // 1. Escudo térmico ablativo de reentrada (Base inferior acampanada)
            DrawPixelRect(4, 34, 20, 8, OutlineBlack);
            DrawPixelRect(5, 35, 18, 6, new Color(32, 22, 18, 255)); // Quemado por plasma
            DrawPixelRect(7, 37, 14, 2, new Color(180, 75, 25, 255)); // Borde incandescente

            // 2. Fuste principal cilíndrico de la cápsula
            DrawPixelRect(5, 12, 18, 24, OutlineBlack);
            DrawPixelRect(6, 13, 16, 22, GunmetalDark);
            DrawPixelRect(8, 14, 12, 20, GunmetalLight);

            // Franjas amarillas reglamentarias de Super Tierra
            DrawPixelRect(6, 18, 16, 4, SuperEarthYellow);
            DrawPixelRect(6, 26, 16, 4, SuperEarthYellow);

            // Estrella blanca de Super Tierra en el centro del morro
            DrawPixelRect(13, 20, 2, 2, Color.White);

            // 3. Cono superior de penetración atmosférica y toberas de control
            DrawPixelRect(7, 4, 14, 10, OutlineBlack);
            DrawPixelRect(8, 5, 12, 8, GunmetalDark);
            DrawPixelRect(10, 2, 8, 4, OutlineBlack);
            DrawPixelRect(11, 3, 6, 2, GunmetalLight);

            // Ventilas de propulsores RCS de dirección
            DrawPixelRect(6, 14, 2, 2, TacticalCyan);
            DrawPixelRect(20, 14, 2, 2, TacticalCyan);

            Raylib.EndTextureMode();
            Raylib.SetTextureFilter(rt.Texture, TextureFilter.Point);
            return rt.Texture;
        }

        /// <summary>
        /// Dibuja el transbordador Pelican-1 aterrizando o esperando extracción con propulsores VTOL.
        /// </summary>
        public static void DrawPelican1(Vector2 extractionPos, float landingAltitude, float rotationDeg, float time)
        {
            if (!_initialized) Initialize();

            float scale = 2.2f + (landingAltitude * 0.004f);
            // Sombra proyectada en el suelo
            Raylib.DrawEllipse(
                (int)(extractionPos.X + landingAltitude * 0.4f),
                (int)(extractionPos.Y + landingAltitude * 0.4f),
                85f,
                50f,
                new Color(0, 0, 0, 120)
            );

            Vector2 drawPos = extractionPos - new Vector2(0, landingAltitude);

            // Estelas de propulsores VTOL azules
            float jetPulse = 14f + MathF.Sin(time * 50f) * 5f;
            Raylib.DrawCircleV(drawPos + new Vector2(-18, -52), jetPulse, TacticalCyan);
            Raylib.DrawCircleV(drawPos + new Vector2(-18, 52), jetPulse, TacticalCyan);

            Rectangle src = new(0, 0, _pelicanTexture.Width, _pelicanTexture.Height);
            Rectangle dst = new(drawPos.X, drawPos.Y, _pelicanTexture.Width * scale, _pelicanTexture.Height * scale);
            Vector2 origin = new(dst.Width * 0.5f, dst.Height * 0.5f);

            Raylib.DrawTexturePro(_pelicanTexture, src, dst, origin, rotationDeg, Color.White);
        }

        // =========================================================================
        // 6. CAPA TÁCTICA CON FÍSICA VERLET PARA EL HELLDIVER
        // =========================================================================
        public static void UpdateAndDrawHelldiverCape(Vector2 playerPos, Vector2 playerVelocity, float aimAngleRad, float time)
        {
            Vector2 backDir = new(-MathF.Cos(aimAngleRad), -MathF.Sin(aimAngleRad));
            _capeNodes[0] = playerPos + backDir * 6f;

            for (int i = 1; i < _capeNodes.Length; i++)
            {
                Vector2 wind = new(
                    MathF.Cos(time * 8f + i) * 1.4f - playerVelocity.X * 0.04f,
                    MathF.Sin(time * 7f + i) * 1.4f - playerVelocity.Y * 0.04f
                );

                _capeNodes[i] += wind + backDir * 2.2f;

                // Restricción de distancia entre segmentos de la capa
                Vector2 diff = _capeNodes[i] - _capeNodes[i - 1];
                float dist = diff.Length();
                float maxSeg = 5.5f;
                if (dist > maxSeg && dist > 0.001f)
                {
                    _capeNodes[i] = _capeNodes[i - 1] + (diff / dist) * maxSeg;
                }
            }

            // Dibujar segmentos de la capa con borde negro y franja amarilla Super Tierra
            for (int i = 0; i < _capeNodes.Length - 1; i++)
            {
                float width = 12f + i * 1.6f;
                Raylib.DrawLineEx(_capeNodes[i], _capeNodes[i + 1], width + 3f, OutlineBlack);
                Raylib.DrawLineEx(_capeNodes[i], _capeNodes[i + 1], width, new Color(28, 30, 35, 255));
                if (i == _capeNodes.Length - 2)
                {
                    Raylib.DrawLineEx(_capeNodes[i], _capeNodes[i + 1], width * 0.6f, SuperEarthYellow);
                }
            }
        }

        private static void DrawPixelRect(int x, int y, int w, int h, Color col)
        {
            Raylib.DrawRectangle(x, y, w, h, col);
        }
    }
}
