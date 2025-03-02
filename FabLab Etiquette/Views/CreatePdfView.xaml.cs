using FabLab_Etiquette.Models;
using FabLab_Etiquette.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection.Emit;


namespace FabLab_Etiquette.Views
{
    /// <summary>
    /// Interaction logic for CreatePdfView.xaml
    /// </summary>
    /// 
    public partial class CreatePdfView : Window
    {
        private List<LabelModel> labelList = new List<LabelModel>();
        public ObservableCollection<LabelModel> LabelList { get; set; } = new ObservableCollection<LabelModel>();
        private CreatePdfViewModel ViewModel => DataContext as CreatePdfViewModel;

        public CreatePdfView()
        {
            InitializeComponent();
            DataContext = new CreatePdfViewModel();
            var viewModel = DataContext as CreatePdfViewModel;
            if (viewModel != null)
            {
                viewModel.Labels.CollectionChanged += (s, e) => DrawLabels();
            }
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Width" || e.PropertyName == "Height")
                {
                    OnLabelResized(s, e);
                }
            };
            if (LabelListView == null)
            {
                Debug.WriteLine("⚠️ LabelListView n'est pas trouvé dans XAML !");
            }
        }

        public ListBox GetLabelListView()
        {
            return LabelListView;
        }

        private void OpenPdfSettingsWindow(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new PdfSettingsWindow
            {
                DataContext = new PdfSettingsViewModel()
            };
            settingsWindow.ShowDialog();
        }

        private void OnAddImageClicked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var viewModel = DataContext as CreatePdfViewModel;
                if (viewModel != null)
                {
                    viewModel.ImageSource = openFileDialog.FileName;
                    DrawLabels(); // Rafraîchir le Canvas
                }
            }
        }

        private void OnPrintPdfClicked(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as CreatePdfViewModel;
            if (viewModel == null || string.IsNullOrWhiteSpace(viewModel.SelectedFilePath))
            {
                MessageBox.Show("Aucun fichier PDF sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine("ERREUR : SelectedFilePath est vide !");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Tentative d'impression du fichier : {viewModel.SelectedFilePath}");

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = viewModel.SelectedFilePath,
                        Verb = "print",
                        UseShellExecute = true
                    });

                    MessageBox.Show("Impression envoyée !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'impression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void DrawLabels()
        {
            UIElement shape = null;
            System.Diagnostics.Debug.WriteLine("Méthode DrawLabels appelée !");
            PreviewCanvas.Children.Clear();

            var viewModel = DataContext as CreatePdfViewModel;
            if (viewModel == null || viewModel.Labels == null) return;

            // Calculer l'échelle entre le Canva (pixels) et le PDF (millimètres)
            double scaleX = 1237.0 / 2267.0; // Canva width / PDF width (mm)
            double scaleY = 666.0 / 1133.0;  // Canva height / PDF height (mm)
            double scaleFactor = Math.Min(scaleX, scaleY); // Utiliser le facteur d'échelle le plus petit

            foreach (var label in viewModel.Labels)
            {
                // Création des formes standards (Rectangle, Ellipse, etc.)
                Shape shapeElement;
                switch (label.Shape.ToLower())
                {
                    case "rectangle":
                        shapeElement = new Rectangle
                        {
                            Width = label.Width,
                            Height = label.Height,
                            Stroke = label.BorderColor,
                            StrokeThickness = label.BorderThickness,
                            Fill = label.BackgroundColor
                        };
                        break;

                    case "ellipse":
                        shapeElement = new Ellipse
                        {
                            Width = label.Width,
                            Height = label.Height,
                            Stroke = label.BorderColor,
                            StrokeThickness = label.BorderThickness,
                            Fill = label.BackgroundColor
                        };
                        break;

                    default:
                        shapeElement = new Rectangle
                        {
                            Width = label.Width,
                            Height = label.Height,
                            Stroke = label.BorderColor,
                            StrokeThickness = label.BorderThickness,
                            Fill = label.BackgroundColor
                        };
                        break;
                }

                Canvas.SetLeft(shapeElement, label.X);
                Canvas.SetTop(shapeElement, label.Y);
                PreviewCanvas.Children.Add(shapeElement);

                // 📌 Vérifier si une image est associée à l’étiquette
                // 📌 Vérifier si une image est associée à l’étiquette
                if (!string.IsNullOrWhiteSpace(label.ImageSource))
                {
                    Image img = new Image
                    {
                        Source = new BitmapImage(new Uri(label.ImageSource, UriKind.Absolute)),
                        Stretch = Stretch.Fill, // ✅ Permet d'étirer l'image pour remplir l'étiquette
                        Width = label.Width,
                        Height = label.Height
                    };

                    // 📌 Associer l'image à l'étiquette et l'afficher
                    Canvas.SetLeft(img, label.X);
                    Canvas.SetTop(img, label.Y);
                    PreviewCanvas.Children.Add(img);

                    System.Diagnostics.Debug.WriteLine($"📷 Image ajoutée à l’étiquette avec dimension : {label.Width}x{label.Height}");
                }


                // 📌 Ajouter le texte au centre de l’étiquette
                var textBlock = new TextBlock
                {
                    Text = label.Text,
                    FontSize = label.FontSize,
                    Foreground = Brushes.Black,
                    Width = label.Width,
                    Height = label.Height,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                // Centrer le texte verticalement
                Canvas.SetLeft(textBlock, label.X);
                Canvas.SetTop(textBlock, label.Y + (label.Height - label.FontSize) / 2);
                PreviewCanvas.Children.Add(textBlock);

            }
            PreviewCanvas.UpdateLayout();

        }
        private void OnLabelResized(object sender, EventArgs e)
        {
            DrawLabels(); // Redessine toutes les étiquettes avec leurs nouvelles tailles
        }
        private void OnAddImageToLabel(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var viewModel = DataContext as CreatePdfViewModel;
                if (viewModel != null && viewModel.SelectedLabel != null)
                {
                    viewModel.SelectedLabel.ImageSource = openFileDialog.FileName;
                    DrawLabels(); // Rafraîchir le Canvas
                }
                else
                {
                    MessageBox.Show("Sélectionnez une étiquette avant d’ajouter une image.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private void OnBackgroundColorChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string colorHex = selectedItem.Tag.ToString(); // Récupérer la couleur en hexadécimal
                var viewModel = DataContext as CreatePdfViewModel;

                if (viewModel != null && viewModel.SelectedLabel != null)
                {
                    // 📌 Appliquer la couleur de fond
                    viewModel.SelectedLabel.BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));

                    // 📌 Déterminer la couleur du contour (rouge si découpe, noir si gravure)
                    if (selectedItem.Content.ToString().Contains("Sécurité")) // ⚠ Sécurité (Rouge sur Jaune)
                    {
                        viewModel.SelectedLabel.BorderColor = Brushes.Red;
                    }
                    else if (selectedItem.Content.ToString().Contains("Standard")) // 📄 Standard (Noir sur Blanc)
                    {
                        viewModel.SelectedLabel.BorderColor = Brushes.Black;
                    }
                    else if (selectedItem.Content.ToString().Contains("Information")) // 🔵 Information (Bleu sur Blanc)
                    {
                        viewModel.SelectedLabel.BorderColor = Brushes.Blue;
                    }
                    else if (selectedItem.Content.ToString().Contains("Autorisation")) // ✅ Autorisation (Vert sur Blanc)
                    {
                        viewModel.SelectedLabel.BorderColor = Brushes.Green;
                    }
                    else if (selectedItem.Content.ToString().Contains("Marquage spécifique")) // ⚫ Marquage spécifique (Blanc sur Noir)
                    {
                        viewModel.SelectedLabel.BorderColor = Brushes.White;
                    }
                    else if (selectedItem.Content.ToString().Contains("Attention")) // 🚨 Attention (Rouge sur Blanc)
                    {
                        viewModel.SelectedLabel.BorderColor = Brushes.Red;
                    }
                }
            }
        }
        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Synchronisation pour éviter que LabelList soit vide
                ViewModel.SyncLabelList();

                Debug.WriteLine($"🔍 Vérification juste avant sauvegarde: LabelList contient {ViewModel.LabelList.Count} étiquettes.");

                if (ViewModel.LabelList == null || !ViewModel.LabelList.Any())
                {
                    MessageBox.Show("Aucune étiquette à sauvegarder !");
                    return;
                }

                Debug.WriteLine("📌 Début du processus de sauvegarde");

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Enregistrer le projet",
                    Filter = "Fichiers JSON (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = ViewModel.GenerateStandardJsonFileName()
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    Debug.WriteLine("❌ Annulation de la sélection du fichier.");
                    return;
                }

                try
                {
                    string jsonData = System.Text.Json.JsonSerializer.Serialize(ViewModel.LabelList, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    System.IO.File.WriteAllText(saveFileDialog.FileName, jsonData);
                    MessageBox.Show("Projet sauvegardé avec succès !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}");
                }

                Debug.WriteLine($"📌 Fichier sélectionné : {saveFileDialog.FileName}");
            }
            catch (Exception generalEx)
            {
                MessageBox.Show($"Erreur inattendue : {generalEx.Message}");
            }
        }

        private void LoadProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Fichier JSON (*.json)|*.json",
                    Title = "Charger un projet"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string json = File.ReadAllText(openFileDialog.FileName);

                    // Désérialisation en liste d'étiquettes
                    var labels = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<LabelModel>>(json);

                    if (labels != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // 1️⃣ Récupération de la vue et désactivation temporaire du binding
                            var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                            if (view?.GetLabelListView() != null)
                            {
                                view.GetLabelListView().ItemsSource = null; // Désactive temporairement le binding
                            }

                            // 2️⃣ Mise à jour sécurisée des collections
                            ViewModel.Labels.Clear();
                            foreach (var label in labels)
                            {
                                ViewModel.Labels.Add(label);
                            }

                            ViewModel.SyncLabelList(); // Synchroniser les collections

                            // 3️⃣ Réactivation du binding après la mise à jour
                            if (view?.LabelListView != null)
                            {
                                view.LabelListView.ItemsSource = ViewModel.Labels;
                            }
                        });

                        MessageBox.Show("Projet chargé avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ce fichier ne contient aucune étiquette valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
