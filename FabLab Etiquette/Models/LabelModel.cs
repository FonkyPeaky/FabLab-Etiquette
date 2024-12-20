using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FabLab_Etiquette.Models
{
    public class LabelModel : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Existing properties
        public double X { get; set; } // Position in pixels or millimeters
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text { get; set; } = "Nouvelle étiquette";
        public double BorderThickness { get; set; } = 1.0;
        public Brush BackgroundColor { get; set; } = Brushes.White;
        public string FontFamily { get; set; } = "Arial";
        public double FontSize { get; set; } = 12;
        public string HorizontalAlignment { get; set; } = "Centre"; // Centre, Gauche, Droite
        public string VerticalAlignment { get; set; } = "Milieu";  // Haut, Milieu, Bas
        public string Shape { get; set; } = "Rectangle"; // Rectangle, Ellipse, Losange
        public string Action { get; set; } = "Découpe";

        // New properties
        private Brush borderColor = Brushes.Black; // Default value
        public Brush BorderColor
        {
            get => borderColor;
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string image;
        public string Image
        {
            get => image;
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged();
                }
            }
        }

        // Conversion properties for millimeters
        private const double PixelsPerMillimeter = 3.7795275591; // Conversion ratio

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
            return $"LabelModel: {Text}, Position: ({XInMillimeters} mm, {YInMillimeters} mm), " +
                   $"Dimensions: {WidthInMillimeters} mm x {HeightInMillimeters} mm, Shape: {Shape}";
        }
    }
}
