using System.Numerics;

namespace HelldiversDesktop.Models;

public class BestiaryEntry
{
    public string Id { get; set; } = "";
    public string SpriteKey { get; set; } = "";
    public string Name { get; set; } = "";
    public string EnglishName { get; set; } = "";
    public string Codename { get; set; } = "";
    public Faction Faction { get; set; }
    public EnemyType EnemyType { get; set; }
    public int ThreatPips { get; set; } = 1; // 1 to 4
    public string ArmorClass { get; set; } = "";
    public string Weakpoints { get; set; } = "";
    public string Description { get; set; } = "";
    public string TacticalAdvice { get; set; } = "";
    public string RecommendedWeapons { get; set; } = "";
}

public static class BestiaryRegistry
{
    public static readonly List<BestiaryEntry> Entries = new()
    {
        // ═══════════════════════════════════════════════════════════════
        // TERMINIDS (12 ESPECIES - ENJAMBRE BIO-MUTANTE)
        // ═══════════════════════════════════════════════════════════════
        new BestiaryEntry
        {
            Id = "scavenger",
            SpriteKey = "scavenger",
            Name = "Carroñero",
            EnglishName = "Scavenger",
            Codename = "Insectoide de Colmena",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Scavenger,
            ThreatPips = 1,
            ArmorClass = "Ligero (Sin coraza)",
            Weakpoints = "Cuerpo completo (Chitina blanda)",
            Description = "Espécimen básico del enjambre termínido. Débil y lento en solitario, pero altamente peligroso en grupo. Si detecta peligro, levanta las patas delanteras y emite feromonas anaranjadas para abrir una brecha de enjambre.",
            TacticalAdvice = "Elimínalo de inmediato al detectar su postura de alarma antes de que libere esporas de brecha.",
            RecommendedWeapons = "Liberator, Subfusil Defender, Pistola reglamentaria."
        },
        new BestiaryEntry
        {
            Id = "pouncer",
            SpriteKey = "pouncer",
            Name = "Saltador",
            EnglishName = "Pouncer",
            Codename = "Insectoide Alado Ligero",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Scavenger,
            ThreatPips = 1,
            ArmorClass = "Ligero (Sin coraza)",
            Weakpoints = "Cabeza y alas",
            Description = "Variante menor con alas membranosas capaz de ejecutar pequeños saltos para cerrar distancia a gran velocidad y acosar a helldivers heridos.",
            TacticalAdvice = "Fuego automático en barrido continuo mientras te desplazas.",
            RecommendedWeapons = "Liberator, Breaker, Subfusil Defender."
        },
        new BestiaryEntry
        {
            Id = "warrior",
            SpriteKey = "warrior",
            Name = "Guerrero",
            EnglishName = "Warrior",
            Codename = "Tropa de Choque",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Warrior,
            ThreatPips = 2,
            ArmorClass = "Medio Ligero (Quitina ámbar)",
            Weakpoints = "Boca, articulaciones y patas",
            Description = "Soldado acorazado con gruesas placas quitinosas ámbar y mandíbulas dentadas capaces de partir blindaje balístico. Su sistema nervioso redundante le permite seguir atacando durante 3 segundos incluso decapitado.",
            TacticalAdvice = "No bajes la guardia tras arrancarle la cabeza; esquiva lateralmente hasta que colapse por completo.",
            RecommendedWeapons = "Fusil Liberator, Breaker, Dominator Jar-5."
        },
        new BestiaryEntry
        {
            Id = "hunter",
            SpriteKey = "hunter",
            Name = "Cazador",
            EnglishName = "Hunter",
            Codename = "Depredador Flanqueador",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Hunter,
            ThreatPips = 2,
            ArmorClass = "Ligero / Alta Agilidad",
            Weakpoints = "Cabeza, alas celestes y torso",
            Description = "Letal criatura verde lima con alas celestes y espolones frontales de mantis. Ejecuta saltos parabólicos acrobáticos de hasta 30 metros para envolver a los helldivers y clavar sus aguijones con neurotoxina ralentizante.",
            TacticalAdvice = "Nunca huyas en línea recta. Dispara retrocediendo y usa la Zambullida Táctica [ESPACIO] si salta hacia ti. Máxima prioridad en combate mixto.",
            RecommendedWeapons = "Escopeta Breaker, Fusil Liberator, Granada de Fragmentación."
        },
        new BestiaryEntry
        {
            Id = "stalker",
            SpriteKey = "stalker",
            Name = "Acechador",
            EnglishName = "Stalker",
            Codename = "Infiltrador Camuflado",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Hunter,
            ThreatPips = 3,
            ArmorClass = "Medio / Camuflaje Óptico",
            Weakpoints = "Cara, lengua retráctil y torso",
            Description = "Depredador alfa capaz de cloaking óptico cuasi invisible. Embosca por la espalda golpeando con sus garras de azote que arrojan al helldiver por los aires.",
            TacticalAdvice = "Rastrea el brillo translúcido de su camuflaje. Localiza y destruye su nido de acechadores de inmediato con granadas.",
            RecommendedWeapons = "Escopeta Breaker, Dominator Jar-5, Cañón Automático."
        },
        new BestiaryEntry
        {
            Id = "hive_guard",
            SpriteKey = "hive_guard",
            Name = "Guardia de Colmena",
            EnglishName = "Hive Guard",
            Codename = "Escudo Viviente",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Warrior,
            ThreatPips = 3,
            ArmorClass = "Medio Pesado (Cresta Frontal)",
            Weakpoints = "Flancos desprotegidos y abdomen trasero",
            Description = "Tanque biológico equipado con un pesado escudo cefálico que bloquea fuego frontal balístico. Se atrinchera al recibir daño cerrando sus placas.",
            TacticalAdvice = "No gastes munición frontal cuando se atrinchere; camina a su alrededor y dispara a su abdomen blando expuesto.",
            RecommendedWeapons = "Dominator Jar-5, Cañón Automático, Granadas de Impacto."
        },
        new BestiaryEntry
        {
            Id = "brood_commander",
            SpriteKey = "brood_commander",
            Name = "Comandante de Prole",
            EnglishName = "Brood Commander",
            Codename = "Líder de Enjambre",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Warrior,
            ThreatPips = 3,
            ArmorClass = "Medio Pesado Reforzado",
            Weakpoints = "Boca, uniones de patas y abdomen",
            Description = "Colosal guerrero líder con cuerno dorsal ramificado. Ruge para convocar refuerzos de guerreros y carga con furia suicida incluso sin cabeza.",
            TacticalAdvice = "Concéntrate en volar sus patas para reducir drásticamente su movilidad antes de rematar el tórax.",
            RecommendedWeapons = "Cañón Automático, Breaker, Dominator Jar-5."
        },
        new BestiaryEntry
        {
            Id = "bile_spewer",
            SpriteKey = "bile_spewer",
            Name = "Vomitador de Bilis",
            EnglishName = "Bile Spewer",
            Codename = "Artillería Bio-Cáustica",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Spitter,
            ThreatPips = 3,
            ArmorClass = "Medio (Saco dorsal vulnerable)",
            Weakpoints = "Cabeza / Saco dorsal de bilis (Detonación)",
            Description = "Insectoide colosal hinchado de fluidos gastrointestinales hiperácidos y pústulas amarillas dorsales. Dispara chorros caústicos a presión que licúan las defensas en segundos.",
            TacticalAdvice = "Apunta al abdomen bulboso amarillo con munición pesada o explosivos para provocar una detonación interna letal para los enemigos adyacentes.",
            RecommendedWeapons = "Dominator Jar-5, Cañón Automático, Ataque Aéreo Eagle."
        },
        new BestiaryEntry
        {
            Id = "nursing_spewer",
            SpriteKey = "nursing_spewer",
            Name = "Vomitador Nodriza",
            EnglishName = "Nursing Spewer",
            Codename = "Vomitador Ámbar",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Spitter,
            ThreatPips = 3,
            ArmorClass = "Medio Ligero",
            Weakpoints = "Cabeza y abdomen amarillo",
            Description = "Variante pálida/amarillenta del vomitador de bilis. Libera densas nubes de esporas fúngicas que ciegan los radares tácticos y rocía ácido viscoso.",
            TacticalAdvice = "Usa granadas de impacto o fuego concentrado al vientre para reventarlo antes de que cree niebla de esporas.",
            RecommendedWeapons = "Breaker, Dominator Jar-5, Granada de Fragmentación."
        },
        new BestiaryEntry
        {
            Id = "charger",
            SpriteKey = "charger",
            Name = "Embestidor",
            EnglishName = "Charger",
            Codename = "Ariete Acorazado",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Charger,
            ThreatPips = 4,
            ArmorClass = "Pesado Acorazado (Obsidiana)",
            Weakpoints = "Abdomen blando trasero anaranjado (Crítico 2.2x)",
            Description = "Enorme ariete viviente cubierto por un escudo frontal de roca y obsidiana impenetrable para calibres ligeros. Carga a velocidades de vértigo arrasando rocas y aplastando todo a su paso.",
            TacticalAdvice = "Espera a que inicie la carga y realiza una Zambullida Táctica [ESPACIO] hacia un lado en el último instante. Gira velozmente y vacía fuego concentrado en su vientre trasero expuesto.",
            RecommendedWeapons = "Cañón Automático, Cañón de Riel, Ataque Orbital 500KG, Torreta Automática."
        },
        new BestiaryEntry
        {
            Id = "bile_titan",
            SpriteKey = "bile_titan",
            Name = "Titán de Bilis",
            EnglishName = "Bile Titan",
            Codename = "Behemoth Colosal",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Titan,
            ThreatPips = 4,
            ArmorClass = "Ultra Pesado Titánico",
            Weakpoints = "Sacos ventrales verdes de ácido y boca abierta",
            Description = "La bio-arma definitiva de la plaga termínida. Se alza sobre 4 patas estiliformes titánicas que empalan blindados con facilidad. Arroja tormentas masivas de ácido corrosivo a cientos de metros de distancia.",
            TacticalAdvice = "Destruye primero sus sacos ventrales brillantes para anular su ataque de vómito ácido. Solicita apoyo aéreo inmediato (Bomba 500kg o Láser Orbital) apuntando a su cabeza.",
            RecommendedWeapons = "Bomba 500KG Eagle, Láser Orbital, Cañón de Riel antitanque."
        },
        new BestiaryEntry
        {
            Id = "shrieker",
            SpriteKey = "shrieker",
            Name = "Chirriador",
            EnglishName = "Shrieker",
            Codename = "Terror Aéreo",
            Faction = Faction.Terminids,
            EnemyType = EnemyType.Hunter,
            ThreatPips = 2,
            ArmorClass = "Ligero / Volador",
            Weakpoints = "Alas membranosas y cabeza",
            Description = "Murciélago insectoide gigante que anida en árboles de esporas gigantescos. Se lanza en picado a velocidad terminal decapitando helldivers con sus afiladas garras traseras.",
            TacticalAdvice = "Dispara a sus alas en pleno picado. Destruye sus nidos de chirriadores a larga distancia con cañones antitanque antes de acercarte.",
            RecommendedWeapons = "Fusil Liberator, Breaker, Cañón Automático a larga distancia."
        },

        // ═══════════════════════════════════════════════════════════════
        // AUTOMATONS (12 MODELOS - LEGIÓN SINTÉTICA)
        // ═══════════════════════════════════════════════════════════════
        new BestiaryEntry
        {
            Id = "trooper",
            SpriteKey = "trooper",
            Name = "Soldado Autómata",
            EnglishName = "Trooper",
            Codename = "Infantería Mecánica",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Trooper,
            ThreatPips = 1,
            ArmorClass = "Ligero (Aleación de acero)",
            Weakpoints = "Visor sensor carmesí (Cabeza)",
            Description = "Unidad básica de infantería esquelética de la legión autómata. Armada con carabina de pulso láser rojo. Si detecta la presencia de helldivers a distancia, levantará el brazo para disparar una bengala roja de alarma.",
            TacticalAdvice = "Disparo a la cabeza para eliminación instantánea antes de que dispare su baliza de refuerzos.",
            RecommendedWeapons = "Fusil Liberator, Subfusil Defender, Pistola reglamentaria."
        },
        new BestiaryEntry
        {
            Id = "raider",
            SpriteKey = "raider",
            Name = "Asaltante",
            EnglishName = "Raider / Marauder",
            Codename = "Fusilero de Asalto",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Trooper,
            ThreatPips = 1,
            ArmorClass = "Ligero Reforzado",
            Weakpoints = "Visor óptico y mochila",
            Description = "Tropa de choque de infantería con placas blindadas en el casco y hombreras rojas. Dispara ráfagas precisas de láser y lanza granadas térmicas.",
            TacticalAdvice = "Apunta al visor sensor rojo; un solo impacto certero de calibre estándar lo desactiva.",
            RecommendedWeapons = "Fusil Liberator, Dominator Jar-5, Breaker."
        },
        new BestiaryEntry
        {
            Id = "commissar",
            SpriteKey = "commissar",
            Name = "Comisario",
            EnglishName = "Commissar",
            Codename = "Oficial Táctico",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Commissar,
            ThreatPips = 2,
            ArmorClass = "Medio Ligero",
            Weakpoints = "Visor sensor frontal y núcleo de energía",
            Description = "Oficial de campo autómata distinguible por su gorra de plato militar, capa roja de mando y sable de energía térmica de combate cerrado. Dirige escuadras con precisión implacable y tiene máxima prioridad para ordenar fuego de apoyo.",
            TacticalAdvice = "Elimínalo primero en cualquier patrulla para impedir que lance bengalas de refuerzo táctico.",
            RecommendedWeapons = "Fusil Liberator, Escopeta Breaker, Dominator Jar-5."
        },
        new BestiaryEntry
        {
            Id = "berserker",
            SpriteKey = "berserker",
            Name = "Berserker",
            EnglishName = "Berserker",
            Codename = "Asalto Cuerpo a Cuerpo",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Berserker,
            ThreatPips = 3,
            ArmorClass = "Medio Reforzado",
            Weakpoints = "Cráneo pequeño, cintura articulada y torso",
            Description = "Sádico autómata de asalto con dos sierras mecánicas circulares giratorias en lugar de manos. Corre en línea recta desoyendo el fuego frontal recibido con el único objetivo de desmembrar helldivers.",
            TacticalAdvice = "Retrocede en sprint constante mientras disparas con ráfagas concentradas a la articulación pélvica o a su cráneo.",
            RecommendedWeapons = "Escopeta Breaker, Dominator Jar-5, Torreta Gatling."
        },
        new BestiaryEntry
        {
            Id = "devastator",
            SpriteKey = "devastator",
            Name = "Devastador",
            EnglishName = "Devastator",
            Codename = "Armadura Pesada Estándar",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Devastator,
            ThreatPips = 3,
            ArmorClass = "Blindaje Medio Pesado",
            Weakpoints = "Ojo visor rojo central (Cabeza diminuta)",
            Description = "Chasis pesado de combate forjado en acero templado. Avanza implacablemente disparando blásteres dobles pesados montados en sus antebrazos.",
            TacticalAdvice = "Usa cobertura rocosa y apunta con precisión al pequeño sensor óptico de su cabeza para derribarlo de un tiro perforante.",
            RecommendedWeapons = "Dominator Jar-5, Cañón de Riel, Cañón Automático."
        },
        new BestiaryEntry
        {
            Id = "heavy_devastator",
            SpriteKey = "heavy_devastator",
            Name = "Devastador Pesado",
            EnglishName = "Heavy Devastator",
            Codename = "Defensa de Asedio",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Devastator,
            ThreatPips = 3,
            ArmorClass = "Pesado con Escudo Balístico",
            Weakpoints = "Brazo expuesto de la minigun y cabeza sobre el escudo",
            Description = "Variante blindada equipada con un enorme escudo antimotines impenetrable y una ametralladora láser gatling pesada de alta cadencia de fuego.",
            TacticalAdvice = "Dispara a su hombro derecho expuesto cuando asome el cañón o lanza granadas de impacto justo a sus pies.",
            RecommendedWeapons = "Cañón Automático, Cañón de Riel, Granadas de Impacto."
        },
        new BestiaryEntry
        {
            Id = "rocket_devastator",
            SpriteKey = "rocket_devastator",
            Name = "Devastador Cohetero",
            EnglishName = "Rocket Devastator",
            Codename = "Plataforma de Cohetes",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Devastator,
            ThreatPips = 3,
            ArmorClass = "Blindaje Medio Pesado",
            Weakpoints = "Sensor óptico central y pods de cohetes dorsales",
            Description = "Brutal chasis pesado de combate armado con dos baterías de cohetes 2x2 sobre los hombros. Dispara salvas de cohetes de alta precisión que pulverizan posiciones defensivas.",
            TacticalAdvice = "Usa cobertura sólida de roca cuando veas que flexiona las piernas para apuntar sus cohetes. Un tiro preciso con arma de medio calibre a su visor rojo lo destruye.",
            RecommendedWeapons = "Dominator Jar-5, Cañón Automático, Cañón de Riel."
        },
        new BestiaryEntry
        {
            Id = "scout_strider",
            SpriteKey = "scout_strider",
            Name = "Zancudo Explorador",
            EnglishName = "Scout Strider",
            Codename = "Bípode de Reconocimiento",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Trooper,
            ThreatPips = 2,
            ArmorClass = "Escudo Frontal / Jinete Expuesto",
            Weakpoints = "Piloto autómata expuesto en la parte trasera",
            Description = "Caminante bípode blindado operado por un soldado autómata situado en la plataforma trasera descubierta. Sus cañones dobles suprimen helldivers a distancia.",
            TacticalAdvice = "Flanquea el zancudo o lanza una granada de fragmentación tras su escudo para matar al piloto instantáneamente.",
            RecommendedWeapons = "Dominator Jar-5, Granada de Impacto, Cañón Automático."
        },
        new BestiaryEntry
        {
            Id = "hulk",
            SpriteKey = "hulk",
            Name = "Hulk Acorazado",
            EnglishName = "Hulk",
            Codename = "Acorazado Pesado",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Hulk,
            ThreatPips = 4,
            ArmorClass = "Acorazado Pesado (Carbono-Acero)",
            Weakpoints = "Radiador térmico trasero incandescente (Crítico 2.5x)",
            Description = "Titán bípedo de asedio equipado con lanzallamas industrial de napalm en un brazo y una sierra mecánica gigante en el otro. Su placa frontal blindada desvía cualquier munición balística ligera o media.",
            TacticalAdvice = "Flanquea al monstruo utilizando señuelos o granadas aturdidoras. Dos disparos directos de Cañón Automático a su rejilla trasera incandescente causan una explosión catastrófica inmediata.",
            RecommendedWeapons = "Cañón de Riel, Cañón Automático a la espalda, Bomba 500KG, Láser Orbital."
        },
        new BestiaryEntry
        {
            Id = "tank",
            SpriteKey = "tank",
            Name = "Tanque Aniquilador",
            EnglishName = "Annihilator Tank",
            Codename = "Blindado de Orugas Pesado",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Hulk,
            ThreatPips = 4,
            ArmorClass = "Blindaje Ultra Pesado",
            Weakpoints = "Radiador térmico trasero en la torreta",
            Description = "Tanque pesado de orugas equipado con cañón de alta velocidad capaz de desintegrar búnkeres de un disparo y ametralladora coaxial para helldivers.",
            TacticalAdvice = "Aprovecha la lenta rotación de su torreta para correr a su espalda y disparar armas antitanque a las rejillas térmicas de ventilación.",
            RecommendedWeapons = "Láser Orbital, Bomba 500KG Eagle, Cañón de Riel."
        },
        new BestiaryEntry
        {
            Id = "factory_strider",
            SpriteKey = "factory_strider",
            Name = "Caminante de Fábrica",
            EnglishName = "Factory Strider",
            Codename = "Fortaleza Móvil",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.FactoryStrider,
            ThreatPips = 4,
            ArmorClass = "Fortaleza Ultra Pesada",
            Weakpoints = "Cañones gemelos de barbilla, compuerta inferior y generador",
            Description = "Enorme fortaleza cuadrúpeda andante capaz de demoler escuadras completas con sus cañones pesados frontales mientras fabrica y despliega patrullas de autómatas directamente desde su compuerta ventral.",
            TacticalAdvice = "Destruye prioritariamente las ametralladoras de su barbilla para crear un punto ciego frontal, luego ataca la compuerta inferior con apoyo aéreo masivo.",
            RecommendedWeapons = "Láser Orbital, Bomba 500KG Eagle, Cañón de Riel antitanque."
        },
        new BestiaryEntry
        {
            Id = "gunship",
            SpriteKey = "gunship",
            Name = "Cañonera de Asalto",
            EnglishName = "Gunship",
            Codename = "Patrulla Aérea",
            Faction = Faction.Automatons,
            EnemyType = EnemyType.Devastator,
            ThreatPips = 3,
            ArmorClass = "Medio Aéreo",
            Weakpoints = "Motores a reacción en las alas",
            Description = "Nave no tripulada VTOL fuertemente blindada con cañones láser y baterías de cohetes que patrulla zonas de alta prioridad buscando intrusos.",
            TacticalAdvice = "Apunta directamente a uno de sus motores con armas perforantes pesadas para desestabilizarla y provocar que se estrelle.",
            RecommendedWeapons = "Cañón Automático, Cañón de Riel, Torreta Automática."
        }
    };
}
