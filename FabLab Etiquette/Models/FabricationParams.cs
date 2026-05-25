namespace FabLab_Etiquette.Models
{
    public class FabricationParams
    {
        public PlateStyle? SelectedStyle { get; set; }
        public string SelectedColor { get; set; } = "";
        public int PrintCount { get; set; } = 1;
        public string LabelTitle { get; set; } = "";
    }
}
