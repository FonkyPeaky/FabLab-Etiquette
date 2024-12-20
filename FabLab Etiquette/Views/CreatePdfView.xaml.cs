using FabLab_Etiquette.ViewModels;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PdfSharp.Pdf;
using System.IO.Packaging;


namespace FabLab_Etiquette.Views
{
    /// <summary>
    /// Interaction logic for CreatePdfView.xaml
    /// </summary>
    public partial class CreatePdfView : Window
    {
        public CreatePdfView()
        {
            InitializeComponent();
            DataContext = new CreatePdfViewModel();
            var viewModel = DataContext as CreatePdfViewModel;
            if (viewModel != null)
            {
                viewModel.Labels.CollectionChanged += (s, e) => DrawLabels();
            }
        }

        private void OpenPdfSettingsWindow(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new PdfSettingsWindow
            {
                DataContext = new PdfSettingsViewModel()
            };
            settingsWindow.ShowDialog();
        }


        public void DrawLabels()
        {
            System.Diagnostics.Debug.WriteLine("Méthode DrawLabels appelée !");
            PreviewCanvas.Children.Clear();

            var viewModel = DataContext as CreatePdfViewModel;
            if (viewModel == null || viewModel.Labels == null) return;

            const double scaleFactor = 1.5; // Facteur d'échelle pour prévisualisation

            foreach (var label in viewModel.Labels)
            {

                // Positions et dimensions originales (non échelonnées)
                double x = label.X;
                double y = label.Y;
                double width = label.Width;
                double height = label.Height;

                // Échelle pour la prévisualisation WPF
                double scaledX = x / scaleFactor;
                double scaledY = y / scaleFactor;
                double scaledWidth = width / scaleFactor;
                double scaledHeight = height / scaleFactor;
                double borderThickness = label.BorderThickness / scaleFactor;
                double fontSize = label.FontSize / scaleFactor;

                // Création de la forme pour le Canvas WPF
                Shape shapeElement = null;
                switch (label.Shape)
                {
                    case "Rectangle":
                        shapeElement = new Rectangle
                        {
                            Width = scaledWidth,
                            Height = scaledHeight,
                            Stroke = Brushes.Red,
                            StrokeThickness = borderThickness,
                            Fill = Brushes.White
                        };
                        break;

                    case "Ellipse":
                        shapeElement = new Ellipse
                        {
                            Width = scaledWidth,
                            Height = scaledHeight,
                            Stroke = Brushes.Red,
                            StrokeThickness = borderThickness,
                            Fill = Brushes.White
                        };
                        break;

                    case "Losange":
                        var losange = new Polygon
                        {
                            Points = new PointCollection
                    {
                        new Point(scaledWidth / 2, 0),
                        new Point(scaledWidth, scaledHeight / 2),
                        new Point(scaledWidth / 2, scaledHeight),
                        new Point(0, scaledHeight / 2)
                    },
                            Stroke = Brushes.Red,
                            StrokeThickness = borderThickness,
                            Fill = Brushes.White
                        };
                        shapeElement = losange;
                        break;
                }

                // Positionner la forme dans le Canvas
                if (shapeElement != null)
                {
                    Canvas.SetLeft(shapeElement, scaledX);
                    Canvas.SetTop(shapeElement, scaledY);
                    PreviewCanvas.Children.Add(shapeElement);
                }

                // Ajouter le texte
                var textBlock = new TextBlock
                {
                    Text = label.Text,
                    FontSize = fontSize,
                    Foreground = Brushes.Black,
                    Width = scaledWidth,
                    Height = scaledHeight,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                Canvas.SetLeft(textBlock, scaledX);
                Canvas.SetTop(textBlock, scaledY + (scaledHeight - fontSize) / 2);
                PreviewCanvas.Children.Add(textBlock);

                // --- Dessin dans le PDF ---
                var gfx = FabLab_Etiquette.Services.PdfService.GetGraphics();

                // Conversion des unités de pixels en points (1 pixel = 0.75 point)
                double unitConversion = 0.75;
                double pdfX = x * unitConversion;
                double pdfY = y * unitConversion;
                double pdfWidth = width * unitConversion;
                double pdfHeight = height * unitConversion;

                // Création des objets pour PDFsharp
                var pen = new XPen(XColors.Red, label.BorderThickness);
                var brush = XBrushes.White;
                var font = new XFont(label.FontFamily, label.FontSize);
                var textBrush = XBrushes.Black;

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

                // Dessiner le texte dans le PDF
                var layoutRect = new XRect(pdfX, pdfY, pdfWidth, pdfHeight);
                var format = new XStringFormat
                {
                    Alignment = XStringAlignment.Center,
                    LineAlignment = XLineAlignment.Center
                };
                gfx.DrawString(label.Text, font, textBrush, layoutRect, format);
            }
        }




    }
}
