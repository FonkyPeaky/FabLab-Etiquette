using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media;
using Newtonsoft.Json;


namespace FabLab_Etiquette.Models
{
    public class LabelModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 📌 Propriétés existantes
        public double X { get; set; } // Position en pixels ou mm
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text { get; set; } = "Nouvelle étiquette";
        public double BorderThickness { get; set; } = 1.0;
        public string FontFamily { get; set; } = "Arial";
        public double FontSize { get; set; } = 12;
        public string HorizontalAlignment { get; set; } = "Centre";
        public string VerticalAlignment { get; set; } = "Milieu";
        public string Shape { get; set; } = "Rectangle";
        public string Action { get; set; } = "Découpe";

        // ✅ Stockage des couleurs en format Hex (au lieu d'objets `Brush`)
        public string BackgroundColorHex { get; set; } = "#FFFFFF"; // Blanc par défaut
        public string BorderColorHex { get; set; } = "#000000"; // Noir par défaut

        // ✅ Ignorer ces propriétés dans la sérialisation JSON (elles utilisent `Brush`)
        [System.Text.Json.Serialization.JsonIgnore]
        public Brush BackgroundColor
        {
            get => (Brush)new BrushConverter().ConvertFromString(BackgroundColorHex);
            set
            {
                BackgroundColorHex = new ColorConverter().ConvertToString(((SolidColorBrush)value).Color);
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public Brush BorderColor
        {
            get => (Brush)new BrushConverter().ConvertFromString(BorderColorHex);
            set
            {
                BorderColorHex = new ColorConverter().ConvertToString(((SolidColorBrush)value).Color);
                OnPropertyChanged(nameof(BorderColor));
            }
        }

        // ✅ Image associée à l'étiquette (si applicable)
        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        // 📌 Conversion Pixels ↔ Millimètres
        private const double PixelsPerMillimeter = 3.7795275591;

        public double XInMillimeters
        {
            get => X / PixelsPerMillimeter;
            set => X = value * PixelsPerMillimeter;
        }

        public double YInMillimeters
        {
            get => Y / PixelsPerMillimeter;
            set => Y = value * PixelsPerMillimeter;
        }

        public double WidthInMillimeters
        {
            get => Width / PixelsPerMillimeter;
            set => Width = value * PixelsPerMillimeter;
        }

        public double HeightInMillimeters
        {
            get => Height / PixelsPerMillimeter;
            set => Height = value * PixelsPerMillimeter;
        }

        public override string ToString()
        {
            return $"LabelModel: {Text}, Position: ({X}, {Y}), Dimensions: {Width}x{Height}, Couleur: {BackgroundColorHex}";
        }
    }
}
