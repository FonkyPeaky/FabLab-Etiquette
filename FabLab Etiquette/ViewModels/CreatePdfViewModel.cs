using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Services;
using Microsoft.Win32;
using System.Windows.Input;
using FabLab_Etiquette.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using FabLab_Etiquette.Views;
using System.Linq;

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
        public ObservableCollection<LabelModel> Labels { get; } = new ObservableCollection<LabelModel>();
        public event PropertyChangedEventHandler PropertyChanged;
        private LabelModel _selectedLabel;

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

                BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName));
                SelectedLabel.Image = image;
                // Mettre à jour la prévisualisation
                // Mettre à jour la prévisualisation
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
            var newLabel = new LabelModel
            {
                X = 0, // Position initiale
                Y = 0,
                Width = 100,
                Height = 50,
                Text = "Nouvelle étiquette",
                FontFamily = "Arial",
                FontSize = 10
            };

            // Trouver une position libre
            FindFreePosition(newLabel);

            Labels.Add(newLabel);
            SelectedLabel = newLabel; // Sélectionner automatiquement la nouvelle étiquette
        }



        private void GeneratePdf()
        {

            System.Diagnostics.Debug.WriteLine("test 1");
            if (Labels.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("test 2");
                System.Windows.MessageBox.Show("Aucune étiquette à générer !");
                return;
            }
            System.Diagnostics.Debug.WriteLine("test 3");
            string outputPath = @"C:\Temp\Etiquettes.pdf"; // Chemin temporaire
            PdfService.CreateLabelsPdf(Labels, outputPath);
            System.Windows.MessageBox.Show($"PDF généré avec succès à : {outputPath}");
            System.Diagnostics.Debug.WriteLine("test 4");
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
            const double step = 1; // Pas pour décaler les étiquettes
            double maxWidth = 844; // Largeur maximale du plateau
            double maxHeight = 500; // Hauteur maximale du plateau

            while (IsOverlapping(newLabel))
            {
                newLabel.X += step;
                if (newLabel.X + newLabel.Width > maxWidth) // Si on dépasse la largeur, passer à une nouvelle ligne
                {
                    newLabel.X = 0;
                    newLabel.Y += step;
                }

                if (newLabel.Y + newLabel.Height > maxHeight) // Si on dépasse la hauteur
                {
                    System.Windows.MessageBox.Show("Impossible de placer l'étiquette : espace insuffisant. Réorganisez vos étiquettes.");
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

