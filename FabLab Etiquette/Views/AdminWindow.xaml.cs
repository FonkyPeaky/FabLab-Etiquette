using FabLab_Etiquette.ViewModels;
using System.Windows;

namespace FabLab_Etiquette.Views
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            DataContext = new AdminViewModel();
        }
    }
}
