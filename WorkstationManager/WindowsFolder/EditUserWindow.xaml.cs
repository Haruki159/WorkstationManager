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

namespace WorkstationManager.WindowsFolder
{
    /// <summary>
    /// Логика взаимодействия для EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {
        private User _currentUser;

        public EditUserWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            this.DataContext = _currentUser;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем список ролей в ComboBox
            try
            {
                RoleComboBox.ItemsSource = WorkstationManagerDB_v2Entities.GetContext().Roles.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }

            // Если это новый пользователь, сделаем его активным по умолчанию
            if (_currentUser.UserID == 0)
            {
                _currentUser.IsActive = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(_currentUser.Login) ||
                string.IsNullOrWhiteSpace(_currentUser.FullName) ||
                RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Все поля, отмеченные (*), должны быть заполнены.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обработка пароля
            if (_currentUser.UserID == 0) // Для нового пользователя пароль обязателен
            {
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Пароль обязателен для нового пользователя.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _currentUser.Password = PasswordBox.Password;
            }
            else // Для существующего пользователя обновляем пароль, только если он введен
            {
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _currentUser.Password = PasswordBox.Password;
                }
            }

            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();

                // Проверка на уникальность логина
                if (context.Users.Any(u => u.Login == _currentUser.Login && u.UserID != _currentUser.UserID))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_currentUser.UserID == 0)
                {
                    context.Users.Add(_currentUser);
                }

                context.SaveChanges();
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
        }
    }
}
