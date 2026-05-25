using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace FabLab_Etiquette.Models
{
    public class LabelModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private const double PixelsPerMillimeter = 3.7795275591;

        // ---- Propriétés avec notification ----

        private double _x;
        public double X { get => _x; set { _x = value; OnPropertyChanged(); } }

        private double _y;
        public double Y { get => _y; set { _y = value; OnPropertyChanged(); } }

        private double _width = 150;
        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); OnPropertyChanged(nameof(WidthInMillimeters)); }
        }

        private double _height = 60;
        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); OnPropertyChanged(nameof(HeightInMillimeters)); }
        }

        private string _text = "Nouvelle étiquette";
        public string Text { get => _text; set { _text = value; OnPropertyChanged(); } }

        private double _borderThickness = 1.0;
        public double BorderThickness { get => _borderThickness; set { _borderThickness = value; OnPropertyChanged(); } }

        private string _fontFamily = "Arial";
        public string FontFamily { get => _fontFamily; set { _fontFamily = value; OnPropertyChanged(); } }

        private double _fontSize = 12;
        public double FontSize { get => _fontSize; set { _fontSize = value; OnPropertyChanged(); } }

        private string _horizontalAlignment = "Centre";
        public string HorizontalAlignment { get => _horizontalAlignment; set { _horizontalAlignment = value; OnPropertyChanged(); } }

        private string _verticalAlignment = "Milieu";
        public string VerticalAlignment { get => _verticalAlignment; set { _verticalAlignment = value; OnPropertyChanged(); } }

        private string _shape = "Rectangle";
        public string Shape { get => _shape; set { _shape = value; OnPropertyChanged(); } }

        private string _action = "Découpe";
        public string Action { get => _action; set { _action = value; OnPropertyChanged(); } }

        // ---- Couleurs stockées en Hex ----

        private string _backgroundColorHex = "#FFFFFF";
        public string BackgroundColorHex
        {
            get => _backgroundColorHex;
            set { _backgroundColorHex = value; OnPropertyChanged(); OnPropertyChanged(nameof(BackgroundColor)); }
        }

        private string _borderColorHex = "#FF0000";
        public string BorderColorHex
        {
            get => _borderColorHex;
            set { _borderColorHex = value; OnPropertyChanged(); OnPropertyChanged(nameof(BorderColor)); }
        }

        [JsonIgnore]
        public Brush BackgroundColor
        {
            get => (Brush)new BrushConverter().ConvertFromString(BackgroundColorHex)!;
            set
            {
                BackgroundColorHex = new ColorConverter().ConvertToString(((SolidColorBrush)value).Color) ?? "#FFFFFF";
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Brush BorderColor
        {
            get => (Brush)new BrushConverter().ConvertFromString(BorderColorHex)!;
            set
            {
                BorderColorHex = new ColorConverter().ConvertToString(((SolidColorBrush)value).Color) ?? "#FF0000";
                OnPropertyChanged();
            }
        }

        // ---- Image ----

        private string? _imageSource;
        public string? ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(); }
        }

        // ---- Conversions pixels ↔ millimètres ----

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
            => $"LabelModel: {Text}, ({X},{Y}), {Width}×{Height}";
    }
}
