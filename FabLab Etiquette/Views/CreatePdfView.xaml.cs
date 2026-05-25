using FabLab_Etiquette.Models;
using FabLab_Etiquette.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FabLab_Etiquette.Views
{
    public partial class CreatePdfView : Window
    {
        public ObservableCollection<LabelModel> LabelList { get; set; } = new();
        private CreatePdfViewModel ViewModel => (CreatePdfViewModel)DataContext;

        public CreatePdfView(UserInfo userInfo, FabricationParams fabParams)
        {
            InitializeComponent();
            var viewModel = new CreatePdfViewModel(userInfo, fabParams);
            DataContext = viewModel;

            viewModel.Labels.CollectionChanged += (s, e) => DrawLabels();
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(viewModel.SelectedLabel) or "Width" or "Height")
                    DrawLabels();
            };

            // Afficher le résumé des paramètres collectés
            UpdateStatusBar(userInfo, fabParams);
        }

        private void UpdateStatusBar(UserInfo userInfo, FabricationParams fabParams)
        {
            if (StatusBarText != null)
                StatusBarText.Text =
                    $"Demandeur : {userInfo.Name.ToUpper()} | Service : {userInfo.Service.ToUpper()} | " +
                    $"Style : {fabParams.SelectedStyle?.DisplayName} | Couleur : {fabParams.SelectedColor} | " +
                    $"Titre : {fabParams.LabelTitle} | Impressions : x{fabParams.PrintCount}";
        }

        public ListBox GetLabelListView() => LabelListView;

        private void OnLabelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel?.SetSelectedLabels(LabelListView.SelectedItems);
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
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ViewModel.ImageSource = openFileDialog.FileName;
                DrawLabels();
            }
        }

        private void OnPrintPdfClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ViewModel.SelectedFilePath))
            {
                MessageBox.Show("Générez d'abord un PDF avant d'imprimer.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = ViewModel.SelectedFilePath,
                        Verb = "print",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'impression : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void DrawLabels()
        {
            PreviewCanvas.Children.Clear();

            if (ViewModel?.Labels == null) return;

            // Dessiner la grille si activée
            if (ViewModel.IsGridVisible)
                DrawGrid();

            foreach (var label in ViewModel.Labels)
            {
                bool isSelected = ViewModel.SelectedLabels.Contains(label) || label == ViewModel.SelectedLabel;

                // Forme principale
                Shape shapeElement = label.Shape.ToLower() switch
                {
                    "ellipse" => new Ellipse
                    {
                        Width = label.Width, Height = label.Height,
                        Stroke = label.BorderColor,
                        StrokeThickness = isSelected ? label.BorderThickness + 2 : label.BorderThickness,
                        Fill = label.BackgroundColor
                    },
                    _ => new Rectangle
                    {
                        Width = label.Width, Height = label.Height,
                        Stroke = isSelected ? Brushes.Blue : label.BorderColor,
                        StrokeThickness = isSelected ? label.BorderThickness + 2 : label.BorderThickness,
                        Fill = label.BackgroundColor,
                        RadiusX = 4, RadiusY = 4  // Coins légèrement arrondis
                    }
                };

                Canvas.SetLeft(shapeElement, label.X);
                Canvas.SetTop(shapeElement, label.Y);
                PreviewCanvas.Children.Add(shapeElement);

                // Image associée à l'étiquette
                if (!string.IsNullOrWhiteSpace(label.ImageSource))
                {
                    try
                    {
                        var img = new Image
                        {
                            Source = new BitmapImage(new Uri(label.ImageSource, UriKind.Absolute)),
                            Stretch = Stretch.Uniform,
                            Width = label.Width - 4,
                            Height = label.Height - 4
                        };
                        Canvas.SetLeft(img, label.X + 2);
                        Canvas.SetTop(img, label.Y + 2);
                        PreviewCanvas.Children.Add(img);
                    }
                    catch { /* image invalide, on ignore */ }
                }

                // Texte centré — blanc sur fond sombre, noir sur fond clair
                bool isDarkBg = IsColorDark(label.BackgroundColorHex);
                var textBlock = new TextBlock
                {
                    Text = label.Text,
                    FontSize = label.FontSize,
                    FontFamily = new FontFamily(label.FontFamily),
                    Foreground = isDarkBg ? Brushes.White : Brushes.Black,
                    Width = label.Width,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Canvas.SetLeft(textBlock, label.X);
                Canvas.SetTop(textBlock, label.Y + (label.Height - label.FontSize * 1.4) / 2);
                PreviewCanvas.Children.Add(textBlock);
            }

            PreviewCanvas.UpdateLayout();
        }

        private static bool IsColorDark(string hex)
        {
            try
            {
                var c = (Color)ColorConverter.ConvertFromString(hex);
                double luminance = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
                return luminance < 128;
            }
            catch { return false; }
        }

        private void DrawGrid()
        {
            const double gridSpacing = 50;
            var pen = new System.Windows.Media.Pen(Brushes.LightGray, 0.5);

            for (double x = 0; x <= PreviewCanvas.Width; x += gridSpacing)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = x, Y1 = 0, X2 = x, Y2 = PreviewCanvas.Height,
                    Stroke = Brushes.LightGray, StrokeThickness = 0.5
                };
                PreviewCanvas.Children.Add(line);
            }

            for (double y = 0; y <= PreviewCanvas.Height; y += gridSpacing)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 0, Y1 = y, X2 = PreviewCanvas.Width, Y2 = y,
                    Stroke = Brushes.LightGray, StrokeThickness = 0.5
                };
                PreviewCanvas.Children.Add(line);
            }
        }

        private void OnBackgroundColorChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is ComboBoxItem selectedItem && ViewModel?.SelectedLabel != null)
            {
                string colorHex = selectedItem.Tag.ToString()!;
                ViewModel.SelectedLabel.BackgroundColor =
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            }
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.SyncLabelList();

                if (!ViewModel.LabelList.Any())
                {
                    MessageBox.Show("Aucune étiquette à sauvegarder.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Enregistrer le projet",
                    Filter = "Fichiers JSON (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = ViewModel.GenerateStandardJsonFileName()
                };

                if (saveFileDialog.ShowDialog() != true) return;

                string jsonData = System.Text.Json.JsonSerializer.Serialize(ViewModel.LabelList,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFileDialog.FileName, jsonData);
                MessageBox.Show("Projet sauvegardé avec succès !", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Fichier JSON (*.json)|*.json",
                    Title = "Charger un projet"
                };

                if (openFileDialog.ShowDialog() != true) return;

                string json = File.ReadAllText(openFileDialog.FileName);
                var labels = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<LabelModel>>(json);

                if (labels == null || !labels.Any())
                {
                    MessageBox.Show("Ce fichier ne contient aucune étiquette valide.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LabelListView.ItemsSource = null;
                    ViewModel.Labels.Clear();
                    foreach (var label in labels)
                        ViewModel.Labels.Add(label);
                    ViewModel.SyncLabelList();
                    LabelListView.ItemsSource = ViewModel.Labels;
                });

                MessageBox.Show("Projet chargé avec succès !", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteLabel_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLabel != null)
            {
                ViewModel.Labels.Remove(ViewModel.SelectedLabel);
                ViewModel.SelectedLabel = ViewModel.Labels.LastOrDefault();
            }
        }

        private void DuplicateLabel_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLabel is LabelModel src)
            {
                var copy = new LabelModel
                {
                    X = src.X + src.Width,
                    Y = src.Y,
                    Width = src.Width,
                    Height = src.Height,
                    Text = src.Text,
                    FontFamily = src.FontFamily,
                    FontSize = src.FontSize,
                    BackgroundColor = src.BackgroundColor,
                    BorderColor = src.BorderColor,
                    BorderThickness = src.BorderThickness,
                    Shape = src.Shape,
                    Action = src.Action,
                    ImageSource = src.ImageSource
                };
                ViewModel.Labels.Add(copy);
                ViewModel.SelectedLabel = copy;
            }
        }
    }
}
