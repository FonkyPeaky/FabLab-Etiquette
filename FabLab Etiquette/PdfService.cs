using FabLab_Etiquette.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FabLab_Etiquette.Services
{
    public static class PdfService
    {
        // Couleurs laser : rouge pour la découpe, noir pour la gravure
        private static readonly XColor LaserCutColor = XColor.FromArgb(255, 255, 0, 0);   // #FF0000
        private static readonly XColor LaserEngColor = XColor.FromArgb(255, 0, 0, 0);     // #000000
        private const double CutLineWidthMm = 0.02; // épaisseur du trait de découpe en mm

        /// <summary>
        /// Génère le PDF final 600×300 mm avec uniquement rouge (découpe) et noir (gravure).
        /// Les étiquettes sont réorganisées automatiquement pour minimiser l'espace vide.
        /// </summary>
        public static void CreateLabelsPdf(IEnumerable<LabelModel> labels, string outputPath)
        {
            using var document = new PdfDocument();
            var page = document.AddPage();
            page.Width = XUnit.FromMillimeter(600);
            page.Height = XUnit.FromMillimeter(300);

            using var gfx = XGraphics.FromPdfPage(page);

            // Fond blanc
            gfx.DrawRectangle(XBrushes.White,
                new XRect(0, 0, page.Width.Point, page.Height.Point));

            // Réorganiser les étiquettes (bin-packing simple : gauche → droite, ligne par ligne)
            var arranged = ArrangeLabels(labels.ToList(),
                page.Width.Point, page.Height.Point);

            foreach (var (label, x, y) in arranged)
            {
                DrawLabelOnPdf(gfx, label, x, y);
            }

            document.Save(outputPath);
        }

        private static List<(LabelModel label, double x, double y)> ArrangeLabels(
            List<LabelModel> labels, double pageW, double pageH)
        {
            var result = new List<(LabelModel, double, double)>();
            double curX = 0, curY = 0, rowH = 0;

            foreach (var label in labels)
            {
                // Conversion pixels → points PDF (les pixels du canvas sont à 96dpi, PDF à 72dpi)
                // On utilise les dimensions directement en points (les px du canvas ont déjà le bon ratio)
                double w = XUnit.FromMillimeter(label.WidthInMillimeters).Point;
                double h = XUnit.FromMillimeter(label.HeightInMillimeters).Point;

                if (curX + w > pageW)
                {
                    curX = 0;
                    curY += rowH;
                    rowH = 0;
                }

                if (curY + h > pageH) break; // Pas assez de place

                result.Add((label, curX, curY));
                curX += w;
                rowH = Math.Max(rowH, h);
            }

            return result;
        }

        private static void DrawLabelOnPdf(XGraphics gfx, LabelModel label, double x, double y)
        {
            double w = XUnit.FromMillimeter(label.WidthInMillimeters).Point;
            double h = XUnit.FromMillimeter(label.HeightInMillimeters).Point;
            var rect = new XRect(x, y, w, h);

            // Remplissage intérieur : toujours noir (gravure)
            gfx.DrawRectangle(new XSolidBrush(LaserEngColor), rect);

            // Contour de découpe : rouge, 0.02 mm
            var cutPen = new XPen(LaserCutColor, XUnit.FromMillimeter(CutLineWidthMm).Point);

            if (label.Shape.ToLower() == "ellipse")
            {
                // Pour les ellipses, l'intérieur gravé reste un rectangle mais le contour est elliptique
                gfx.DrawEllipse(cutPen, rect);
            }
            else
            {
                // Rectangle (avec coins légèrement arrondis en prévisualisation → carré standard dans le PDF)
                gfx.DrawRectangle(cutPen, rect);
            }

            // Texte : noir sur fond noir → on dessine en blanc pour être visible sur la plaque
            // (la gravure enlève le noir de surface pour révéler la couleur en dessous)
            if (!string.IsNullOrWhiteSpace(label.Text))
            {
                var font = new XFont(label.FontFamily, label.FontSize > 0 ? label.FontSize : 8);
                gfx.DrawString(label.Text, font, XBrushes.White, rect, XStringFormats.Center);
            }
        }
    }
}
