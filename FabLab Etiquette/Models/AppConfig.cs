using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FabLab_Etiquette.Models
{
    public class PlateStyle
    {
        public string Name { get; set; } = "";
        public double Thickness { get; set; }
        public bool IsAdhesive { get; set; }
        public List<string> AvailableColors { get; set; } = new();

        [JsonIgnore]
        public string DisplayName =>
            $"{Name} - {Thickness} mm - {(IsAdhesive ? "autocollante" : "non autocollante")}";

        [JsonIgnore]
        public string StyleCode =>
            $"{Thickness}_{(IsAdhesive ? "autocollant" : "non autocollant")}";
    }

    public class AppConfig
    {
        public string DefaultOutputPath { get; set; } = "";
        public List<PlateStyle> PlateStyles { get; set; } = new();
    }
}
