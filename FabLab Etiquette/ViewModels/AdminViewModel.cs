using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Models;
using FabLab_Etiquette.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FabLab_Etiquette.ViewModels
{
    public class StyleEditorRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _name = "";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private double _thickness;
        public double Thickness { get => _thickness; set { _thickness = value; OnPropertyChanged(); } }

        private bool _isAdhesive;
        public bool IsAdhesive { get => _isAdhesive; set { _isAdhesive = value; OnPropertyChanged(); } }

        private string _colorsText = "";
        public string ColorsText { get => _colorsText; set { _colorsText = value; OnPropertyChanged(); } }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public PlateStyle ToPlateStyle() => new PlateStyle
        {
            Name = Name,
            Thickness = Thickness,
            IsAdhesive = IsAdhesive,
            AvailableColors = ColorsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
        };

        public static StyleEditorRow FromPlateStyle(PlateStyle s) => new StyleEditorRow
        {
            Name = s.Name,
            Thickness = s.Thickness,
            IsAdhesive = s.IsAdhesive,
            ColorsText = string.Join(", ", s.AvailableColors)
        };
    }

    public class AdminViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<StyleEditorRow> Styles { get; } = new();

        private string _defaultOutputPath = "";
        public string DefaultOutputPath
        {
            get => _defaultOutputPath;
            set { _defaultOutputPath = value; OnPropertyChanged(); }
        }

        private StyleEditorRow? _selectedStyle;
        public StyleEditorRow? SelectedStyle
        {
            get => _selectedStyle;
            set { _selectedStyle = value; OnPropertyChanged(); }
        }

        public ICommand AddStyleCommand { get; }
        public ICommand RemoveStyleCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand BrowseOutputPathCommand { get; }

        public AdminViewModel()
        {
            var config = ConfigService.Load();
            DefaultOutputPath = config.DefaultOutputPath;
            foreach (var s in config.PlateStyles)
                Styles.Add(StyleEditorRow.FromPlateStyle(s));

            AddStyleCommand = new RelayCommand(AddStyle);
            RemoveStyleCommand = new RelayCommand(RemoveStyle, () => SelectedStyle != null);
            SaveCommand = new RelayCommand(Save);
            ResetCommand = new RelayCommand(Reset);
            BrowseOutputPathCommand = new RelayCommand(BrowseOutputPath);
        }

        private void AddStyle()
        {
            Styles.Add(new StyleEditorRow
            {
                Name = "Nouveau style",
                Thickness = 1.6,
                IsAdhesive = true,
                ColorsText = "Blanc sur fond Noir"
            });
        }

        private void RemoveStyle()
        {
            if (SelectedStyle != null)
                Styles.Remove(SelectedStyle);
        }

        private void Save()
        {
            var config = new AppConfig
            {
                DefaultOutputPath = DefaultOutputPath,
                PlateStyles = Styles.Select(s => s.ToPlateStyle()).ToList()
            };
            ConfigService.Save(config);
            MessageBox.Show("Configuration sauvegardée avec succès.", "Succès",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Reset()
        {
            var result = MessageBox.Show("Réinitialiser la configuration par défaut ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var def = ConfigService.GetDefault();
            Styles.Clear();
            foreach (var s in def.PlateStyles)
                Styles.Add(StyleEditorRow.FromPlateStyle(s));
            DefaultOutputPath = def.DefaultOutputPath;
        }

        private void BrowseOutputPath()
        {
            var dialog = new OpenFolderDialog { Title = "Choisir le dossier de sortie des PDF" };
            if (dialog.ShowDialog() == true)
                DefaultOutputPath = dialog.FolderName;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            (RemoveStyleCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
