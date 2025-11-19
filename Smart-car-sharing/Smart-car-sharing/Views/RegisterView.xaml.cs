using System;
using System.Windows;
using System.Windows.Controls;

namespace SmartCarSharingApp.UI.Views
{
    /// <summary>
    /// Логіка взаємодії для RegisterView.xaml
    /// </summary>
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Отримання даних з полів
            string name = NameBox.Text;
            string email = EmailBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // TODO: 1. Перевірити валідацію (чи співпадають паролі, чи не пусті поля)
            // TODO: 2. Додати 'async' до сигнатури методу та викликати await IAuthenticationService.RegisterUserAsync(...)
            // TODO: 3. Обробити результат (успіх або помилка)

            MessageBox.Show("Функціонал реєстрації буде реалізовано пізніше.", "Інфо", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NavigateToLogin_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реалізувати навігацію назад до вікна входу
            MessageBox.Show("Перехід до вікна входу.", "Навігація", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}