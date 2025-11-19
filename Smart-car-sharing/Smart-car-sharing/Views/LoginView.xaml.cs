using System;
using System.Windows;
using System.Windows.Controls;

namespace SmartCarSharingApp.UI.Views
{
    /// <summary>
    /// Логіка взаємодії для LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Отримання даних
            string email = EmailBox.Text;
            string password = PasswordBox.Password;

            // TODO: 1. Валідація полів (перевірка на пусті значення)
            // TODO: 2. Додати 'async' та викликати await _authService.LoginUserAsync(email, password)
            // TODO: 3. Якщо успішно -> відкрити Dashboard / MainView

            MessageBox.Show($"Спроба входу для: {email}", "Інфо", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NavigateToRegister_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реалізувати навігацію на RegisterView
            MessageBox.Show("Перехід до реєстрації.", "Навігація", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}