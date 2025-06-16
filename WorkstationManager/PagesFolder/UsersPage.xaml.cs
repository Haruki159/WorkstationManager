using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkstationManager.DataBaseFolder;
using WorkstationManager.ServiceFolder;
using WorkstationManager.WindowsFolder;

namespace WorkstationManager.PagesFolder
{
    /// <summary>
    /// Логика взаимодействия для UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            ApplyAccessRights();
        }

        private void ApplyAccessRights()
        {
            bool isAdmin = CurrentUser.IsAdmin;
            AddButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadUsers()
        {
            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                // Обязательно подгружаем связанную сущность Role, чтобы отобразить RoleName
                UsersListView.ItemsSource = context.Users.Include(u => u.Role).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditUserWindow(new User());
            if (editWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersListView.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EditUserWindow(selectedUser);
            if (editWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersListView.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Защита от удаления основного администратора
            if (selectedUser.Login.ToLower() == "admin")
            {
                MessageBox.Show("Основного администратора нельзя удалить.", "Действие запрещено", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{selectedUser.Login}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = WorkstationManagerDB_v2Entities.GetContext();
                    context.Users.Remove(selectedUser);
                    context.SaveChanges();
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                UsersListView.ItemsSource = context.Users.Include(u => u.Role)
                    .Where(u => u.Login.ToLower().Contains(searchText) ||
                                u.FullName.ToLower().Contains(searchText) ||
                                u.Role.RoleName.ToLower().Contains(searchText))
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadUsers();
        }
    }
}
