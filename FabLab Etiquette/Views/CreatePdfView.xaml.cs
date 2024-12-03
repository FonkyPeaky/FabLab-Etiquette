using FabLab_Etiquette.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FabLab_Etiquette.Views
{
    /// <summary>
    /// Interaction logic for CreatePdfView.xaml
    /// </summary>
    public partial class CreatePdfView : Window
    {
        public CreatePdfView()
        {
            InitializeComponent();
            DataContext = new CreatePdfViewModel();
        }
    }
}
