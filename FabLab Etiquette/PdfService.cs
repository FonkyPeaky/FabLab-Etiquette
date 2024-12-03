using FabLab_Etiquette.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.Generic;

namespace FabLab_Etiquette.Services
{
    public static class PdfService
    {
        public static void CreateLabelsPdf(IEnumerable<LabelModel> labels, string outputPath)
        {
            // Créer un nouveau document PDF
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Étiquettes FabLab";

            // Ajouter une page
            PdfPage page = document.AddPage();
            page.Width = XUnit.FromMillimeter(600);
            page.Height = XUnit.FromMillimeter(300);

            // Obtenir un contexte graphique
            XGraphics gfx = XGraphics.FromPdfPage(page);

            foreach (var label in labels)
            {
                // Dessiner le rectangle
                XPen pen = new XPen(XColors.Red, 0.02);
                gfx.DrawRectangle(pen, XBrushes.Transparent, label.X, label.Y, label.Width, label.Height);

                // Mesurer le texte
                XFont font = new XFont(label.FontFamily, label.FontSize);
                XSize textSize = gfx.MeasureString(label.Text, font);

                // Calculer la position pour centrer le texte
                double textX = label.X + (label.Width - textSize.Width) / 2;
                double textY = label.Y + (label.Height - textSize.Height) / 2 + textSize.Height;

                // Dessiner le texte
                gfx.DrawString(label.Text, font, XBrushes.Black, new XPoint(textX, textY));
            }

            // Sauvegarder le fichier
            document.Save(outputPath);
        }
    }
}
