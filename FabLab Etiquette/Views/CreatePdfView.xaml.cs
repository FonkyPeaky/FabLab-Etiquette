using FabLab_Etiquette.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public void UpdatePdfPreview(string pdfPath)
        {
            if (System.IO.File.Exists(pdfPath))
            {
                PdfPreview.Navigate(new Uri(pdfPath));
            }
            else
            {
                System.Windows.MessageBox.Show("Impossible de trouver le fichier PDF.");
            }
        }

        public void DrawLabels()
        {
            System.Diagnostics.Debug.WriteLine("Méthode DrawLabels appelée !");
            // Effacer le Canvas avant de redessiner
            PreviewCanvas.Children.Clear();

            // Récupérer le ViewModel
            var viewModel = DataContext as CreatePdfViewModel;
            if (viewModel == null || viewModel.Labels == null) return;

            foreach (var label in viewModel.Labels)
            {
                if (label == null)
                {
                    System.Diagnostics.Debug.WriteLine("Étiquette invalide détectée et ignorée.");
                    continue;
                }

                // Dessiner le fond
                var backgroundRect = new Rectangle
                {
                    Width = label.Width,
                    Height = label.Height,
                    Fill = label.BackgroundColor ?? Brushes.White,
                    StrokeThickness = 0
                };
                Canvas.SetLeft(backgroundRect, label.X);
                Canvas.SetTop(backgroundRect, label.Y);
                PreviewCanvas.Children.Add(backgroundRect);

                // Dessiner la bordure
                var borderRect = new Rectangle
                {
                    Width = label.Width,
                    Height = label.Height,
                    Stroke = Brushes.Red, // Bordure rouge pour découpe
                    StrokeThickness = label.BorderThickness,
                    Fill = Brushes.Transparent
                };
                Canvas.SetLeft(borderRect, label.X);
                Canvas.SetTop(borderRect, label.Y);
                PreviewCanvas.Children.Add(borderRect);

                // Ajouter une image si elle existe
                if (label.Image != null)
                {
                    var image = new Image
                    {
                        Source = label.Image,
                        Width = label.Width,
                        Height = label.Height,
                        Stretch = Stretch.UniformToFill
                    };
                    Canvas.SetLeft(image, label.X);
                    Canvas.SetTop(image, label.Y);
                    PreviewCanvas.Children.Add(image);
                }

                // Ajouter du texte
                var textBlock = new TextBlock
                {
                    Text = label.Text,
                    FontSize = label.FontSize,
                    Foreground = label.Action?.ToLower() == "gravure" ? Brushes.Black : Brushes.Black,
                    TextWrapping = TextWrapping.Wrap, // Autoriser le retour à la ligne
                    Width = label.Width,
                    Height = label.Height,
                    TextAlignment = TextAlignment.Center, // Alignement horizontal
                    VerticalAlignment = VerticalAlignment.Center // Alignement vertical
                };

                if (string.IsNullOrWhiteSpace(label.Text))
                {
                    System.Diagnostics.Debug.WriteLine($"Étiquette sans texte détectée et ignorée.");
                    continue; // Passe à l'étiquette suivante
                }

                // Calculer la position en fonction de l'alignement
                double textX = label.X;
                double textY = label.Y;

                switch (label.HorizontalAlignment?.ToLower())
                {
                    case "gauche":
                        textBlock.TextAlignment = TextAlignment.Left;
                        break;
                    case "droite":
                        textBlock.TextAlignment = TextAlignment.Right;
                        break;
                    default:
                        textBlock.TextAlignment = TextAlignment.Center;
                        break;
                }

                // Ajustement vertical
                switch (label.VerticalAlignment?.ToLower())
                {
                    case "haut":
                        textY = label.Y;
                        break;
                    case "bas":
                        textY = label.Y + label.Height - textBlock.FontSize;
                        break;
                    default:
                        textY = label.Y + (label.Height - textBlock.FontSize) / 2; // Centré
                        break;
                }

                Canvas.SetLeft(textBlock, textX);
                Canvas.SetTop(textBlock, textY);

                // Zone de découpe pour ne pas dépasser les limites de l'étiquette
                var clipRectangle = new RectangleGeometry(new Rect(0, 0, label.Width, label.Height));
                textBlock.Clip = clipRectangle;

                PreviewCanvas.Children.Add(textBlock);

                System.Diagnostics.Debug.WriteLine($"Ajouté au Canvas : {label.Text} à ({textX}, {textY})");
            }
        }










    }
}
