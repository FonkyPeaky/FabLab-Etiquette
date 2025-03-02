using FabLab_Etiquette.ViewModels;
using FabLab_Etiquette.Views;
using System.Windows;
using System.Windows.Threading;

namespace FabLab_Etiquette
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        private void OpenCreatePdfView_Click(object sender, RoutedEventArgs e)
        {
            // Désactiver le bouton temporairement pour éviter le spam
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
                button.IsEnabled = false;

            // Créer un timer pour retarder l'ouverture
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500); // 0.5 seconde
            timer.Tick += (s, args) =>
            {
                timer.Stop(); // Arrêter le timer une fois exécuté
                CreatePdfView createPdfWindow = new CreatePdfView();
                createPdfWindow.Show();

                // Réactiver le bouton après l'ouverture de la fenêtre
                if (button != null)
                    button.IsEnabled = true;
            };
            timer.Start();
        }
    }
}

