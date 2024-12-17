using FabLab_Etiquette.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;


namespace FabLab_Etiquette.Services
{
    public static class PdfService
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
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            foreach (var label in labels)
            {
                // Conversion des positions et dimensions (unités PDF)
                double unitConversion = 0.75; // Ajustement pour les unités du PDF
                double pdfX = label.X * unitConversion;
                double pdfY = label.Y * unitConversion;
                double pdfWidth = label.Width * unitConversion;
                double pdfHeight = label.Height * unitConversion;

                var pen = new XPen(XColors.Red, label.BorderThickness);
                var brush = XBrushes.White; // Fond blanc
                var font = new XFont(label.FontFamily, label.FontSize);
                var textBrush = XBrushes.Black;

                // Dessiner les formes en fonction de Shape
                switch (label.Shape)
                {
                    case "Rectangle":
                        gfx.DrawRectangle(pen, brush, pdfX, pdfY, pdfWidth, pdfHeight);
                        break;

                    case "Ellipse":
                        gfx.DrawEllipse(pen, brush, pdfX, pdfY, pdfWidth, pdfHeight);
                        break;

                    case "Losange":
                        var points = new[]
                        {
                        new XPoint(pdfX + pdfWidth / 2, pdfY),
                        new XPoint(pdfX + pdfWidth, pdfY + pdfHeight / 2),
                        new XPoint(pdfX + pdfWidth / 2, pdfY + pdfHeight),
                        new XPoint(pdfX, pdfY + pdfHeight / 2)
                    };
                        gfx.DrawPolygon(pen, brush, points, XFillMode.Winding);
                        break;
                }

                // Ajouter le texte centré
                var layoutRect = new XRect(pdfX, pdfY, pdfWidth, pdfHeight);
                var format = new XStringFormat
                {
                    Alignment = XStringAlignment.Center,
                    LineAlignment = XLineAlignment.Center
                };

                gfx.DrawString(label.Text, font, textBrush, layoutRect, format);
            }

            // Sauvegarder le PDF
            document.Save(outputPath);
            document.Close();
        }
    }
}

