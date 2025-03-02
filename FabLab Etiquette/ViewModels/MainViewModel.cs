using FabLab_Etiquette.Helpers;
using FabLab_Etiquette.Views;
using System.Windows.Input;

namespace FabLab_Etiquette.ViewModels
{
    public class MainViewModel
    {
        public ICommand CreatePdfCommand { get; }
        public ICommand StandardizePdfCommand { get; }

        public MainViewModel()
        {
            CreatePdfCommand = new RelayCommand(OpenCreatePdfView);
            StandardizePdfCommand = new RelayCommand(OpenStandardizePdfView);
        }
        private void OpenCreatePdfView()
        {
            CreatePdfView view = new CreatePdfView();
            view.ShowDialog();
        }

        private void OpenStandardizePdfView()
        {
            var viewModel = new StandardizePdfViewModel();
            var view = new StandardizePdfView { DataContext = viewModel };
            view.ShowDialog();
        }
    }
}
