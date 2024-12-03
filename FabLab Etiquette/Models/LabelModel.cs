using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FabLab_Etiquette.Models
{
    public class LabelModel
    {
        public double X { get; set; } // Position X
        public double Y { get; set; } // Position Y
        public double Width { get; set; } // Largeur
        public double Height { get; set; } // Hauteur
        public string Text { get; set; } // Texte
        public string FontFamily { get; set; } = "Arial"; // Police
        public double FontSize { get; set; } = 20; // Taille de la police
    }
}
