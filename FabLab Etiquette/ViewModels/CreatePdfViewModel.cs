using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Models;
using FabLab_Etiquette.Views;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FabLab_Etiquette.ViewModels
{
    public class CreatePdfViewModel : INotifyPropertyChanged
    {
        public ICommand AddLabelCommand { get; }
        public ICommand GeneratePdfCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand UpdateSelectedLabelCommand { get; }
        public ICommand RearrangeLabelsCommand { get; }
        public ICommand AlignLabelsCommand { get; }
        public ICommand AddNewLineCommand { get; }
        public ICommand ToggleGridCommand { get; }
        public ObservableCollection<LabelModel> Labels { get; } = new ObservableCollection<LabelModel>();
        public event PropertyChangedEventHandler PropertyChanged;
        private LabelModel _selectedLabel;
        private bool _isGridVisible;
        public string _pdfPath = @"C:\Temp\Etiquettes.pdf"; // Chemin du fichier PDF


        public bool IsGridVisible
        {
            get => _isGridVisible;
            set
            {
                _isGridVisible = value;
                OnPropertyChanged();

                // Actualiser la prévisualisation
                var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                view?.DrawLabels(); // Redessine avec ou sans grille
            }
        }

        public LabelModel SelectedLabel
        {
            get => _selectedLabel;
            set
            {
                _selectedLabel = value;
                OnPropertyChanged();

                // Actualiser l'affichage lorsque l'étiquette sélectionnée change
                var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                view?.DrawLabels();
            }
        }
        private void AddNewLine()
        {

            if (SelectedLabel != null)
            {
                SelectedLabel.Text += "\n"; // Ajouter un retour à la ligne
                OnPropertyChanged(nameof(SelectedLabel)); // Notifie l'interface utilisateur
            }
        }
        private void Label_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Actualiser après modification d'une propriété
            var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
            view?.DrawLabels();
        }


        public CreatePdfViewModel()
        {
            Labels = new ObservableCollection<LabelModel>();
            AddLabelCommand = new RelayCommand(AddLabel);
            GeneratePdfCommand = new RelayCommand(GeneratePdf);
            UpdateSelectedLabelCommand = new RelayCommand(UpdateSelectedLabel);
            RearrangeLabelsCommand = new RelayCommand(RearrangeLabels);
            AddImageCommand = new RelayCommand(AddImageToLabel);
            AlignLabelsCommand = new RelayCommand(AlignLabels);
            AddNewLineCommand = new RelayCommand(AddNewLine);
            ToggleGridCommand = new RelayCommand(() =>
            {
                IsGridVisible = !IsGridVisible; // Alterne l'état
            });


            Labels.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (LabelModel label in e.NewItems)
                    {
                        label.PropertyChanged += Label_PropertyChanged;
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (LabelModel label in e.OldItems)
                    {
                        label.PropertyChanged -= Label_PropertyChanged;
                    }
                }

                // Actualiser après modification de la collection
                var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                view?.DrawLabels();
            };

        }
        public void AddImageToLabel()
        {
            if (SelectedLabel == null)
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner une étiquette avant d'ajouter une image.");
                return;
            }

            System.Windows.MessageBox.Show("La commande AddImageCommand a bien été appelée !");

            // Ouvrir une boîte de dialogue pour sélectionner une image
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Sélectionnez une image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (SelectedLabel == null)
                {
                    System.Windows.MessageBox.Show("Aucune étiquette sélectionnée !");
                    return;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"SelectedLabel : Texte={SelectedLabel.Text}, X={SelectedLabel.X}, Y={SelectedLabel.Y}");
                }

                string imagePath = openFileDialog.FileName;
                SelectedLabel.Image = imagePath;

                // Mettre à jour la prévisualisation
                var image = new BitmapImage(new Uri(imagePath));

                // Mettre à jour la prévisualisation via la méthode existante
                var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                view?.DrawLabels();

                System.Windows.MessageBox.Show("Image ajoutée avec succès !");
            }
        }

        private void RearrangeLabels()
        {
            const double step = 0; // Pas entre les étiquettes
            double currentX = 0, currentY = 0;
            const double maxWidth = 844, maxHeight = 500;

            foreach (var label in Labels)
            {
                label.X = currentX;
                label.Y = currentY;

                System.Diagnostics.Debug.WriteLine($"Positionnement : {label.Text} à ({label.X}, {label.Y})");

                currentX += label.Width + step;
                if (currentX + label.Width > maxWidth)
                {
                    currentX = 0;
                    currentY += label.Height + step;
                }

                if (currentY + label.Height > maxHeight)
                {
                    System.Windows.MessageBox.Show("Toutes les étiquettes ne peuvent pas tenir sur le plateau. Réduisez leur taille ou leur nombre.");
                    break;
                }
            }
        }


        private void UpdateSelectedLabel()
        {
            if (SelectedLabel != null)
            {
                OnPropertyChanged(nameof(SelectedLabel)); // Notifie l'interface utilisateur
                System.Windows.MessageBox.Show("Étiquette mise à jour !");
            }
        }

        private void AddLabel()
        {
            const double scaleFactor = 1.5; // Facteur d'échelle pour la prévisualisation
            const double canvasWidth = 1110; // Largeur visible à l'écran
            const double canvasHeight = 619; // Hauteur visible à l'écran
            const double realWidth = canvasWidth * scaleFactor; // Largeur réelle
            const double realHeight = canvasHeight * scaleFactor; // Hauteur réelle

            LabelModel newLabel;

            if (Labels.Count > 0)
            {
                var lastLabel = Labels.Last();
                newLabel = new LabelModel
                {
                    X = lastLabel.X, // Même colonne que la dernière étiquette
                    Y = lastLabel.Y + lastLabel.Height, // Position en dessous
                    Width = lastLabel.Width,
                    Height = lastLabel.Height,
                    Text = lastLabel.Text,
                    FontFamily = lastLabel.FontFamily,
                    FontSize = lastLabel.FontSize,
                    BackgroundColor = lastLabel.BackgroundColor,
                    BorderColor = lastLabel.BorderColor,
                    BorderThickness = lastLabel.BorderThickness,
                    Shape = lastLabel.Shape
                };

                // Détection des dépassements en coordonnées réelles
                if (newLabel.Y + newLabel.Height > realHeight)
                {
                    newLabel.Y = 0;
                    newLabel.X += newLabel.Width;
                }

                if (newLabel.X + newLabel.Width > realWidth)
                {
                    MessageBox.Show("Impossible d'ajouter plus d'étiquettes : dépassement des limites du PDF.");
                    return;
                }
            }
            else
            {
                // Valeurs par défaut pour la première étiquette
                newLabel = new LabelModel
                {
                    X = 0,
                    Y = 0,
                    Width = 100,
                    Height = 50,
                    Text = "Nouvelle étiquette",
                    FontFamily = "Arial",
                    FontSize = 10,
                    BackgroundColor = Brushes.White,
                    BorderColor = Brushes.Red,
                    BorderThickness = 2,
                    Shape = "Rectangle"
                };
            }

            // Ajouter l'étiquette
            Labels.Add(newLabel);
            SelectedLabel = newLabel;

            // Mettre à jour l'affichage
            var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
            view?.DrawLabels();
        }

        public void GeneratePdf()
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var pen = new XPen(XColors.Red, 2);
            gfx.DrawRectangle(pen, XBrushes.Transparent, 50, 50, 100, 50);

            /* var Labels = new List<LabelModel>
             {
                 new LabelModel
                 {
                     X = 10,
                     Y = 20,
                     Width = 100,
                     Height = 50,
                     Shape = "Rectangle",
                     Text = "Exemple",
                     Action = "Gravure",
                     FontFamily = "Arial",
                     FontSize = 12,
                     BorderThickness = 1
                 }
             };*/

            if (Labels.Count == 0)
            {
                System.Windows.MessageBox.Show("Aucune étiquette à générer !");
                return;
            }

            // Convertir en ObservableCollection pour correspondre à CreateLabelsPdf
            var observableLabels = new ObservableCollection<LabelModel>(Labels);

            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string projectRootPath = Path.GetFullPath(Path.Combine(exePath, @"..\..\..\.."));
            string fileName = $"Etiquette_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string outputPath = Path.Combine(projectRootPath, fileName);

            // Appeler la méthode du service
            FabLab_Etiquette.Services.PdfService.CreateLabelsPdf(observableLabels, outputPath);

            MessageBox.Show($"PDF généré avec succès : {outputPath}",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsOverlapping(LabelModel newLabel)
        {
            foreach (var label in Labels)
            {
                if (label == newLabel) continue; // Ignorer l'étiquette elle-même

                bool overlapsHorizontally = newLabel.X < label.X + label.Width && newLabel.X + newLabel.Width > label.X;
                bool overlapsVertically = newLabel.Y < label.Y + label.Height && newLabel.Y + newLabel.Height > label.Y;

                if (overlapsHorizontally && overlapsVertically)
                {
                    return true; // Il y a un chevauchement
                }
            }
            return false;
        }
        private void FindFreePosition(LabelModel newLabel)
        {
            const double step = 1;
            double maxWidth = 1701; // Taille PDF en points (600 mm)
            double maxHeight = 850; // Taille PDF en points (300 mm)

            while (IsOverlapping(newLabel))
            {
                newLabel.Y += step; // Descendre à la prochaine ligne

                if (newLabel.Y + newLabel.Height > maxHeight)
                {
                    newLabel.Y = 0;
                    newLabel.X += newLabel.Width + step; // Déplacer vers la droite
                }

                if (newLabel.X + newLabel.Width > maxWidth)
                {
                    System.Windows.MessageBox.Show("Impossible d'ajouter plus d'étiquettes : espace insuffisant.");
                    break;
                }
            }
        }

        private void AlignLabels()
        {
            const double padding = 0; // Espace entre les étiquettes
            const double canvasWidth = 600; // Largeur de la zone d'affichage
            const double canvasHeight = 300; // Hauteur de la zone d'affichage

            double currentX = padding;
            double currentY = padding;

            foreach (var label in Labels)
            {
                // Vérifier si l'étiquette dépasse la largeur du Canvas
                if (currentX + label.Width > canvasWidth)
                {
                    currentX = padding; // Revenir à gauche
                    currentY += label.Height + padding; // Passer à la ligne suivante
                }

                // Vérifier si l'étiquette dépasse la hauteur du Canvas
                if (currentY + label.Height > canvasHeight)
                {
                    System.Windows.MessageBox.Show("Certaines étiquettes dépassent les limites de la zone d'affichage.");
                    break;
                }

                // Positionner l'étiquette
                label.X = currentX;
                label.Y = currentY;

                // Décaler vers la droite pour la prochaine étiquette
                currentX += label.Width + padding;
            }

            // Demander une mise à jour du Canvas
            var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
            view?.DrawLabels();
        }
    }
}

