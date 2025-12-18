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

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }

        private void NavigateToRegister_Click(object sender, RoutedEventArgs e) { }
    }
}