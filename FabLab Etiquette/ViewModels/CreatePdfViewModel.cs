using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Services;
using Microsoft.Win32;
using System.Windows.Input;
using FabLab_Etiquette.Models;
using System.Collections.ObjectModel;

namespace FabLab_Etiquette.ViewModels
{
    public class CreatePdfViewModel
    {
        public ObservableCollection<LabelModel> Labels { get; }

        public ICommand AddLabelCommand { get; }
        public ICommand GeneratePdfCommand { get; }

        public CreatePdfViewModel()
        {
            Labels = new ObservableCollection<LabelModel>();

            AddLabelCommand = new RelayCommand(AddLabel);
            GeneratePdfCommand = new RelayCommand(GeneratePdf);
        }

        private void AddLabel()
        {
            Labels.Add(new LabelModel
            {
                X = 50,
                Y = 50,
                Width = 200,
                Height = 100,
                Text = "Nouvelle étiquette"
            });
        }

        private void GeneratePdf()
        {
            if (Labels.Count == 0)
            {
                System.Windows.MessageBox.Show("Aucune étiquette à générer !");
                return;
            }
            System.Windows.MessageBox.Show("Méthode GeneratePdf appelée !");
            string outputPath = "Etiquettes.pdf"; // À remplacer par un chemin sélectionné par l'utilisateur
            PdfService.CreateLabelsPdf(Labels, outputPath);
        }
    }
}

