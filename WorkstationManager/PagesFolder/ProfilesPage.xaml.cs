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
    /// Логика взаимодействия для ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage : Page
    {
        public ProfilesPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyAccessRights();
            LoadProfiles();
        }

        private void ApplyAccessRights()
        {
            bool isAdmin = CurrentUser.IsAdmin;
            AddButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadProfiles()
        {
            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                ProfilesDataGrid.ItemsSource = context.ConfigurationProfiles.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профилей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Получаем объект, выбранный в DataGrid
            var selectedProfileFromGrid = ProfilesDataGrid.SelectedItem as ConfigurationProfile;

            if (selectedProfileFromGrid == null)
            {
                // Если ничего не выбрано, очищаем панель деталей
                SelectedProfileGroupBox.Header = "Детали профиля";
                SoftwareInProfileListBox.ItemsSource = null;
                return;
            }

            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                var actualProfile = context.ConfigurationProfiles
                                           .Include(p => p.Softwares) // Явно говорим EF подгрузить коллекцию Softwares
                                           .FirstOrDefault(p => p.ProfileID == selectedProfileFromGrid.ProfileID);

                // 3. Если по какой-то причине профиль не найден в базе, выходим.
                if (actualProfile == null)
                {
                    MessageBox.Show("Не удалось найти выбранный профиль в базе данных.", "Ошибка");
                    return;
                }

                // 4. Обновляем заголовок и список ПО, используя АКТУАЛЬНЫЕ данные
                SelectedProfileGroupBox.Header = $"ПО для профиля: {actualProfile.Name}";

                // Берем список ПО из свежезагруженного объекта
                var softwareList = actualProfile.Softwares.OrderBy(s => s.Name).ToList();

                SoftwareInProfileListBox.ItemsSource = softwareList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ПО для профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ProfileEditWindow(new ConfigurationProfile());
            if (editWindow.ShowDialog() == true) { LoadProfiles(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = ProfilesDataGrid.SelectedItem as ConfigurationProfile;
            if (selectedProfile == null)
            {
                MessageBox.Show("Пожалуйста, выберите профиль для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new ProfileEditWindow(selectedProfile);
            if (editWindow.ShowDialog() == true)
            {
                LoadProfiles();
                // Обновляем детали для выбранного профиля
                ProfilesDataGrid_SelectionChanged(null, null);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = ProfilesDataGrid.SelectedItem as ConfigurationProfile;
            if (selectedProfile == null)
            {
                MessageBox.Show("Пожалуйста, выберите профиль для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить профиль '{selectedProfile.Name}'?\n\nКомпьютеры, использующие этот профиль, станут 'без профиля'.",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = WorkstationManagerDB_v2Entities.GetContext();

                    // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
                    // Используем правильное имя ключа, скорее всего 'ID'
                    var profileToDelete = context.ConfigurationProfiles.Find(selectedProfile);

                    if (profileToDelete != null)
                    {
                        context.ConfigurationProfiles.Remove(profileToDelete);
                        context.SaveChanges();
                        LoadProfiles();
                    }
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
            var context = WorkstationManagerDB_v2Entities.GetContext();

            var query = context.ConfigurationProfiles
                .Include(c => c.Name)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c => c.Name.ToLower().Contains(searchText));
            }

            ProfilesDataGrid.ItemsSource = WorkstationManagerDB_v2Entities.GetContext()
                .ConfigurationProfiles.Where(
                p => p.Name.ToLower()
                .Contains(searchText))
                .ToList();
        }
    }
}
