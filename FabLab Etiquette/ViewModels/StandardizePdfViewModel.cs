using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Models;
using FabLab_Etiquette.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace FabLab_Etiquette.ViewModels
{
    public class StandardizePdfViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly string _sourcePath;
        private readonly UserInfo _userInfo;
        private readonly FabricationParams _fabParams;

        // Lecture seule — affiché en résumé
        public string SourceFileName => Path.GetFileName(_sourcePath);
        public string SummaryUserName => _userInfo.Name.ToUpper();
        public string SummaryUserService => _userInfo.Service.ToUpper();
        public string SummaryUserNumber => _userInfo.Number;
        public string SummaryStyle => _fabParams.SelectedStyle?.DisplayName ?? "-";
        public string SummaryColor => _fabParams.SelectedColor;
        public string SummaryPrintCount => $"x{_fabParams.PrintCount}";
        public string SummaryLabelTitle => _fabParams.LabelTitle;
        public string PreviewFileName => GenerateStandardFileName();

        // Texte d'échantillon saisissable manuellement (extrait du contenu d'une étiquette)
        private string _sampleText = "";
        public string SampleText
        {
            get => _sampleText;
            set
            {
                _sampleText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewFileName));
            }
        }

        public ICommand StandardizeCommand { get; }

        public StandardizePdfViewModel(string sourcePath, UserInfo userInfo, FabricationParams fabParams)
        {
            _sourcePath = sourcePath;
            _userInfo = userInfo;
            _fabParams = fabParams;

            StandardizeCommand = new RelayCommand(StandardizeFile);
        }

        private string GenerateStandardFileName()
        {
            string styleCode = _fabParams.SelectedStyle?.StyleCode ?? "inconnu";
            string color = SanitizeFileName(_fabParams.SelectedColor);
            string name = SanitizeFileName(_userInfo.Name.ToUpper());
            string service = SanitizeFileName(_userInfo.Service.ToUpper());
            string number = SanitizeFileName(_userInfo.Number);
            string title = SanitizeFileName(_fabParams.LabelTitle);
            string sample = SanitizeFileName(_sampleText);
            if (sample.Length > 40) sample = sample[..40].TrimEnd();

            string filename = $"{color}_{styleCode}_x{_fabParams.PrintCount}#" +
                              $"{name}_{service}_{number}#" +
                              $"{title}_{sample}.pdf";

            return SanitizeWindowsFileName(filename);
        }

        private void StandardizeFile()
        {
            var config = ConfigService.Load();
            string initialDir = System.IO.Directory.Exists(config.DefaultOutputPath)
                ? config.DefaultOutputPath
                : Path.GetDirectoryName(_sourcePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string newFileName = GenerateStandardFileName();
            string newFilePath = Path.Combine(initialDir, newFileName);

            try
            {
                File.Copy(_sourcePath, newFilePath, overwrite: false);
                MessageBox.Show(
                    $"Fichier standardisé avec succès !\n\nFichier créé :\n{newFilePath}",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException)
            {
                var result = MessageBox.Show(
                    $"Un fichier portant ce nom existe déjà :\n{newFilePath}\n\nVoulez-vous le remplacer ?",
                    "Fichier existant", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    File.Copy(_sourcePath, newFilePath, overwrite: true);
                    MessageBox.Show($"Fichier remplacé avec succès :\n{newFilePath}",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la copie : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string SanitizeFileName(string input)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(input.Where(c => !invalid.Contains(c)).ToArray());
        }

        private static string SanitizeWindowsFileName(string filename)
        {
            if (filename.Length > 200)
                filename = filename[..196] + ".pdf";
            return filename;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
