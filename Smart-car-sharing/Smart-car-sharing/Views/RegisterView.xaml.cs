using SmartCarSharing.Core.Services;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using SmartCarSharingApp.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartCarSharingApp.UI.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();


            var context = new AppDbContext();
            var authService = new AuthenticationService(context);
            var viewModel = new RegisterViewModel(authService);

            this.DataContext = viewModel;

            viewModel.RequestNavigateToLogin += () =>
            {
                MessageBox.Show("Реєстрація успішна! Перехід на сторінку входу.", "Успіх");
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }

        private void NavigateToLogin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Перехід до входу.", "Навігація");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}