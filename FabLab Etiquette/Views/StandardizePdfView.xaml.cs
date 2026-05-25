using FabLab_Etiquette.Models;
using FabLab_Etiquette.ViewModels;
using System.Windows;

namespace FabLab_Etiquette.Views
{
    public partial class StandardizePdfView : Window
    {
        public StandardizePdfView(string sourcePath, UserInfo userInfo, FabricationParams fabParams)
        {
            InitializeComponent();
            DataContext = new StandardizePdfViewModel(sourcePath, userInfo, fabParams);
        }
    }
}
