using PdfSharp.Drawing;
using System.Windows.Media;

namespace FabLab_Etiquette.Utils
{
    public static class ColorUtils
    {
        public static XColor ConvertBrushToXColor(Brush brush)
        {
            if (brush is SolidColorBrush solidColorBrush)
            {
                Color mediaColor = solidColorBrush.Color;
                return XColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
            }
            return XColors.White; // Retourne blanc par défaut si la conversion échoue
        }
    }
}
