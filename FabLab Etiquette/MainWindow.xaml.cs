using FabLab_Etiquette.Views;
using Microsoft.Win32;
using System.Windows;

namespace FabLab_Etiquette
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenCreatePdfView_Click(object sender, RoutedEventArgs e)
        {
            // Étape 3 : Infos utilisateur
            var userInfoWindow = new UserInfoWindow();
            if (userInfoWindow.ShowDialog() != true) return;
            var userInfo = userInfoWindow.GetUserInfo();

            // Étape 4 : Paramètres de fabrication
            var fabParamsWindow = new FabricationParamsWindow();
            if (fabParamsWindow.ShowDialog() != true) return;
            var fabParams = fabParamsWindow.ViewModel!.Result!;

            // Étape 6 : Conception
            var createView = new CreatePdfView(userInfo, fabParams);
            createView.Show();
        }

        private void OpenStandardizePdfView_Click(object sender, RoutedEventArgs e)
        {
            // Étape 2 : Sélection du fichier PDF existant
            var openDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Sélectionnez le fichier PDF à standardiser"
            };
            if (openDialog.ShowDialog() != true) return;
            string selectedFilePath = openDialog.FileName;

            // Étape 3 : Infos utilisateur
            var userInfoWindow = new UserInfoWindow();
            if (userInfoWindow.ShowDialog() != true) return;
            var userInfo = userInfoWindow.GetUserInfo();

            // Étape 4 : Paramètres de fabrication
            var fabParamsWindow = new FabricationParamsWindow();
            if (fabParamsWindow.ShowDialog() != true) return;
            var fabParams = fabParamsWindow.ViewModel!.Result!;

            // Étape 7 : Standardisation (renommage)
            var standardizeView = new StandardizePdfView(selectedFilePath, userInfo, fabParams);
            standardizeView.ShowDialog();
        }

        private void OpenAdminWindow_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.ShowDialog();
        }
    }
}
