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
    /// Логика взаимодействия для EmployeeEditWindow.xaml
    /// </summary>
    public partial class EmployeeEditWindow : Window
    {
        private Employee _currentEmployee;
        public EmployeeEditWindow(Employee employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            this.DataContext = _currentEmployee;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(_currentEmployee.FullName))
            {
                MessageBox.Show("Поле 'ФИО' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();

                // Проверка на уникальность ФИО (по желанию, может быть не нужна)
                if (context.Employees.Any(emp => emp.FullName == _currentEmployee.FullName && emp.EmployeeID != _currentEmployee.EmployeeID))
                {
                    MessageBox.Show("Сотрудник с таким ФИО уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_currentEmployee.EmployeeID == 0) // Новая запись
                {
                    context.Employees.Add(_currentEmployee);
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
