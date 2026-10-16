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
    public bool Completed { get; set; } = false;
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

    // Estado específico para nuevos tipos de misiones
    public int KillsRequired { get; set; } = 0;
    public int CurrentKills { get; set; } = 0;
    public float DefenseTimer { get; set; } = 0f;
    public float MaxDefenseTimer { get; set; } = 0f;
    public Vector2 DefensePos { get; set; } = Vector2.Zero;
    public float DefenseStationHp { get; set; } = 1200f;
    public float DefenseStationMaxHp { get; set; } = 1200f;
    public List<Vector2> SampleContainers { get; set; } = new();
    public List<bool> SamplesCollected { get; set; } = new();
    public List<MissionInteractable> SabotageNodes { get; set; } = new();

    public static Campaign GenerateRandom(int playerLevel)
    {
        var rnd = new Random();
        var planets = new[]
        {
            new Planet { Name = "Malevelon Creek", Faction = Faction.Automatons, Biome = "creek", ThreatLevel = 8, Description = "Selva densa y oscuridad eterna donde las patrullas robóticas acechan tras la niebla." },
            new Planet { Name = "Hellmire", Faction = Faction.Terminids, Biome = "hellmire", ThreatLevel = 7, Description = "Planeta volcánico con tormentas ígneas y nidos masivos de Terminidos." },
            new Planet { Name = "Crimsica", Faction = Faction.Terminids, Biome = "crimsika", ThreatLevel = 6, Description = "Planeta de rocas rojas con esporas gigantescas y enjambres voraces." },
            new Planet { Name = "Vandalon IV", Faction = Faction.Automatons, Biome = "vandalon", ThreatLevel = 9, Description = "Complejo industrial polar de los Autómatas fuertemente fortificado." },
            new Planet { Name = "Cyberstan", Faction = Faction.Automatons, Biome = "cyberstan", ThreatLevel = 10, Description = "Planeta fortaleza natal de los Cyborgs. Fábricas pesadas de Warlords, ríos de escoria y hornos incandescentes." },
            new Planet { Name = "Calypso", Faction = Faction.Illuminate, Biome = "calypso", ThreatLevel = 8, Description = "Mundo en ruinas de los Iluminados, con obeliscos de cristal psiónico y tecnología dimensional alienígena." },
            new Planet { Name = "Squ'th", Faction = Faction.Illuminate, Biome = "squth", ThreatLevel = 9, Description = "Ciudadela imperial ancestral de los Iluminados con templos de mármol y barreras de energía impenetrable." }
        };

        var selectedPlanet = planets[rnd.Next(planets.Length)];
        var missionTypes = new[]
        {
            MissionType.Outposts,
            MissionType.Icbm,
            MissionType.Seismic,
            MissionType.Assassinate,
            MissionType.EradicateHive,
            MissionType.ExtractSamples,
            MissionType.HoldTheLine,
            MissionType.SabotageFactory
        };
        var selectedType = missionTypes[rnd.Next(missionTypes.Length)];

        var campaign = new Campaign
        {
            Planet = selectedPlanet,
            Mission = selectedType switch
            {
                MissionType.Outposts => new Mission
                {
                    Type = MissionType.Outposts,
                    Title = selectedPlanet.Faction == Faction.Automatons ? "DESTRUIR FÁBRICAS DE FABRICACIÓN" : (selectedPlanet.Faction == Faction.Illuminate ? "DESMANTELAR PORTALES DIMENSIONALES" : "PURGAR NIDOS DE ESPORAS"),
                    Description = "Localiza y destruye las 3 estructuras principales enemigas antes de solicitar la extracción."
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
                    Description = "Activa las 2 sondas de prospección sísmica en los yacimientos marcados en el radar."
                },
                MissionType.Assassinate => new Mission
                {
                    Type = MissionType.Assassinate,
                    Title = selectedPlanet.Faction == Faction.Automatons ? "ELIMINAR WARLORD COMANDANTE" : (selectedPlanet.Faction == Faction.Illuminate ? "NEUTRALIZAR MIEMBRO DEL CONSEJO" : "ERRADICAR TITÁN DE BILIS ÉLITE"),
                    Description = "Localiza y aniquila al comandante supremo que lidera la fuerza enemiga en este sector."
                },
                MissionType.EradicateHive => new Mission
                {
                    Type = MissionType.EradicateHive,
                    Title = selectedPlanet.Faction == Faction.Automatons ? "PURGA DE LEGIÓN SINTÉTICA" : (selectedPlanet.Faction == Faction.Illuminate ? "SUPRESIÓN DE FUERZA ILUMINADA" : "ERRADICACIÓN DE ENJAMBRE BIO-MUTANTE"),
                    Description = "Extermina a 75 unidades hostiles en el sector para asegurar la supremacía de la Super Tierra."
                },
                MissionType.ExtractSamples => new Mission
                {
                    Type = MissionType.ExtractSamples,
                    Title = "EXTRACCIÓN DE MUESTRAS SUPER-RARAS",
                    Description = "Recupera 5 contenedores de muestras científicas esparcidos en territorio hostil."
                },
                MissionType.HoldTheLine => new Mission
                {
                    Type = MissionType.HoldTheLine,
                    Title = "DEFENSA DE RELÉ DE TRANSMISIÓN",
                    Description = "Protege el relé de comunicaciones de Super Tierra durante 45 segundos contra oleadas enemigas masivas."
                },
                _ => new Mission
                {
                    Type = MissionType.SabotageFactory,
                    Title = selectedPlanet.Faction == Faction.Automatons ? "SABOTAJE DE COMPLEJO FABRIL" : (selectedPlanet.Faction == Faction.Illuminate ? "SOBRECARGA DE CONDENSADORES PSÍONICOS" : "ESTERILIZACIÓN DE NODOS REINAS"),
                    Description = "Planta cargas de demolición en los 3 nodos estratégicos y sobrevive a las detonaciones."
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
        else if (selectedType == MissionType.EradicateHive)
        {
            campaign.KillsRequired = 75;
            campaign.CurrentKills = 0;
        }
        else if (selectedType == MissionType.ExtractSamples)
        {
            // 5 Contenedores de muestras científicas repartidos
            campaign.SampleContainers.Add(new Vector2(Constants.WorldSize * 0.30f, Constants.WorldSize * 0.30f));
            campaign.SampleContainers.Add(new Vector2(Constants.WorldSize * 0.70f, Constants.WorldSize * 0.25f));
            campaign.SampleContainers.Add(new Vector2(Constants.WorldSize * 0.50f, Constants.WorldSize * 0.52f));
            campaign.SampleContainers.Add(new Vector2(Constants.WorldSize * 0.22f, Constants.WorldSize * 0.75f));
            campaign.SampleContainers.Add(new Vector2(Constants.WorldSize * 0.80f, Constants.WorldSize * 0.78f));
            for (int i = 0; i < 5; i++) campaign.SamplesCollected.Add(false);
        }
        else if (selectedType == MissionType.HoldTheLine)
        {
            campaign.DefensePos = new Vector2(Constants.WorldSize * 0.5f, Constants.WorldSize * 0.5f);
            campaign.DefenseTimer = 45f;
            campaign.MaxDefenseTimer = 45f;
            campaign.DefenseStationHp = 1500f;
            campaign.DefenseStationMaxHp = 1500f;
        }
        else if (selectedType == MissionType.SabotageFactory)
        {
            campaign.SabotageNodes.Add(new MissionInteractable
            {
                Type = "sabotage",
                Id = 1,
                Position = new Vector2(Constants.WorldSize * 0.28f, Constants.WorldSize * 0.32f),
                Radius = 55f,
                MaxProgress = 6f
            });
            campaign.SabotageNodes.Add(new MissionInteractable
            {
                Type = "sabotage",
                Id = 2,
                Position = new Vector2(Constants.WorldSize * 0.72f, Constants.WorldSize * 0.40f),
                Radius = 55f,
                MaxProgress = 6f
            });
            campaign.SabotageNodes.Add(new MissionInteractable
            {
                Type = "sabotage",
                Id = 3,
                Position = new Vector2(Constants.WorldSize * 0.48f, Constants.WorldSize * 0.78f),
                Radius = 55f,
                MaxProgress = 6f
            });
        }

        return campaign;
    }
}
