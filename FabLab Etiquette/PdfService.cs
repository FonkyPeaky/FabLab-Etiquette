using FabLab_Etiquette.Models;
using FabLab_Etiquette.Views;
using PdfSharp;
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
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PageSize.A4;

            var gfx = XGraphics.FromPdfPage(page);

            foreach (var label in labels)
            {
                XBrush brush = null;
                XPen pen = null;

                // Définir les couleurs en fonction de l'action
                if (label.Action == "Découpe")
                {
                    pen = new XPen(XColors.Red, label.BorderThickness);
                }
                else if (label.Action == "Gravure")
                {
                    pen = new XPen(XColors.Black, label.BorderThickness);
                    brush = new XSolidBrush(XColors.Black);
                }

                // Dessiner les formes en fonction de la propriété Shape
                if (label.Shape == "Rectangle")
                {
                    gfx.DrawRectangle(pen, brush, label.X, label.Y, label.Width, label.Height);
                }
                else if (label.Shape == "Ellipse")
                {
                    gfx.DrawEllipse(pen, brush, label.X, label.Y, label.Width, label.Height);
                }
                else if (label.Shape == "Losange")
                {
                    var points = new[]
                    {
                new XPoint(label.X + label.Width / 2, label.Y), // Haut
                new XPoint(label.X + label.Width, label.Y + label.Height / 2), // Droite
                new XPoint(label.X + label.Width / 2, label.Y + label.Height), // Bas
                new XPoint(label.X, label.Y + label.Height / 2) // Gauche
            };
                    gfx.DrawPolygon(pen, brush, points, XFillMode.Winding);
                }

                // Ajouter le texte
                if (!string.IsNullOrWhiteSpace(label.Text))
                {
                    var font = new XFont(label.FontFamily, label.FontSize);

                    var textBrush = label.Action?.ToLower() == "gravure" ? XBrushes.Black : XBrushes.Black;

                    // Dessiner le texte
                    gfx.DrawString(label.Text, font, textBrush,
                        new XRect(label.X, label.Y, label.Width, label.Height),
                        XStringFormats.Center);

                    // Centrer le texte
                    var layoutRect = new XRect(label.X, label.Y, label.Width, label.Height);
                    var format = new XStringFormat
                    {
                        Alignment = XStringAlignment.Center,
                        LineAlignment = XLineAlignment.Center
                    };
                    gfx.DrawString(label.Text, font, textBrush, layoutRect, format);
                }
            }

            // Sauvegarder le PDF
            try
            {
                
                if (File.Exists(outputPath))
                {
                    // Vérifier si le fichier est ouvert par un autre processus
                    File.Delete(outputPath);
                }

                // Sauvegarder le PDF
                document.Save(outputPath);

                // Ouvrir l'aperçu PDF dans l'application (si besoin)
                var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                view?.PdfPreview.Navigate(outputPath);
            }
            catch (IOException ex)
            {
                System.Windows.MessageBox.Show($"Adobe Reader a besoin d'etre actualisé veuillez cliquer sur OK .\n\nDétails : {ex.Message}");
            }

            document.Close();
        }

    }
}
