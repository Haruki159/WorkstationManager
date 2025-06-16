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
    /// Логика взаимодействия для ComputersPage.xaml
    /// </summary>
    public partial class ComputersPage : Page
    {
        public ComputersPage()
        {
            InitializeComponent();
            ComputersDataGrid.ItemsSource = WorkstationManagerDB_v2Entities.GetContext().Computers.ToList();
        }

        private void LoadComputers()
        {
            try
            {
                // Получаем единственный экземпляр контекста через ваш метод
                var context = WorkstationManagerDB_v2Entities.GetContext();

                // Загружаем компьютеры, включая связанные данные.
                // Include() работает так же, как в EF Core.
                var computers = context.Computers
                    .Include(c => c.Employee)
                    .Include(c => c.ConfigurationProfile) // Убедитесь, что навигационное свойство называется именно так
                    .Include(c => c.ComputerStatus)     // Аналогично
                    .ToList();

                ComputersDataGrid.ItemsSource = computers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Передаем новый, пустой объект
            var editWindow = new ComputerEditWindow(new Computer());
            if (editWindow.ShowDialog() == true)
            {
                LoadComputers(); // Обновляем DataGrid, если сохранение прошло успешно
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedComputer = ComputersDataGrid.SelectedItem as Computer;
            if (selectedComputer == null)
            {
                MessageBox.Show("Пожалуйста, выберите компьютер для редактирования.", "Внимание");
                return;
            }

            // Передаем выбранный объект
            var editWindow = new ComputerEditWindow(selectedComputer);
            if (editWindow.ShowDialog() == true)
            {
                LoadComputers(); // Обновляем DataGrid
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedComputer = ComputersDataGrid.SelectedItem as Computer;
            if (selectedComputer == null)
            {
                MessageBox.Show("Пожалуйста, выберите компьютер для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"Вы уверены, что хотите удалить компьютер '{selectedComputer.Name}'?", 
                                         "Подтверждение удаления", 
                                         MessageBoxButton.YesNo, 
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    WorkstationManagerDB_v2Entities.GetContext().Computers.Remove(selectedComputer);
                    WorkstationManagerDB_v2Entities.GetContext().SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Получаем выбранный компьютер из таблицы
            var selectedComputer = ComputersDataGrid.SelectedItem as Computer;

            // 2. Проверяем, выбрал ли пользователь что-нибудь
            if (selectedComputer == null)
            {
                MessageBox.Show("Пожалуйста, выберите компьютер для проверки.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Проверяем, назначен ли компьютеру профиль
            // (Без профиля отчет не имеет смысла)
            if (selectedComputer.ProfileID == null)
            {
                MessageBox.Show($"У компьютера '{selectedComputer.Name}' не назначен профиль конфигурации. Сравнение невозможно.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 4. Создаем экземпляр окна отчета, передавая ему ID выбранного компьютера
                var reportWindow = new ComplianceReportWindow(selectedComputer.ComputerID);

                // 5. Открываем окно как диалоговое (модальное)
                reportWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть окно отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadComputers();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            var context = WorkstationManagerDB_v2Entities.GetContext();

            var query = context.Computers
                .Include(c => c.Employee)
                .Include(c => c.ConfigurationProfile)
                .Include(c => c.ComputerStatus)
                .AsQueryable(); // Преобразуем в IQueryable для построения динамического запроса

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchText) ||
                    (c.InventoryNumber != null && c.InventoryNumber.ToLower().Contains(searchText)) ||
                    (c.Employee != null && c.Employee.FullName.ToLower().Contains(searchText)) ||
                    (c.ConfigurationProfile != null && c.ConfigurationProfile.Name.ToLower().Contains(searchText))
                );
            }

            ComputersDataGrid.ItemsSource = query.ToList();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComputers();
            ApplyAccessRights();
        }

        private void ApplyAccessRights()
        {
            // Проверяем, является ли текущий пользователь администратором
            bool isAdmin = CurrentUser.IsAdmin;

            // Скрываем или показываем кнопки в зависимости от роли
            // Visibility.Collapsed - элемент не виден и не занимает место
            AddButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
