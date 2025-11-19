using System.Windows;
using System.Windows.Controls;
using SmartCarSharingApp.UI.ViewModels;

namespace SmartCarSharingApp.UI.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        // Передача пароля у ViewModel
        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }

        private void NavigateToRegister_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Перехід до реєстрації.", "Навігація", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
