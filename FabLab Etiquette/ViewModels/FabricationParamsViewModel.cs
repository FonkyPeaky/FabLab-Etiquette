using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Models;
using FabLab_Etiquette.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FabLab_Etiquette.ViewModels
{
    public class FabricationParamsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<bool>? RequestClose;

        public ObservableCollection<PlateStyle> PlateStyles { get; } = new();
        public ObservableCollection<string> AvailableColors { get; } = new();

        private PlateStyle? _selectedStyle;
        public PlateStyle? SelectedStyle
        {
            get => _selectedStyle;
            set
            {
                _selectedStyle = value;
                OnPropertyChanged();
                UpdateAvailableColors();
            }
        }

        private string _selectedColor = "";
        public string SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(); }
        }

        private int _printCount = 1;
        public int PrintCount
        {
            get => _printCount;
            set { _printCount = value < 1 ? 1 : value; OnPropertyChanged(); }
        }

        private string _labelTitle = "";
        public string LabelTitle
        {
            get => _labelTitle;
            set { _labelTitle = value; OnPropertyChanged(); }
        }

        public ICommand ValidateCommand { get; }
        public ICommand CancelCommand { get; }

        public FabricationParams? Result { get; private set; }

        public FabricationParamsViewModel()
        {
            var config = ConfigService.Load();
            foreach (var style in config.PlateStyles)
                PlateStyles.Add(style);

            ValidateCommand = new RelayCommand(Validate, CanValidate);
            CancelCommand = new RelayCommand(() => RequestClose?.Invoke(this, false));
        }

        private void UpdateAvailableColors()
        {
            AvailableColors.Clear();
            if (_selectedStyle != null)
            {
                foreach (var color in _selectedStyle.AvailableColors)
                    AvailableColors.Add(color);
                SelectedColor = AvailableColors.Count > 0 ? AvailableColors[0] : "";
            }
            (ValidateCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanValidate() =>
            SelectedStyle != null &&
            !string.IsNullOrWhiteSpace(SelectedColor) &&
            PrintCount > 0 &&
            !string.IsNullOrWhiteSpace(LabelTitle);

        private void Validate()
        {
            Result = new FabricationParams
            {
                SelectedStyle = SelectedStyle,
                SelectedColor = SelectedColor,
                PrintCount = PrintCount,
                LabelTitle = LabelTitle
            };
            RequestClose?.Invoke(this, true);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            (ValidateCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
