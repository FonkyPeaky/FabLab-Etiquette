using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Models;
using FabLab_Etiquette.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FabLab_Etiquette.ViewModels
{
    public class CreatePdfViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand AddLabelCommand { get; }
        public ICommand GeneratePdfCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand UpdateSelectedLabelCommand { get; }
        public ICommand RearrangeLabelsCommand { get; }
        public ICommand AlignLabelsCommand { get; }
        public ICommand ArrangeHorizontalCommand { get; }
        public ICommand ArrangeVerticalCommand { get; }
        public ICommand AddNewLineCommand { get; }
        public ICommand ToggleGridCommand { get; }

        public ObservableCollection<LabelModel> Labels { get; } = new();
        public ObservableCollection<LabelModel> LabelList { get; set; } = new();

        // Infos collectées avant l'ouverture de cette vue
        public string UserName { get; }
        public string UserService { get; }
        public string UserNumber { get; }
        public string LabelTitle { get; }
        public string LabelColor { get; }
        public string StyleCode { get; }
        public bool IsAdhesive { get; }
        public int PrintCount { get; }

        private readonly FabricationParams _fabParams;

        private LabelModel? _selectedLabel;
        public LabelModel? SelectedLabel
        {
            get => _selectedLabel;
            set
            {
                _selectedLabel = value;
                OnPropertyChanged();
                RefreshView();
            }
        }

        // Mode disposition courant (horizontal par défaut)
        private bool _isVerticalLayout = false;

        // Étiquettes sélectionnées (multi-sélection)
        public ObservableCollection<LabelModel> SelectedLabels { get; } = new();

        public bool IsMultiSelect => SelectedLabels.Count > 1;

        public void SetSelectedLabels(System.Collections.IList items)
        {
            SelectedLabels.Clear();
            foreach (LabelModel l in items)
                SelectedLabels.Add(l);

            _selectedLabel = SelectedLabels.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedLabel));
            OnPropertyChanged(nameof(IsMultiSelect));
            OnPropertyChanged(nameof(SelectedLabels));
            RefreshView();
        }

        private bool _isGridVisible;
        public bool IsGridVisible
        {
            get => _isGridVisible;
            set
            {
                _isGridVisible = value;
                OnPropertyChanged();
                RefreshView();
            }
        }

        private string _imageSource = "";
        public string ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(); }
        }

        private string _selectedFilePath = "";
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set { _selectedFilePath = value; OnPropertyChanged(); }
        }

        public CreatePdfViewModel(UserInfo userInfo, FabricationParams fabParams)
        {
            UserName = userInfo.Name;
            UserService = userInfo.Service;
            UserNumber = userInfo.Number;
            LabelTitle = fabParams.LabelTitle;
            LabelColor = fabParams.SelectedColor;
            StyleCode = fabParams.SelectedStyle?.StyleCode ?? "";
            IsAdhesive = fabParams.SelectedStyle?.IsAdhesive ?? false;
            PrintCount = fabParams.PrintCount;
            _fabParams = fabParams;

            AddLabelCommand = new RelayCommand(AddLabel);
            GeneratePdfCommand = new RelayCommand(GeneratePdf);
            UpdateSelectedLabelCommand = new RelayCommand(UpdateSelectedLabel);
            RearrangeLabelsCommand = new RelayCommand(ArrangeHorizontal);
            AlignLabelsCommand = new RelayCommand(ArrangeHorizontal);
            ArrangeHorizontalCommand = new RelayCommand(ArrangeHorizontal);
            ArrangeVerticalCommand = new RelayCommand(ArrangeVertical);
            AddImageCommand = new RelayCommand(AddImageToLabel);
            AddNewLineCommand = new RelayCommand(AddNewLine);
            ToggleGridCommand = new RelayCommand(() => IsGridVisible = !IsGridVisible);

            Labels.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (LabelModel label in e.NewItems)
                        label.PropertyChanged += Label_PropertyChanged;

                if (e.OldItems != null)
                    foreach (LabelModel label in e.OldItems)
                        label.PropertyChanged -= Label_PropertyChanged;

                SyncLabelList();
                RefreshView();
            };
        }

        private bool _propagating = false;

        private void Label_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Propagate changes from the primary selected label to all other selected labels
            if (!_propagating && sender is LabelModel source && source == _selectedLabel && SelectedLabels.Count > 1)
            {
                _propagating = true;
                foreach (var other in SelectedLabels)
                {
                    if (other == source) continue;
                    PropagateProperty(source, other, e.PropertyName);
                }
                _propagating = false;
            }
            RefreshView();
        }

        private static void PropagateProperty(LabelModel src, LabelModel dst, string? prop)
        {
            switch (prop)
            {
                case nameof(LabelModel.Text):              dst.Text = src.Text; break;
                case nameof(LabelModel.Width):             dst.Width = src.Width; break;
                case nameof(LabelModel.Height):            dst.Height = src.Height; break;
                case nameof(LabelModel.FontFamily):        dst.FontFamily = src.FontFamily; break;
                case nameof(LabelModel.FontSize):          dst.FontSize = src.FontSize; break;
                case nameof(LabelModel.Shape):             dst.Shape = src.Shape; break;
                case nameof(LabelModel.Action):            dst.Action = src.Action; break;
                case nameof(LabelModel.BackgroundColorHex):dst.BackgroundColorHex = src.BackgroundColorHex; break;
                case nameof(LabelModel.BorderColorHex):    dst.BorderColorHex = src.BorderColorHex; break;
                case nameof(LabelModel.BorderThickness):   dst.BorderThickness = src.BorderThickness; break;
                // X et Y ne sont pas propagés (chaque étiquette garde sa position)
            }
        }

        private void RefreshView()
        {
            var view = Application.Current.Windows.OfType<Views.CreatePdfView>().FirstOrDefault();
            view?.DrawLabels();
        }

        public void SyncLabelList()
        {
            LabelList.Clear();
            foreach (var label in Labels)
                LabelList.Add(label);
        }

        private void AddNewLine()
        {
            if (SelectedLabel != null)
            {
                SelectedLabel.Text += "\n";
                OnPropertyChanged(nameof(SelectedLabel));
            }
        }

        public void AddImageToLabel()
        {
            if (SelectedLabel == null)
            {
                MessageBox.Show("Veuillez sélectionner une étiquette avant d'ajouter une image.",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Sélectionnez une image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedLabel.ImageSource = openFileDialog.FileName;
                RefreshView();
            }
        }

        // Gauche → droite en grille uniforme (pas = largeur max des étiquettes)
        private void ArrangeHorizontal()
        {
            _isVerticalLayout = false;
            const double maxW = 1700, maxH = 850;
            if (!Labels.Any()) return;

            double colW = Labels.Max(l => l.Width);   // pas de colonne uniforme
            double rowH = Labels.Max(l => l.Height);  // pas de ligne uniforme

            double x = 0, y = 0;
            foreach (var label in Labels)
            {
                if (x + colW > maxW) { x = 0; y += rowH; }
                if (y + rowH > maxH)
                {
                    MessageBox.Show("Certaines étiquettes dépassent les limites du plateau.",
                        "Dépassement", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                }
                label.X = x;
                label.Y = y;
                x += colW;
            }
            RefreshView();
        }

        // Haut → bas en grille uniforme (pas = hauteur max des étiquettes)
        private void ArrangeVertical()
        {
            _isVerticalLayout = true;
            const double maxW = 1700, maxH = 850;
            if (!Labels.Any()) return;

            double colW = Labels.Max(l => l.Width);   // pas de colonne uniforme
            double rowH = Labels.Max(l => l.Height);  // pas de ligne uniforme

            double x = 0, y = 0;
            foreach (var label in Labels)
            {
                if (y + rowH > maxH) { y = 0; x += colW; }
                if (x + colW > maxW)
                {
                    MessageBox.Show("Certaines étiquettes dépassent les limites du plateau.",
                        "Dépassement", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                }
                label.X = x;
                label.Y = y;
                y += rowH;
            }
            RefreshView();
        }

        public void UpdateSelectedLabel()
        {
            if (SelectedLabel != null)
            {
                SelectedLabel.Text = SelectedLabel.Text.Trim();
                OnPropertyChanged(nameof(SelectedLabel));
                RefreshView();
            }
        }

        private void AddLabel()
        {
            const double canvasWidth = 1700;
            const double canvasHeight = 850;

            double width = SelectedLabel?.Width ?? 150;
            double height = SelectedLabel?.Height ?? 60;
            string text = SelectedLabel?.Text ?? "Nouvelle étiquette";
            string fontFamily = SelectedLabel?.FontFamily ?? "Arial";
            double fontSize = SelectedLabel?.FontSize ?? 12;
            string bgHex = SelectedLabel?.BackgroundColorHex ?? ParseBackgroundHex(LabelColor);
            Brush borderColor = SelectedLabel?.BorderColor ?? Brushes.Red;
            double borderThickness = SelectedLabel?.BorderThickness ?? 1;
            string shape = SelectedLabel?.Shape ?? "Rectangle";

            LabelModel newLabel;
            if (Labels.Count > 0)
            {
                var lastLabel = Labels.Last();
                // Pas de grille basé sur la taille max (pour rester aligné)
                double colStride = Math.Max(width, Labels.Max(l => l.Width));
                double rowStride = Math.Max(height, Labels.Max(l => l.Height));
                double nextX, nextY;

                if (_isVerticalLayout)
                {
                    nextX = lastLabel.X;
                    nextY = lastLabel.Y + rowStride;
                    if (nextY + height > canvasHeight)
                    {
                        nextY = 0;
                        nextX = lastLabel.X + colStride;
                    }
                }
                else
                {
                    nextX = lastLabel.X + colStride;
                    nextY = lastLabel.Y;
                    if (nextX + width > canvasWidth)
                    {
                        nextX = 0;
                        nextY = lastLabel.Y + rowStride;
                    }
                }

                if (nextY + height > canvasHeight || nextX + width > canvasWidth)
                {
                    MessageBox.Show("Impossible d'ajouter plus d'étiquettes : dépassement des limites du plateau.",
                        "Limite atteinte", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                newLabel = new LabelModel
                {
                    X = nextX, Y = nextY,
                    Width = width, Height = height, Text = text,
                    FontFamily = fontFamily, FontSize = fontSize,
                    BackgroundColorHex = bgHex, BorderColor = borderColor,
                    BorderThickness = borderThickness, Shape = shape
                };
            }
            else
            {
                newLabel = new LabelModel
                {
                    X = 0, Y = 0, Width = width, Height = height, Text = text,
                    FontFamily = fontFamily, FontSize = fontSize,
                    BackgroundColorHex = bgHex, BorderColor = borderColor,
                    BorderThickness = borderThickness, Shape = shape
                };
            }

            Labels.Add(newLabel);
            SelectedLabel = newLabel;
        }

        // Extrait la couleur de fond à partir du libellé de couleur plaque (ex. "Blanc sur fond Vert")
        private static string ParseBackgroundHex(string colorLabel)
        {
            var lower = colorLabel.ToLowerInvariant();
            int idx = lower.IndexOf("fond ", StringComparison.Ordinal);
            string part = idx >= 0 ? lower[(idx + 5)..].Trim() : lower;

            if (part.StartsWith("noir")) return "#000000";
            if (part.StartsWith("vert")) return "#008000";
            if (part.StartsWith("jaune")) return "#FFFF00";
            if (part.StartsWith("rouge")) return "#FF0000";
            if (part.StartsWith("bleu")) return "#0000FF";
            if (part.StartsWith("blanc")) return "#FFFFFF";
            return "#FFFFFF";
        }

        public void GeneratePdf()
        {
            if (Labels == null || Labels.Count == 0)
            {
                MessageBox.Show("Aucune étiquette à générer.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var config = ConfigService.Load();
            string initialDir = System.IO.Directory.Exists(config.DefaultOutputPath)
                ? config.DefaultOutputPath
                : Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Enregistrer le PDF",
                InitialDirectory = initialDir,
                FileName = GenerateStandardFileName()
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string outputPath = saveFileDialog.FileName;

            try
            {
                PdfService.CreateLabelsPdf(Labels, outputPath);
                SelectedFilePath = outputPath;
                MessageBox.Show($"PDF généré avec succès !\n\nEmplacement : {outputPath}",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la génération du PDF : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string GenerateStandardFileName()
        {
            string styleCode = _fabParams.SelectedStyle?.StyleCode ?? "inconnu";
            string color = SanitizeFileName(LabelColor);
            string sample = SanitizeFileName(Labels.FirstOrDefault()?.Text ?? "");
            if (sample.Length > 30) sample = sample[..30].TrimEnd();

            string filename = $"{color}_{styleCode}_x{PrintCount}#" +
                              $"{UserName.ToUpper()}_{UserService.ToUpper()}_{UserNumber}#" +
                              $"{SanitizeFileName(LabelTitle)}_{sample}.pdf";

            return SanitizeWindowsFileName(filename);
        }

        public string GenerateStandardJsonFileName()
        {
            string styleCode = _fabParams.SelectedStyle?.StyleCode ?? "inconnu";
            string color = SanitizeFileName(LabelColor);
            string sample = SanitizeFileName(Labels.FirstOrDefault()?.Text ?? "");
            if (sample.Length > 30) sample = sample[..30].TrimEnd();

            string filename = $"{color}_{styleCode}_x{PrintCount}#" +
                              $"{UserName.ToUpper()}_{UserService.ToUpper()}_{UserNumber}#" +
                              $"{SanitizeFileName(LabelTitle)}_{sample}.json";

            return SanitizeWindowsFileName(filename);
        }

        private static string SanitizeFileName(string input)
        {
            // Supprime les caractères interdits dans les noms de fichier
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            return new string(input.Where(c => !invalid.Contains(c)).ToArray());
        }

        private static string SanitizeWindowsFileName(string filename)
        {
            // Tronque si trop long (limite Windows ~255 caractères)
            if (filename.Length > 200)
                filename = filename[..196] + ".pdf";
            return filename;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
