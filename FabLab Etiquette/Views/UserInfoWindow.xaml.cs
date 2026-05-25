using FabLab_Etiquette.Models;
using System.Windows;

namespace FabLab_Etiquette.Views
{
    public partial class UserInfoWindow : Window
    {
        public string UserName { get; private set; } = "";
        public string UserService { get; private set; } = "";
        public string UserNumber { get; private set; } = "";

        public UserInfoWindow()
        {
            InitializeComponent();
        }

        public UserInfo GetUserInfo() => new UserInfo
        {
            Name = UserName,
            Service = UserService,
            Number = UserNumber
        };

        private void OnValidateClick(object sender, RoutedEventArgs e)
        {
            UserName = UserNameTextBox.Text.Trim();
            UserService = UserServiceTextBox.Text.Trim();
            UserNumber = UserNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(UserService) ||
                string.IsNullOrWhiteSpace(UserNumber))
            {
                MessageBox.Show("Tous les champs doivent être remplis.", "Champs requis",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
