using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace FabLab_Etiquette.Models
{
    public class LabelModel : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private BitmapImage _image;
        public event PropertyChangedEventHandler PropertyChanged;
        private string _fontFamily = "Arial"; // Défaut : Arial
        private double _fontSize = 20; // Défaut : 20
        private double _width;
        private double _height;
        private string _text;
        private Brush _backgroundColor = Brushes.White; // Couleur par défaut
        private Brush _borderColor = Brushes.Red;
        private double _borderThickness = 2; // Épaisseur par défaut
        private string _shape = "Rectangle"; // Forme par défaut : Rectangle
        private string _actionType;
        public string HorizontalAlignment { get; set; } = "Centre"; // Par défaut centré
        public string VerticalAlignment { get; set; } = "Milieu"; // Par défaut au milieu
        public string Action { get; set; } = "Découpe"; // Par défaut, une étiquette sera en "Découpe".


         public LabelModel()
         {
             ActionType = "Découpe"; // Par défaut
         }

         public string ActionType
         {
             get => _actionType;
             set
             {
                 _actionType = value;
                 OnPropertyChanged();
             }
         }
        public string Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                OnPropertyChanged();
            }
        }
        public BitmapImage Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }
        public double BorderThickness
        {
            get => _borderThickness;
            set
            {
                _borderThickness = value;
                OnPropertyChanged();
            }
        }
        public Brush BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                OnPropertyChanged();
            }
        }
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }
        
        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }
       
        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public string FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; OnPropertyChanged(); }
        }

        public double FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
