using FabLab_Etiquette.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Windows.Media;


namespace FabLab_Etiquette.Services
{
    public class PdfService
    {
        public static PdfDocument _document = new PdfDocument();
        public static PdfPage _currentPage;
        public static XGraphics _gfx;

        public static XGraphics GetGraphics()
        {
            if (_gfx == null)
            {
                if (_currentPage == null)
                {
                    _currentPage = _document.AddPage();
                    // Définir la taille de la page en points
                    // Exemple : 600 mm x 300 mm
                    double widthInMillimeters = 600;
                    double heightInMillimeters = 300;
                    double mmToPoint = 72.0 / 25.4; // 1 mm = 72 points / 25.4

                    _currentPage.Width = widthInMillimeters * mmToPoint;
                    _currentPage.Height = heightInMillimeters * mmToPoint;
                }
                _gfx = XGraphics.FromPdfPage(_currentPage);
            }
            return _gfx;
        }
        public static PdfPage GetPage()
        {
            if (_currentPage == null)
            {
                // Ajouter une nouvelle page si aucune page n'existe
                _currentPage = _document.AddPage();
                _currentPage.Size = PdfSharp.PageSize.A4; // Définir la taille de la page A4
            }
            return _currentPage;
        }

        public static void SavePdf(string outputPath)
        {
            // Sauvegarder le document PDF
            _document.Save(outputPath);
            _document.Close();
        }

        public static void ResetPdf()
        {
            // Réinitialiser le document pour un nouveau fichier
            _document = new PdfDocument();
            _currentPage = null;
        }

        public static void CreateLabelsPdf(IEnumerable<LabelModel> labels, string outputPath)
        {
            using (var document = new PdfDocument())
            {
                var page = document.AddPage();
                page.Width = XUnit.FromMillimeter(600);  // Largeur du PDF en mm
                page.Height = XUnit.FromMillimeter(300); // Hauteur du PDF en mm

                var gfx = XGraphics.FromPdfPage(page);

                foreach (var label in labels)
                {
                    // Dessiner les formes dans le PDF avec les dimensions réelles
                    var brush = new XSolidBrush(XColor.FromArgb(((SolidColorBrush)label.BackgroundColor).Color.A,
                                                                ((SolidColorBrush)label.BackgroundColor).Color.R,
                                                                ((SolidColorBrush)label.BackgroundColor).Color.G,
                                                                ((SolidColorBrush)label.BackgroundColor).Color.B));

                    var pen = new XPen(XColor.FromArgb(((SolidColorBrush)label.BorderColor).Color.A,
                                                       ((SolidColorBrush)label.BorderColor).Color.R,
                                                       ((SolidColorBrush)label.BorderColor).Color.G,
                                                       ((SolidColorBrush)label.BorderColor).Color.B), label.BorderThickness);

                    switch (label.Shape.ToLower())
                    {
                        case "rectangle":
                            gfx.DrawRectangle(pen, brush, label.X, label.Y, label.Width, label.Height);
                            break;

                        case "ellipse":
                            gfx.DrawEllipse(pen, brush, label.X, label.Y, label.Width, label.Height);
                            break;

                        case "losange":
                            var points = new[]
                            {
                        new XPoint(label.X + label.Width / 2, label.Y),                // Haut
                        new XPoint(label.X + label.Width, label.Y + label.Height / 2), // Droite
                        new XPoint(label.X + label.Width / 2, label.Y + label.Height), // Bas
                        new XPoint(label.X, label.Y + label.Height / 2)               // Gauche
                    };
                            gfx.DrawPolygon(pen, brush, points, XFillMode.Winding);
                            break;

                        default:
                            gfx.DrawRectangle(pen, brush, label.X, label.Y, label.Width, label.Height);
                            break;
                    }

                    // Dessiner le texte centré dans l'étiquette
                    var font = new XFont(label.FontFamily, label.FontSize);
                    var textBrush = label.Action?.ToLower() == "gravure" ? XBrushes.Black : XBrushes.Black;

                    gfx.DrawString(label.Text, font, textBrush,
                        new XRect(label.X, label.Y, label.Width, label.Height), XStringFormats.Center);
                }

                // Sauvegarder le document PDF
                document.Save(outputPath);
                System.Diagnostics.Debug.WriteLine($"PDF généré : {outputPath}");
            }
        }
    }
}

