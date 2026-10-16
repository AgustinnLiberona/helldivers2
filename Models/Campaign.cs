using System.Numerics;

namespace HelldiversDesktop.Models;

public class Planet
{
    public string Name { get; set; } = "Malevelon Creek";
    public Faction Faction { get; set; } = Faction.Automatons;
    public string Biome { get; set; } = "creek";
    public int ThreatLevel { get; set; } = 7;
    public string Description { get; set; } = "";
}

public class Mission
{
    public MissionType Type { get; set; } = MissionType.Outposts;
    public string Title { get; set; } = "BÚSQUEDA Y DESTRUCCIÓN";
    public string Description { get; set; } = "Destruye todos los puestos y fábricas enemigas marcadas en el radar.";
}

public class Outpost
{
    public int Id;
    public Vector2 Position;
    public float Hp = 750;
    public float MaxHp = 750;
    public float Radius = 54f;
    public bool Destroyed = false;
    public float SpawnTimer = 4.0f;
}

public class MissionInteractable
{
    public string Type = "icbm"; // "icbm" or "probe"
    public int Id;
    public Vector2 Position;
    public float Radius = 65f;
    public float Progress = 0f;
    public float MaxProgress = 20f;
    public bool Completed = false;
    public bool Active = false;
}

public class Campaign
{
    public Planet Planet { get; set; } = new();
    public Mission Mission { get; set; } = new();
    public List<Outpost> Outposts { get; set; } = new();
    public List<MissionInteractable> Interactables { get; set; } = new();

    public static Campaign GenerateRandom(int playerLevel)
    {
        var rnd = new Random();
        var planets = new[]
        {
            new Planet { Name = "Malevelon Creek", Faction = Faction.Automatons, Biome = "creek", ThreatLevel = 8, Description = "Selva densa y oscuridad eterna donde las patrullas robóticas acechan tras la niebla." },
            new Planet { Name = "Hellmire", Faction = Faction.Terminids, Biome = "hellmire", ThreatLevel = 7, Description = "Planeta volcánico con tormentas ígneas y nidos masivos de Terminidos." },
            new Planet { Name = "Crimsica", Faction = Faction.Terminids, Biome = "crimsika", ThreatLevel = 6, Description = "Planeta de rocas rojas con esporas gigantescas y enjambres voraces." },
            new Planet { Name = "Vandalon IV", Faction = Faction.Automatons, Biome = "creek", ThreatLevel = 9, Description = "Complejo industrial polar de los Autómatas fuertemente fortificado." }
        };

        var selectedPlanet = planets[rnd.Next(planets.Length)];
        var missionTypes = new[] { MissionType.Outposts, MissionType.Icbm, MissionType.Seismic, MissionType.Assassinate };
        var selectedType = missionTypes[rnd.Next(missionTypes.Length)];

        var campaign = new Campaign
        {
            Planet = selectedPlanet,
            Mission = selectedType switch
            {
                MissionType.Outposts => new Mission
                {
                    Type = MissionType.Outposts,
                    Title = selectedPlanet.Faction == Faction.Automatons ? "DESTRUIR FÁBRICAS DE FABRICACIÓN" : "PURGAR NIDOS DE ESPORAS",
                    Description = "Localiza y destruye los 3 puestos principales fortificados antes de solicitar la extracción."
                },
                MissionType.Icbm => new Mission
                {
                    Type = MissionType.Icbm,
                    Title = "LANZAMIENTO DE MISIL ICBM",
                    Description = "Alcanza el silo balístico, inicia la secuencia de abastecimiento y defiende la posición."
                },
                MissionType.Seismic => new Mission
                {
                    Type = MissionType.Seismic,
                    Title = "ESTUDIO GEOLÓGICO SÍSMICO",
                    Description = "Activa las 2 sondas de prospección sísmica en los yacimientos de crudo marcados en el radar."
                },
                _ => new Mission
                {
                    Type = MissionType.Assassinate,
                    Title = selectedPlanet.Faction == Faction.Automatons ? "ELIMINAR HULK COMANDANTE" : "ERRADICAR TITÁN DE BILIS ÉLITE",
                    Description = "Localiza y aniquila al comandante supremo que lidera la fuerza enemiga en este sector."
                }
            }
        };

        // Populate objectives
        if (selectedType == MissionType.Outposts)
        {
            campaign.Outposts.Add(new Outpost { Id = 0, Position = new Vector2(Constants.WorldSize * 0.22f, Constants.WorldSize * 0.22f) });
            campaign.Outposts.Add(new Outpost { Id = 1, Position = new Vector2(Constants.WorldSize * 0.78f, Constants.WorldSize * 0.28f) });
            campaign.Outposts.Add(new Outpost { Id = 2, Position = new Vector2(Constants.WorldSize * 0.68f, Constants.WorldSize * 0.78f) });
        }
        else if (selectedType == MissionType.Icbm)
        {
            campaign.Interactables.Add(new MissionInteractable
            {
                Type = "icbm",
                Position = new Vector2(Constants.WorldSize * 0.72f, Constants.WorldSize * 0.28f),
                Radius = 75f,
                MaxProgress = 20f
            });
        }
        else if (selectedType == MissionType.Seismic)
        {
            campaign.Interactables.Add(new MissionInteractable
            {
                Type = "probe",
                Id = 1,
                Position = new Vector2(Constants.WorldSize * 0.25f, Constants.WorldSize * 0.35f),
                Radius = 50f,
                MaxProgress = 15f
            });
            campaign.Interactables.Add(new MissionInteractable
            {
                Type = "probe",
                Id = 2,
                Position = new Vector2(Constants.WorldSize * 0.75f, Constants.WorldSize * 0.72f),
                Radius = 50f,
                MaxProgress = 15f
            });
        }

        return campaign;
    }
}
