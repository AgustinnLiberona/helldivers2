namespace HelldiversDesktop.Models;

public class Weapon
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeLabel { get; set; } = "";
    public string Description { get; set; } = "";
    public float Damage { get; set; } = 55;
    public float FireRate { get; set; } = 0.094f;
    public int Ammo { get; set; } = 45;
    public int Pellets { get; set; } = 1;
    public float Knockback { get; set; } = 0;
    public bool IsLaser { get; set; } = false;
    public bool IsDominator { get; set; } = false;
    public bool IsPlasma { get; set; } = false;
    public bool IsExplosive { get; set; } = false;
    public bool IsFlame { get; set; } = false;
    public bool IsAutocannon { get; set; } = false;
    public bool IsRailgun { get; set; } = false;
    public bool IsQuasar { get; set; } = false;
    public int Pierce { get; set; } = 1;
    public float RangeBonus { get; set; } = 0;
    public int Rpm { get; set; } = 640;
    public string SpecialTrait { get; set; } = "Estándar equilibrado";
    public string Icon { get; set; } = "[AR]";
}

public static class WeaponRegistry
{
    public static readonly Dictionary<string, Weapon> Weapons = new()
    {
        ["liberator"] = new Weapon
        {
            Key = "liberator",
            Name = "Liberator AR-23",
            TypeLabel = "Rifle de Asalto",
            Description = "Rifle de asalto estándar reglamentario de la Super Tierra. Cadencia y retroceso equilibrados.",
            Damage = 55,
            FireRate = 0.094f,
            Ammo = 45,
            Pellets = 1,
            Rpm = 640,
            SpecialTrait = "Estándar equilibrado",
            Icon = "[AR]"
        },
        ["breaker"] = new Weapon
        {
            Key = "breaker",
            Name = "Breaker SG-225",
            TypeLabel = "Escopeta Automática",
            Description = "Escopeta automática de choque para frenado letal de enjambres a corta distancia.",
            Damage = 41,
            FireRate = 0.20f,
            Ammo = 16,
            Pellets = 8,
            Rpm = 300,
            SpecialTrait = "330 Daño (8 perdigones x 41)",
            Icon = "[SG]"
        },
        ["punisher"] = new Weapon
        {
            Key = "punisher",
            Name = "Punisher SG-8",
            TypeLabel = "Escopeta de Bombeo",
            Description = "Escopeta de bombeo pesado de calibre masivo con retroceso severo que aturde y empuja.",
            Damage = 45,
            FireRate = 0.44f,
            Ammo = 16,
            Pellets = 9,
            Knockback = 14f,
            Rpm = 135,
            SpecialTrait = "405 Daño + Retroceso Severo",
            Icon = "[ST]"
        },
        ["sickle"] = new Weapon
        {
            Key = "sickle",
            Name = "Sickle LAS-16",
            TypeLabel = "Láser Térmico",
            Description = "Rifle de energía de disparo térmico continuo a cadencia extrema sin retroceso balístico.",
            Damage = 55,
            FireRate = 0.08f,
            Ammo = 75,
            Pellets = 1,
            IsLaser = true,
            Rpm = 750,
            SpecialTrait = "Disipador Térmico (75 disparos)",
            Icon = "[LS]"
        },
        ["dominator"] = new Weapon
        {
            Key = "dominator",
            Name = "Dominator JAR-5",
            TypeLabel = "Giroscópico Cohetes",
            Description = "Lanza proyectiles con propulsión por jet guiados cinéticamente con penetración pesada.",
            Damage = 275,
            FireRate = 0.28f,
            Ammo = 15,
            Pellets = 1,
            IsExplosive = true,
            IsDominator = true,
            Rpm = 215,
            SpecialTrait = "275 Daño Impacto Explosivo",
            Icon = "[RK]"
        },
        ["diligence"] = new Weapon
        {
            Key = "diligence",
            Name = "Diligence CS R-63",
            TypeLabel = "Tirador Designado",
            Description = "Rifle de tirador designado de alta precisión con visor ampliado y penetración en línea.",
            Damage = 140,
            FireRate = 0.28f,
            Ammo = 20,
            Pellets = 1,
            Pierce = 2,
            RangeBonus = 250f,
            Rpm = 215,
            SpecialTrait = "Perforación x2 + Alcance (+250m)",
            Icon = "[SR]"
        },
        ["scorcher"] = new Weapon
        {
            Key = "scorcher",
            Name = "Scorcher PLAS-1",
            TypeLabel = "Plasma de Alta Energía",
            Description = "Dispara proyectiles de gas ionizado supercalentado que provocan deflagraciones cinéticas de área.",
            Damage = 100,
            FireRate = 0.24f,
            Ammo = 15,
            Pellets = 1,
            IsPlasma = true,
            Rpm = 250,
            SpecialTrait = "100 Directo + 100 Área (200 Total)",
            Icon = "[PL]"
        },
        ["flamethrower"] = new Weapon
        {
            Key = "flamethrower",
            Name = "Lanzallamas FLAM-40",
            TypeLabel = "Soporte Incendiario",
            Description = "Proyecta un torrente continuo de napalm a presión que calcina hordas e ignora armaduras ligeras.",
            Damage = 28,
            FireRate = 0.05f,
            Ammo = 220,
            Pellets = 3,
            IsFlame = true,
            Rpm = 1200,
            SpecialTrait = "Chorro de Fuego Continuo en Cono",
            Icon = "[FL]"
        },
        ["autocannon"] = new Weapon
        {
            Key = "autocannon",
            Name = "Autocañón AC-8",
            TypeLabel = "Cañón Pesado Balístico",
            Description = "Cañón de 20mm semiautomático con munición explosiva perforante capaz de demoler blindajes y fábricas.",
            Damage = 380,
            FireRate = 0.32f,
            Ammo = 12,
            Pellets = 1,
            Knockback = 20f,
            IsExplosive = true,
            IsAutocannon = true,
            Pierce = 3,
            Rpm = 185,
            SpecialTrait = "380 Daño + Detonación Perforante",
            Icon = "[AC]"
        },
        ["railgun"] = new Weapon
        {
            Key = "railgun",
            Name = "Cañón de Riel RS-422",
            TypeLabel = "Anti-Material Magnético",
            Description = "Acelerador electromagnético lineal que dispara un dardo de tungsteno hiper-acelerado que atraviesa cualquier objetivo.",
            Damage = 750,
            FireRate = 0.85f,
            Ammo = 20,
            Pellets = 1,
            Knockback = 28f,
            IsRailgun = true,
            Pierce = 99,
            RangeBonus = 400f,
            Rpm = 70,
            SpecialTrait = "Perforación Total + 750 Daño",
            Icon = "[RL]"
        },
        ["quasar"] = new Weapon
        {
            Key = "quasar",
            Name = "Cañón Quásar LAS-99",
            TypeLabel = "Energía Pesada Infinita",
            Description = "Condensador de plasma estelar que dispara una esfera de energía devastadora. Sin consumo de munición balística.",
            Damage = 950,
            FireRate = 1.35f,
            Ammo = 999,
            Pellets = 1,
            Knockback = 35f,
            IsExplosive = true,
            IsQuasar = true,
            Rpm = 44,
            SpecialTrait = "Munición Infinita + Pulso 950 Daño",
            Icon = "[QS]"
        }
    };

    public static Weapon GetWeapon(string key)
    {
        return Weapons.TryGetValue(key, out var w) ? w : Weapons["liberator"];
    }
}
