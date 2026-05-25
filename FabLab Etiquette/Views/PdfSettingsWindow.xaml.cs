using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;


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

        public static void CreateLabelsPdf(List<LabelModel> labels, string outputPath)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            foreach (var label in labels)
            {
                var pen = new XPen(XColors.Red, label.BorderThickness);
                var font = new XFont(label.FontFamily, label.FontSize);
                var rect = new XRect(label.X, label.Y, label.Width, label.Height);

                gfx.DrawRectangle(pen, XBrushes.Transparent, rect);
                gfx.DrawString(label.Text, font, XBrushes.Black, rect, XStringFormats.Center);
            }

            document.Save(outputPath);
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
            FabLab_Etiquette.Services.PdfService.CreateLabelsPdf(Labels, outputPath);

            MessageBox.Show($"PDF généré avec succès : {outputPath}",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
