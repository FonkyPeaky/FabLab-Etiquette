using FabLab_Etiquette.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FabLab_Etiquette.Services
{
    public static class ConfigService
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FabLabEtiquette");

        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config?.PlateStyles?.Count > 0)
                        return config;
                }
            }
            catch { }

            var def = GetDefault();
            Save(def);
            return def;
        }

        public static void Save(AppConfig config)
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config,
                new JsonSerializerOptions { WriteIndented = true }));
        }

        public static AppConfig GetDefault() => new AppConfig
        {
            DefaultOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            PlateStyles = new List<PlateStyle>
            {
                new PlateStyle
                {
                    Name = "Étiquette souple",
                    Thickness = 0.2,
                    IsAdhesive = true,
                    AvailableColors = new List<string>
                    {
                        "Blanc sur fond Noir",
                        "Noir sur fond Blanc",
                        "Noir sur fond Jaune"
                    }
                },
                new PlateStyle
                {
                    Name = "Étiquette rigide",
                    Thickness = 1.6,
                    IsAdhesive = true,
                    AvailableColors = new List<string>
                    {
                        "Blanc sur fond Noir",
                        "Noir sur fond Blanc",
                        "Rouge sur fond Jaune",
                        "Blanc sur fond Vert",
                        "Rouge sur fond Blanc"
                    }
                },
                new PlateStyle
                {
                    Name = "Étiquette rigide",
                    Thickness = 1.6,
                    IsAdhesive = false,
                    AvailableColors = new List<string>
                    {
                        "Blanc sur fond Noir",
                        "Noir sur fond Blanc",
                        "Rouge sur fond Jaune",
                        "Blanc sur fond Vert",
                        "Noir sur fond Jaune"
                    }
                },
                new PlateStyle
                {
                    Name = "Étiquette très rigide",
                    Thickness = 3.2,
                    IsAdhesive = false,
                    AvailableColors = new List<string>
                    {
                        "Blanc sur fond Noir",
                        "Noir sur fond Blanc",
                        "Blanc sur fond Rouge",
                        "Rouge sur fond Blanc"
                    }
                }
            }
        };
    }
}
