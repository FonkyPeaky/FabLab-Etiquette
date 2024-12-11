using FabLab_Etiquette.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace FabLab_Etiquette.Services
{
    public static class PdfService
    {
        private static PdfSharp.Drawing.XColor ConvertToXColor(System.Windows.Media.Color mediaColor)
        {
            return PdfSharp.Drawing.XColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }
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
                // Dessiner le fond
                XBrush backgroundBrush = new XSolidBrush(ConvertToXColor((label.BackgroundColor as SolidColorBrush).Color));
                gfx.DrawRectangle(backgroundBrush, label.X, label.Y, label.Width, label.Height);

                // Dessiner les bordures
                XPen borderPen = new XPen(ConvertToXColor((label.BorderColor as SolidColorBrush).Color), label.BorderThickness);
                gfx.DrawRectangle(borderPen, label.X, label.Y, label.Width, label.Height);

                // Dessiner le texte
                XFont font = new XFont(label.FontFamily, label.FontSize);
                gfx.DrawString(label.Text, font, XBrushes.Black, new XPoint(label.X + 5, label.Y + 15));

                // Dessiner l'image si disponible
                if (label.Image != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(label.Image));
                        encoder.Save(stream);

                        stream.Seek(0, SeekOrigin.Begin);
                        XImage xImage = XImage.FromStream(stream);

                        gfx.DrawImage(xImage, label.X, label.Y, label.Width, label.Height);
                    }
                }
            }

            document.Save(outputPath);
            System.Windows.MessageBox.Show($"PDF généré avec succès : {outputPath}");
        }
    }
}
