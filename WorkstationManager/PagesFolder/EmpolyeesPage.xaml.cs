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
    /// Логика взаимодействия для EmpolyeesPage.xaml
    /// </summary>
    public partial class EmpolyeesPage : Page
    {
        public EmpolyeesPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
            ApplyAccessRights();
        }

        private void ApplyAccessRights()
        {
            bool isAdmin = CurrentUser.IsAdmin;
            AddButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadEmployees()
        {
            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                EmployeesDataGrid.ItemsSource = context.Employees.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EmployeeEditWindow(new Employee());
            if (editWindow.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeesDataGrid.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EmployeeEditWindow(selectedEmployee);
            if (editWindow.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeesDataGrid.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить сотрудника '{selectedEmployee.FullName}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = WorkstationManagerDB_v2Entities.GetContext();

                    // Проверяем, закреплены ли за сотрудником компьютеры
                    bool hasComputers = context.Computers.Any(c => c.EmployeeID == selectedEmployee.EmployeeID);
                    if (hasComputers)
                    {
                        MessageBox.Show("За этим сотрудником закреплены компьютеры. Сначала снимите с него все устройства.", "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    context.Employees.Remove(selectedEmployee);
                    context.SaveChanges();
                    LoadEmployees();
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
                EmployeesDataGrid.ItemsSource = context.Employees
                    .Where(emp => emp.FullName.ToLower().Contains(searchText) ||
                                  (emp.Department != null && emp.Department.ToLower().Contains(searchText)) ||
                                  (emp.Position != null && emp.Position.ToLower().Contains(searchText)))
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
            LoadEmployees();
        }
    }
}
