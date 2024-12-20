using FabLab_Etiquette.ViewModels;
using PdfSharp.Drawing;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


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

            // Calculer l'échelle entre le Canva (pixels) et le PDF (millimètres)
            double scaleX = 1110.0 / 2267.0; // Canva width / PDF width (mm)
            double scaleY = 619.0 / 1133.0;  // Canva height / PDF height (mm)
            double scaleFactor = Math.Min(scaleX, scaleY); // Utiliser le facteur d'échelle le plus petit

            foreach (var label in viewModel.Labels)
            {
                // Appliquer l'échelle pour le Canva
                double x = label.X * scaleFactor;
                double y = label.Y * scaleFactor;
                double width = label.Width * scaleFactor;
                double height = label.Height * scaleFactor;
                double borderThickness = label.BorderThickness * scaleFactor;
                double fontSize = label.FontSize * scaleFactor;

                // Dessiner la forme selon son type
                Shape shapeElement;
                switch (label.Shape.ToLower())
                {
                    case "rectangle":
                        shapeElement = new Rectangle
                        {
                            Width = width,
                            Height = height,
                            Stroke = Brushes.Red,
                            StrokeThickness = borderThickness,
                            Fill = Brushes.White // Fond blanc
                        };
                        break;

                    case "ellipse":
                        shapeElement = new Ellipse
                        {
                            Width = width,
                            Height = height,
                            Stroke = Brushes.Red,
                            StrokeThickness = borderThickness,
                            Fill = Brushes.White // Fond blanc
                        };
                        break;

                    case "losange":
                        var losange = new Polygon
                        {
                            Points = new PointCollection
                    {
                        new Point(width / 2, 0),        // Haut
                        new Point(width, height / 2),  // Droite
                        new Point(width / 2, height),  // Bas
                        new Point(0, height / 2)       // Gauche
                    },
                            Stroke = Brushes.Red,
                            StrokeThickness = borderThickness,
                            Fill = Brushes.White // Fond blanc
                        };
                        shapeElement = losange;
                        break;

                    default:
                        shapeElement = new Rectangle
                        {
                            Width = width,
                            Height = height,
                            Stroke = Brushes.Red,
                            StrokeThickness = borderThickness,
                            Fill = Brushes.White // Fond blanc
                        };
                        break;
                }

                // Positionner la forme dans le Canva
                Canvas.SetLeft(shapeElement, x);
                Canvas.SetTop(shapeElement, y);
                PreviewCanvas.Children.Add(shapeElement);

                // Dessiner le texte centré dans l'étiquette
                var textBlock = new TextBlock
                {
                    Text = label.Text,
                    FontSize = fontSize,
                    Foreground = label.Action?.ToLower() == "gravure" ? Brushes.Black : Brushes.Black,
                    Width = width,
                    Height = height,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                // Centrage vertical manuel pour le texte
                double textY = y + (height - fontSize) / 2;
                Canvas.SetLeft(textBlock, x);
                Canvas.SetTop(textBlock, textY);
                PreviewCanvas.Children.Add(textBlock);

                System.Diagnostics.Debug.WriteLine($"Ajouté au Canva : {label.Text} à ({x}, {y})");
            }
        }
    }
}
