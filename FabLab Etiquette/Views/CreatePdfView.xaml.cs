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
        public void DrawLabels()
        {
            System.Diagnostics.Debug.WriteLine("Méthode DrawLabels appelée !");
            // Effacer le Canvas avant de redessiner
            PreviewCanvas.Children.Clear();

            // Récupérer le ViewModel
            var viewModel = DataContext as CreatePdfViewModel;
            if (viewModel == null) return;

            foreach (var label in viewModel.Labels)
            {
                // Dessiner le fond
                var backgroundRect = new Rectangle
                {
                    Width = label.Width,
                    Height = label.Height,
                    Fill = label.BackgroundColor,
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
                    Stroke = label.BorderColor,
                    StrokeThickness = label.BorderThickness,
                    Fill = Brushes.Transparent
                };
                Canvas.SetLeft(borderRect, label.X);
                Canvas.SetTop(borderRect, label.Y);
                PreviewCanvas.Children.Add(borderRect);

                // Créer un rectangle pour représenter l'étiquette
                var rectangle = new Rectangle
                {
                    Width = label.Width,
                    Height = label.Height,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };

                // Positionner le rectangle
                Canvas.SetLeft(rectangle, label.X);
                Canvas.SetTop(rectangle, label.Y);
                PreviewCanvas.Children.Add(rectangle);

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

                // Validation de la taille de la police
                if (label.FontSize <= 0)
                {
                    label.FontSize = 1; // Définit une taille minimale de police
                }

                if (label.FontSize > label.Height / 2)
                {
                    label.FontSize = label.Height / 2; // Réduire la taille du texte pour qu'il reste visible
                }


                // Ajouter du texte
                var textBlock = new TextBlock
                {
                    Text = label.Text,
                    Foreground = Brushes.Black,
                    FontSize = label.FontSize,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.NoWrap, // Pas de retour à la ligne
                    ClipToBounds = true // Le texte ne dépasse pas les limites
                };

                if (label == null || string.IsNullOrWhiteSpace(label.Text))
                {
                    System.Diagnostics.Debug.WriteLine("Étiquette non valide ou sans texte.");
                    continue; // Passez à l'étiquette suivante si elle est invalide
                }

                System.Diagnostics.Debug.WriteLine($"Dessiner l'étiquette : {label.Text} à la position ({label.X}, {label.Y})");

                // Calculer la position pour centrer le texte dans la case
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double textWidth = textBlock.DesiredSize.Width;
                double textHeight = textBlock.DesiredSize.Height;

                double textX = Math.Max(label.X, label.X + (label.Width - textWidth) / 2);
                double textY = label.Y + (label.Height - textHeight) / 2;

                Canvas.SetLeft(textBlock, textX);
                Canvas.SetTop(textBlock, textY);

                // Ajouter une zone de découpe pour empêcher le texte de dépasser la case
                var clipRectangle = new RectangleGeometry(new Rect(label.X, label.Y, label.Width, label.Height));
                textBlock.Clip = clipRectangle;

                PreviewCanvas.Children.Add(textBlock);
                System.Diagnostics.Debug.WriteLine($"Ajouté au Canvas : {label.Text}");
            }
            
        }
    }
}
