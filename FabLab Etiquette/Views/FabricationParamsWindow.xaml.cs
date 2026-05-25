using FabLab_Etiquette.ViewModels;
using System.Windows;

namespace FabLab_Etiquette.Views
{
    public partial class FabricationParamsWindow : Window
    {
        public FabricationParamsWindow()
        {
            InitializeComponent();
            var vm = new FabricationParamsViewModel();
            vm.RequestClose += (s, result) =>
            {
                DialogResult = result;
                Close();
            };
            DataContext = vm;
        }

        public FabricationParamsViewModel? ViewModel => DataContext as FabricationParamsViewModel;
    }
}
