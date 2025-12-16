using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartCarSharingApp.UI.Views
{
    /// <summary>
    /// Логіка взаємодії для CarCard.xaml
    /// Відображає дані одного автомобіля.
    /// </summary>
    public partial class CarCard : UserControl
    {
        public CarCard()
        {
            InitializeComponent();
        }

        // --- Додаємо можливість прив'язки команди (Command) ззовні ---

        // Це DependencyProperty дозволяє писати в XAML: <views:CarCard Command="{Binding ViewDetailsCommand}" ... />
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(CarCard), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Це дозволяє передавати параметр у команду (сама машина)
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(CarCard), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Обробник натискання кнопки "Деталі"
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            // Якщо команда задана (через Binding), виконуємо її
            if (Command != null && Command.CanExecute(CommandParameter ?? DataContext))
            {
                Command.Execute(CommandParameter ?? DataContext);
            }
            else
            {
                // Якщо команда не задана, просто для тесту покажемо повідомлення
                MessageBox.Show("Натиснуто кнопку деталей. Логіка навігації ще не підключена.", "Інфо");
            }
        }
    }
}