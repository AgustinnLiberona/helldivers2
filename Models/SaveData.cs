using System.Text.Json;

namespace HelldiversDesktop.Models;

public class HangarUpgrades
{
    public int MaxHpLevel { get; set; } = 0;
    public int SpeedLevel { get; set; } = 0;
    public int DamageLevel { get; set; } = 0;
    public int ShieldLevel { get; set; } = 0;
}

public class GameSettings
{
    public float MasterVolume { get; set; } = 0.70f;
    public bool ShowDamageNumbers { get; set; } = true;
    public bool ScreenShake { get; set; } = true;
    public bool ShowFps { get; set; } = true;
    public bool AutoAimAssist { get; set; } = true;
    public bool Fullscreen { get; set; } = false;
}

public class SaveData
{
    public int Samples { get; set; } = 0;
    public int Medals { get; set; } = 0;
    public int OperationsCompleted { get; set; } = 0;
    public int PrestigeLevel { get; set; } = 1;
    public int PrestigeXp { get; set; } = 0;
    public string EquippedWeapon { get; set; } = "liberator";
    public List<string> EquippedStratagems { get; set; } = new() { "eagle_500kg", "sentry_gatling", "orbital_laser" };
    public HangarUpgrades Hangar { get; set; } = new();
    public GameSettings Settings { get; set; } = new();

    private const string SaveFileName = "save_data.json";

    public static SaveData Load()
    {
        try
        {
            if (File.Exists(SaveFileName))
            {
                var json = File.ReadAllText(SaveFileName);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                if (data != null)
                {
                    if (data.EquippedStratagems == null || data.EquippedStratagems.Count != 3)
                        data.EquippedStratagems = new() { "eagle_500kg", "sentry_gatling", "orbital_laser" };
                    if (data.Hangar == null)
                        data.Hangar = new();
                    if (data.Settings == null)
                        data.Settings = new();
                    return data;
                }
            }
        }
        catch { }

        return new SaveData();
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SaveFileName, json);
        }
        catch { }
    }

    public int GetXpNeededForRank()
    {
        return (int)Math.Round(120 * Math.Pow(PrestigeLevel, 1.25));
    }

    public void AddXp(int amount)
    {
        PrestigeXp += amount;
        while (PrestigeXp >= GetXpNeededForRank())
        {
            PrestigeXp -= GetXpNeededForRank();
            PrestigeLevel++;
        }
        Save();
    }

    public (string Title, string Icon) GetMilitaryRank()
    {
        return PrestigeLevel switch
        {
            1 => ("Cadete", "[★]"),
            2 => ("Soldado Espacial", "[★★]"),
            3 => ("Cabo Patriota", "[★★★]"),
            4 => ("Sargento Democrático", "[◆]"),
            5 => ("Maestro Sargento", "[◆◆]"),
            6 => ("Teniente de la Libertad", "[▲]"),
            7 => ("Capitán de la Verdad", "[▲▲]"),
            8 => ("Mayor Patriota", "[▼]"),
            9 => ("Coronel de la Super Tierra", "[✦]"),
            10 => ("General del Espacio", "[👑]"),
            _ => ($"Héroe de la Federación Nv.{PrestigeLevel}", "[★👑★]")
        };
    }
}
