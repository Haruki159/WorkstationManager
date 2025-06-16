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
using WorkstationManager.DataBaseFolder;
using WorkstationManager.ServiceFolder;

namespace WorkstationManager.WindowsFolder
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PassBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                // Находим пользователя в базе данных
                var user = context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user != null)
                {
                    if (user.IsActive == false)
                    {
                        MessageBox.Show("Ваша учетная запись деактивирована.", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // --- ИЗМЕНЕНИЕ ЗДЕСЬ ---
                    // Сохраняем пользователя в нашем статическом классе
                    CurrentUser.Login(user);

                    // Открываем главное окно
                    AppWindow mainWindow = new AppWindow();
                    mainWindow.Show();
                    this.Close(); // Закрываем окно авторизации
                }
                else
                {
                    MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
