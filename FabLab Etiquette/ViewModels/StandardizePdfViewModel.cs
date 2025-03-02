using FabLab_Etiquette.Helpers;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;


namespace FabLab_Etiquette.ViewModels
{
    public class StandardizePdfViewModel
    {
        public ICommand SelectFileCommand { get; }
        public ICommand StandardizeFileCommand { get; }

        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                Console.WriteLine($"📂 SelectedFilePath mis à jour : {_selectedFilePath}");
                OnPropertyChanged();
            }
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
                RefreshCanExecute();
            }
        }

        private string _userService;
        public string UserService
        {
            get => _userService;
            set
            {
                _userService = value;
                OnPropertyChanged();
                RefreshCanExecute();
            }
        }

        private string _userNumber;
        public string UserNumber
        {
            get => _userNumber;
            set
            {
                _userNumber = value;
                OnPropertyChanged();
                RefreshCanExecute();
            }
        }

        private string _labelTitle;
        public string LabelTitle
        {
            get => _labelTitle;
            set { _labelTitle = value; OnPropertyChanged(); RefreshCanExecute(); }
        }

        private string _labelText;
        public string LabelText
        {
            get => _labelText;
            set { _labelText = value; OnPropertyChanged(); RefreshCanExecute(); }
        }

        private string _labelColor;
        public string LabelColor
        {
            get => _labelColor;
            set { _labelColor = value; OnPropertyChanged(); RefreshCanExecute(); }
        }

        private string _labelStyle;
        public string LabelStyle
        {
            get => _labelStyle;
            set { _labelStyle = value; OnPropertyChanged(); RefreshCanExecute(); }
        }

        private int _printCount;
        public int PrintCount
        {
            get => _printCount;
            set { _printCount = value; OnPropertyChanged(); RefreshCanExecute(); }
        }
        public StandardizePdfViewModel()
        {
            Console.WriteLine("✅ StandardizePdfViewModel INITIALISÉ !");
            SelectFileCommand = new RelayCommand(SelectFile);
            StandardizeFileCommand = new RelayCommand(StandardizeFile, CanStandardize);

            Console.WriteLine($"📌 SelectFileCommand est null ? {SelectFileCommand == null}");
        }

        public void SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Sélectionnez un fichier PDF"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
                Console.WriteLine($"📂 Fichier sélectionné : {SelectedFilePath}");
                MessageBox.Show($"📂 Fichier sélectionné : {SelectedFilePath}");

                // 🚀 Force la mise à jour de l'UI !
                OnPropertyChanged(nameof(SelectedFilePath));
                RefreshCanExecute();
            }
        }

        private bool CanStandardize()
        {
            Console.WriteLine($"🛠 Vérification CanStandardize :\n" +
                $"📂 SelectedFilePath : {SelectedFilePath}\n" +
                $"👤 UserName : {UserName}\n" +
                $"🏢 UserService : {UserService}\n" +
                $"🔢 UserNumber : {UserNumber}");

            return !string.IsNullOrWhiteSpace(SelectedFilePath) &&
                   !string.IsNullOrWhiteSpace(UserName) &&
                   !string.IsNullOrWhiteSpace(UserService) &&
                   !string.IsNullOrWhiteSpace(UserNumber);
        }

        private void StandardizeFile()
        {
            if (!CanStandardize())
            {
                MessageBox.Show("Veuillez remplir tous les champs avant de standardiser le fichier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            GenerateStandardizedPdf();
        }

        private string GenerateStandardFileName()
        {
            string sanitizedText = Regex.Replace(LabelText ?? "", "[^a-zA-Z0-9 ]", "");
            sanitizedText = string.Join(" ", sanitizedText.Split().Take(5));

            string filename = $"{LabelColor}_{LabelStyle}_{PrintCount}x#{UserName.ToUpper()}_{UserService.ToUpper()}_{UserNumber}#{LabelTitle}_{sanitizedText}.pdf";

            if (filename.Length > 100)
                filename = filename.Substring(0, 100) + ".pdf";

            return filename;
        }

        private void RefreshCanExecute()
        {
            (StandardizeFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GenerateStandardizedPdf()
        {
            if (string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                MessageBox.Show("Aucun fichier PDF sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string directory = Path.GetDirectoryName(SelectedFilePath);
            string newFileName = GenerateStandardFileName();
            string newFilePath = Path.Combine(directory, newFileName);

            try
            {
                // 🚀 Copier directement le fichier sans modification
                File.Copy(SelectedFilePath, newFilePath, overwrite: true);

                MessageBox.Show($"✅ PDF standardisé créé sans modification : {newFilePath}", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de la copie du PDF : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
