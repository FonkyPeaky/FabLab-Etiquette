using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Models;
using FabLab_Etiquette.Views;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        public string _pdfPath = @"C:\Temp\Etiquettes.pdf"; // Chemin du fichier PDF pour test

        public string UserName { get; set; } = "INCONNU";
        public string UserService { get; set; } = "SERVICE";
        public string UserNumber { get; set; } = "00000";
        public string LabelTitle { get; set; } = "Nouvelle étiquette";
        public string LabelColor { get; set; } = "Blanc";
        public string LabelStyle { get; set; } = "1.6";
        public int PrintCount { get; set; } = 1;
        public ObservableCollection<LabelModel> LabelList { get; set; } = new ObservableCollection<LabelModel>();

        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        }

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
            LabelList = new ObservableCollection<LabelModel>();
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

            Debug.WriteLine($"🔍 LabelList initialisée : {LabelList.Count} étiquettes.");
            Debug.WriteLine($"🔍 Initialisation LabelList : {LabelList?.Count ?? 0} étiquettes.");

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

                SyncLabelList(); // 🟢 Synchroniser immédiatement après une modification

                Debug.WriteLine($"🟢 Labels: {Labels.Count} étiquettes");
                Debug.WriteLine($"🟢 LabelList: {LabelList.Count} étiquettes");

                var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                view?.DrawLabels();
            };

        }

        public void SyncLabelList()
        {
            LabelList.Clear();
            foreach (var label in Labels)
            {
                LabelList.Add(label);
            }
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
                    System.Diagnostics.Debug.WriteLine($"📌 SelectedLabel : Texte={SelectedLabel.Text}, X={SelectedLabel.X}, Y={SelectedLabel.Y}");
                }

                string imagePath = openFileDialog.FileName;
                SelectedLabel.ImageSource = imagePath; // ✅ Utilisation de `ImageSource`

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
        public void UpdateSelectedLabel()
        {
            if (SelectedLabel != null)
            {
                SelectedLabel.Text = SelectedLabel.Text.Trim();
                OnPropertyChanged(nameof(SelectedLabel));
                var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
                view?.DrawLabels();
            }
        }

        private void AddLabel()
        {
            const double scaleFactor = 1;
            const double canvasWidth = 1237;
            const double canvasHeight = 666;
            const double realWidth = canvasWidth * scaleFactor;
            const double realHeight = canvasHeight * scaleFactor;

            // 🔥 Copier les valeurs en cours de saisie 🔥
            string existingText = SelectedLabel?.Text ?? "Nouvelle étiquette";
            double existingWidth = SelectedLabel?.Width ?? 100;
            double existingHeight = SelectedLabel?.Height ?? 50;
            string existingFontFamily = SelectedLabel?.FontFamily ?? "Arial";
            double existingFontSize = SelectedLabel?.FontSize ?? 10;
            Brush existingBackgroundColor = SelectedLabel?.BackgroundColor ?? Brushes.White;
            Brush existingBorderColor = SelectedLabel?.BorderColor ?? Brushes.Red;
            double existingBorderThickness = SelectedLabel?.BorderThickness ?? 2;
            string existingShape = SelectedLabel?.Shape ?? "Rectangle";

            LabelModel newLabel;

            if (Labels.Count > 0)
            {
                var lastLabel = Labels.Last();
                newLabel = new LabelModel
                {
                    X = lastLabel.X,
                    Y = lastLabel.Y + lastLabel.Height,
                    Width = existingWidth,
                    Height = existingHeight,
                    Text = existingText,  // 🔥 On garde le texte saisi
                    FontFamily = existingFontFamily,
                    FontSize = existingFontSize,
                    BackgroundColor = existingBackgroundColor,
                    BorderColor = existingBorderColor,
                    BorderThickness = existingBorderThickness,
                    Shape = existingShape
                };

                // Vérification des limites
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
                // Création de la première étiquette avec les valeurs saisies
                newLabel = new LabelModel
                {
                    X = 0,
                    Y = 0,
                    Width = existingWidth,
                    Height = existingHeight,
                    Text = existingText, // 🔥 On garde le texte saisi
                    FontFamily = existingFontFamily,
                    FontSize = existingFontSize,
                    BackgroundColor = existingBackgroundColor,
                    BorderColor = existingBorderColor,
                    BorderThickness = existingBorderThickness,
                    Shape = existingShape
                };
            }

            // Ajout et mise à jour
            Labels.Add(newLabel);
            SelectedLabel = newLabel;

            // Mise à jour de l'affichage
            var view = System.Windows.Application.Current.Windows.OfType<CreatePdfView>().FirstOrDefault();
            view?.DrawLabels();
        }



        private bool AskUserForDetails()
        {
            var userInfoWindow = new UserInfoWindow();
            if (userInfoWindow.ShowDialog() == true)
            {
                UserName = userInfoWindow.UserName;
                UserService = userInfoWindow.UserService;
                UserNumber = userInfoWindow.UserNumber;
                return true;
            }
            return false;
        }

        public void GeneratePdf()
        {
            if (Labels == null || Labels.Count == 0)
            {
                MessageBox.Show("Aucune étiquette à générer !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!AskUserForDetails()) // 🚀 Demander les infos utilisateur
            {
                return; // Si l'utilisateur annule, ne pas générer le PDF
            }

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Enregistrer le PDF",
                FileName = GenerateStandardFileName()
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return; // L'utilisateur a annulé
            }

            string outputPath = saveFileDialog.FileName;

            try
            {
                var observableLabels = new ObservableCollection<LabelModel>(Labels);
                FabLab_Etiquette.Services.PdfService.CreateLabelsPdf(observableLabels, outputPath);

                SelectedFilePath = outputPath;
                MessageBox.Show($"✅ PDF généré avec succès !\n\n📂 Emplacement : {outputPath}",
                                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de la génération du PDF : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            const double canvasWidth = 1237; // Largeur de la zone d'affichage
            const double canvasHeight = 666; // Hauteur de la zone d'affichage

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

        private string GenerateStandardFileName()
        {
            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(UserService) ||
                string.IsNullOrWhiteSpace(UserNumber) ||
                string.IsNullOrWhiteSpace(LabelTitle) ||
                string.IsNullOrWhiteSpace(LabelColor) ||
                string.IsNullOrWhiteSpace(LabelStyle) ||
                Labels.Count == 0 ||
                PrintCount <= 0)
            {
                MessageBox.Show("⚠ Certaines informations sont manquantes pour générer le nom du fichier.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "Etiquette_Invalide.pdf";
            }

            bool isAutocollant = LabelStyle.ToLower().Contains("autocollant");
            string autocollantStr = isAutocollant ? "autocollant" : "non_autocollant";

            // 📌 Récupérer l'étiquette sélectionnée, ou la première si aucune n'est choisie
            string labelSampleText = SelectedLabel != null ? SelectedLabel.Text : Labels.FirstOrDefault()?.Text ?? "Sans_Texte";

            // 📌 Nettoyage des caractères interdits et troncature à 30 caractères max
            string safeLabelText = new string(labelSampleText.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
            safeLabelText = safeLabelText.Length > 30 ? safeLabelText.Substring(0, 30) : safeLabelText;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string filename = $"{LabelColor}_{LabelStyle}_{autocollantStr}_x{PrintCount}#" +
                              $"{UserName.ToUpper()}_{UserService.ToUpper()}_{UserNumber}#" +
                              $"{LabelTitle}_{safeLabelText}_{SelectedLabel?.BackgroundColorHex}.pdf";


            if (filename.Length > 100)
                filename = filename.Substring(0, 100) + ".pdf";

            return filename;
        }

        public string GenerateStandardJsonFileName()
        {
            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(UserService) ||
                string.IsNullOrWhiteSpace(UserNumber) ||
                string.IsNullOrWhiteSpace(LabelTitle) ||
                string.IsNullOrWhiteSpace(LabelColor) ||
                string.IsNullOrWhiteSpace(LabelStyle) ||
                Labels.Count == 0 ||
                PrintCount <= 0)
            {
                MessageBox.Show("⚠ Certaines informations sont manquantes pour générer le nom du fichier.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "Etiquette_Invalide.json";
            }

            bool isAutocollant = LabelStyle.ToLower().Contains("autocollant");
            string autocollantStr = isAutocollant ? "autocollant" : "non_autocollant";

            // 📌 Récupérer l'étiquette sélectionnée, ou la première si aucune n'est choisie
            string labelSampleText = SelectedLabel != null ? SelectedLabel.Text : Labels.FirstOrDefault()?.Text ?? "Sans_Texte";

            // 📌 Nettoyage des caractères interdits et troncature à 30 caractères max
            string safeLabelText = new string(labelSampleText.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
            safeLabelText = safeLabelText.Length > 30 ? safeLabelText.Substring(0, 30) : safeLabelText;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string filename = $"{LabelColor}_{LabelStyle}_{autocollantStr}_x{PrintCount}#" +
                              $"{UserName.ToUpper()}_{UserService.ToUpper()}_{UserNumber}#" +
                              $"{LabelTitle}_{safeLabelText}_{SelectedLabel?.BackgroundColorHex}#" +
                              $"{LabelList.Count}_etiquettes.json";

            if (filename.Length > 100)
                filename = filename.Substring(0, 100) + ".json";

            return filename;
        }
    }
}

