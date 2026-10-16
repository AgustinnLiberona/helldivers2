using System.Numerics;

namespace HelldiversDesktop.Models;

public class StratagemDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public float Cooldown { get; set; } = 20f;
    public string Icon { get; set; } = "[S]";
}

public static class StratagemRegistry
{
    public static readonly Dictionary<string, StratagemDefinition> All = new()
    {
        ["eagle_500kg"] = new StratagemDefinition
        {
            Id = "eagle_500kg",
            Name = "Bomba Eagle de 500kg",
            Category = "Apoyo Aéreo",
            Description = "Explosión masiva de 1500 de daño que aniquila Titanes, Hulks y puestos enemigos.",
            Cooldown = 18f,
            Icon = "[500KG]"
        },
        ["sentry_gatling"] = new StratagemDefinition
        {
            Id = "sentry_gatling",
            Name = "Torreta Gatling A/G-16",
            Category = "Fortificación",
            Description = "Torreta automática de alta cadencia (600 RPM) que limpia enjambres enteros.",
            Cooldown = 22f,
            Icon = "[GATL]"
        },
        ["orbital_laser"] = new StratagemDefinition
        {
            Id = "orbital_laser",
            Name = "Láser Orbital",
            Category = "Orbital",
            Description = "Rayo cinético térmico continuo de 260 DPS que rastrea y calcina a los enemigos más pesados.",
            Cooldown = 30f,
            Icon = "[LASR]"
        },
        ["sentry_autocannon"] = new StratagemDefinition
        {
            Id = "sentry_autocannon",
            Name = "Torreta Cañón Automático",
            Category = "Fortificación",
            Description = "Dispara proyectiles pesados antiblindaje de 300 de daño con perforación múltiple.",
            Cooldown = 25f,
            Icon = "[AUTO]"
        },
        ["orbital_gas"] = new StratagemDefinition
        {
            Id = "orbital_gas",
            Name = "Ataque de Gas Orbital",
            Category = "Orbital",
            Description = "Nube tóxica corrosiva que inflige 60 DPS durante 12s a todo lo que penetre en el área.",
            Cooldown = 16f,
            Icon = "[GAS]"
        },
        ["precision_strike"] = new StratagemDefinition
        {
            Id = "precision_strike",
            Name = "Ataque Orbital de Precisión",
            Category = "Orbital",
            Description = "Impacto cinético directo de 1000 de daño con tiempo de recarga ultrarrápido.",
            Cooldown = 12f,
            Icon = "[PREC]"
        },
        ["guard_dog"] = new StratagemDefinition
        {
            Id = "guard_dog",
            Name = "Dron Guard Dog Rover",
            Category = "Equipo de Apoyo",
            Description = "Dron autónomo que orbita al Helldiver disparando un rayo láser continuo a amenazas cercanas.",
            Cooldown = 0f,
            Icon = "[ROVR]"
        }
    };
}

public class AirstrikeInstance
{
    public Vector2 Position;
    public float Timer;
    public float MaxTimer;
    public float Radius;
    public string Type = "500kg";
}

public class TurretInstance
{
    public Vector2 Position;
    public float Angle;
    public float Duration = 25f;
    public float MaxDuration = 25f;
    public float Cooldown;
    public float FireRate;
    public float Range;
    public string Type = "gatling";
}

public class OrbitalLaserInstance
{
    public Vector2 Position;
    public float Duration;
    public float DamagePerSec;
}

public class GasCloudInstance
{
    public Vector2 Position;
    public float Radius;
    public float Duration;
}
