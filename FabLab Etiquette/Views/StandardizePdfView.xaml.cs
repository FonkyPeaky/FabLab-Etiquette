using System.Windows;
using FabLab_Etiquette.ViewModels;

namespace FabLab_Etiquette.Views
{
    /// <summary>
    /// Interaction logic for StandardizePdfView.xaml
    /// </summary>
    public partial class StandardizePdfView : Window
    {
        public StandardizePdfView()
        {
            InitializeComponent();

            if (DataContext == null)
            {
                MessageBox.Show("❌ DataContext est NULL !");
            }
            else
            {
                MessageBox.Show($"✅ DataContext attaché : {DataContext.GetType().FullName}");
            }
        }

        private void OnBrowseClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("BOUTON PARCOURIR CLIQUÉ !");
        }

        private void OnTestSelectFile(object sender, RoutedEventArgs e)
        {
            if (DataContext is StandardizePdfViewModel vm)
            {
                MessageBox.Show("✅ Bouton de test cliqué !");
                vm.SelectFile();
            }
            else
            {
                MessageBox.Show("❌ DataContext introuvable !");
            }
        }
    }
}
