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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkstationManager.DataBaseFolder;
using WorkstationManager.ServiceFolder;
using WorkstationManager.WindowsFolder;

namespace WorkstationManager.PagesFolder
{
    /// <summary>
    /// Логика взаимодействия для SoftwarePage.xaml
    /// </summary>
    public partial class SoftwarePage : Page
    {
        public SoftwarePage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSoftware();
            ApplyAccessRights();
        }
        private void ApplyAccessRights()
        {
            bool isAdmin = CurrentUser.IsAdmin;
            AddButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadSoftware()
        {
            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                SoftwareDataGrid.ItemsSource = context.Softwares.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новое пустое окно для добавления
            var editWindow = new SoftwareEditWindow(new Software());
            if (editWindow.ShowDialog() == true)
            {
                LoadSoftware(); // Обновляем список, если добавили новую запись
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSoftware = SoftwareDataGrid.SelectedItem as Software;
            if (selectedSoftware == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Передаем выбранную запись в окно редактирования
            var editWindow = new SoftwareEditWindow(selectedSoftware);
            if (editWindow.ShowDialog() == true)
            {
                LoadSoftware(); // Обновляем список
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSoftware = SoftwareDataGrid.SelectedItem as Software;
            if (selectedSoftware == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить '{selectedSoftware.Name}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = WorkstationManagerDB_v2Entities.GetContext();

                    // Проверяем, используется ли это ПО в каких-либо профилях
                    bool isInProfile = context.ConfigurationProfiles.Any(p => p.Softwares.Any(s => s.SoftwareID == selectedSoftware.SoftwareID));
                    if (isInProfile)
                    {
                        MessageBox.Show("Это ПО используется в одном или нескольких профилях. Сначала удалите его из всех профилей.", "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
                    // 1. Находим сущность в текущем контексте по ее ID.
                    var softwareToDelete = context.Softwares.Find(selectedSoftware.SoftwareID);

                    // 2. Если сущность найдена, удаляем именно ее.
                    if (softwareToDelete != null)
                    {
                        context.Softwares.Remove(softwareToDelete);
                        context.SaveChanges();
                        LoadSoftware(); // Обновляем список
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
            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                SoftwareDataGrid.ItemsSource = context.Softwares
                    .Where(s => s.Name.ToLower().Contains(searchText) ||
                                (s.Publisher != null && s.Publisher.ToLower().Contains(searchText)))
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
            LoadSoftware();
        }
    }
}
