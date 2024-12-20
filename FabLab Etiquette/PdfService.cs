using FabLab_Etiquette.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;


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

        public static void CreateLabelsPdf(ObservableCollection<LabelModel> labels, string outputPath)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            foreach (var label in labels)
            {
                var pen = new XPen(XColors.Red, label.BorderThickness); // Bordure rouge
                var brush = XBrushes.White; // Fond blanc

                // Dessin des formes
                switch (label.Shape)
                {
                    case "Rectangle":
                        gfx.DrawRectangle(pen, brush, XUnit.FromPoint(label.X), XUnit.FromPoint(label.Y), XUnit.FromPoint(label.Width), XUnit.FromPoint(label.Height));
                        break;

                    case "Ellipse":
                        gfx.DrawEllipse(pen, brush, XUnit.FromPoint(label.X), XUnit.FromPoint(label.Y), XUnit.FromPoint(label.Width), XUnit.FromPoint(label.Height));
                        break;

                    case "Losange":
                        var points = new XPoint[]
                        {
                        new XPoint(label.X + label.Width / 2, label.Y),
                        new XPoint(label.X + label.Width, label.Y + label.Height / 2),
                        new XPoint(label.X + label.Width / 2, label.Y + label.Height),
                        new XPoint(label.X, label.Y + label.Height / 2)
                        };
                        gfx.DrawPolygon(pen, brush, points, XFillMode.Winding);
                        break;
                }

                // Dessiner le texte
                var font = new XFont("Arial", label.FontSize);
                gfx.DrawString(label.Text, font, XBrushes.Black, new XRect(XUnit.FromPoint(label.X), XUnit.FromPoint(label.Y), XUnit.FromPoint(label.Width), XUnit.FromPoint(label.Height)), XStringFormats.Center);
            }

            // Sauvegarder le PDF
            document.Save(outputPath);
            document.Close();
        }
    }
}

