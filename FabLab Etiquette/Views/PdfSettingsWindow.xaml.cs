using FabLab_Etiquette.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;
using FabLab_Etiquette.Models;
using FabLab_Etiquette.Services;

namespace FabLab_Etiquette.ViewModels
{
    public class PdfSettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<LabelModel> Labels { get; set; } = new ObservableCollection<LabelModel>();


        // Champs fixes
        public string PdfColor { get; } = "Noir/Rouge";
        public string PdfStyle { get; } = "rigide 1.6 mm";
        public string PdfPrintCount { get; } = "10";
        public string PdfService { get; } = "FABLAB";

        // Champs modifiables
        private string _pdfName;
        public string PdfName
        {
            get => _pdfName;
            set { _pdfName = value; OnPropertyChanged(); }
        }

        private string _pdfNumber;
        public string PdfNumber
        {
            get => _pdfNumber;
            set { _pdfNumber = value; OnPropertyChanged(); }
        }

        private string _pdfTitle;
        public string PdfTitle
        {
            get => _pdfTitle;
            set { _pdfTitle = value; OnPropertyChanged(); }
        }

        // Commande pour générer le PDF
        public ICommand GeneratePdfCommand { get; }

        public PdfSettingsViewModel()
        {
            GeneratePdfCommand = new RelayCommand(GeneratePdf);
        }

        public void GeneratePdf()
        {
            if (string.IsNullOrWhiteSpace(PdfName) ||
                string.IsNullOrWhiteSpace(PdfNumber) ||
                string.IsNullOrWhiteSpace(PdfTitle))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires !",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Construire le nom du fichier PDF
            string fileName = $"{PdfColor}_{PdfStyle}_{PdfPrintCount}_" +
                              $"{PdfName}_{PdfService}_{PdfNumber}_{PdfTitle}.pdf";
            string outputPath = System.IO.Path.Combine(@"C:\Temp\", fileName);




            // Appel au service PDF
            PdfService.CreateLabelsPdf(Labels, outputPath);

            MessageBox.Show($"PDF généré avec succès : {outputPath}",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
