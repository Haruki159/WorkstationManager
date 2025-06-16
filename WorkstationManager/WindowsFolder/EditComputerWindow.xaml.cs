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
    /// Interaction logic for EditComputerWindow.xaml
    /// </summary>
    public partial class EditComputerWindow : Window
    {
        private Computer _currentComputer;
        private readonly WorkstationManagerDB_v2Entities _context;

        public EditComputerWindow(Computer computer)
        {
            InitializeComponent();
            _context = WorkstationManagerDB_v2Entities.GetContext();
            _currentComputer = computer;
            this.DataContext = _currentComputer;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загружаем данные для всех выпадающих списков
                // Добавляем к каждому списку "пустой" элемент, чтобы можно было снять выбор
                var employees = _context.Employees.OrderBy(emp => emp.FullName).ToList();
                employees.Insert(0, new Employee { EmployeeID = 0, FullName = " (не выбрано)" });
                EmployeeComboBox.ItemsSource = employees;

                var profiles = _context.ConfigurationProfiles.OrderBy(p => p.Name).ToList();
                profiles.Insert(0, new ConfigurationProfile { ProfileID = 0, Name = " (не выбрано)" }); // Убедитесь, что имя ключа ProfileID
                ProfileComboBox.ItemsSource = profiles;

                StatusComboBox.ItemsSource = _context.ComputerStatuses.ToList();

                // Устанавливаем значения по умолчанию, если это новый компьютер
                if (_currentComputer.ComputerID == 0)
                {
                    // Например, по умолчанию ставим статус "На складе"
                    var defaultStatus = _context.ComputerStatuses.FirstOrDefault(s => s.StatusName == "На складе");
                    if (defaultStatus != null)
                    {
                        _currentComputer.StatusID = defaultStatus.StatusID;
                    }
                }

                // Корректно обрабатываем nullable значения для ComboBox
                // Если у компьютера не выбран сотрудник, EmployeeID будет null.
                // Чтобы ComboBox показал "не выбрано", мы должны присвоить ему 0.
                if (_currentComputer.EmployeeID == null)
                {
                    EmployeeComboBox.SelectedValue = 0;
                }
                if (_currentComputer.ProfileID == null)
                {
                    ProfileComboBox.SelectedValue = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(_currentComputer.Name) || _currentComputer.StatusID == 0)
            {
                MessageBox.Show("Поля 'Имя компьютера' и 'Статус' обязательны для заполнения.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Обрабатываем "пустые" значения из ComboBox
                // Если выбрано "(не выбрано)" (ID=0), то в базу нужно записать NULL
                if (_currentComputer.EmployeeID == 0)
                {
                    _currentComputer.EmployeeID = null;
                }
                if (_currentComputer.ProfileID == 0)
                {
                    _currentComputer.ProfileID = null;
                }

                if (_currentComputer.ComputerID == 0) // Если ID=0, это новая запись
                {
                    _context.Computers.Add(_currentComputer);
                }

                _context.SaveChanges();

                // Логирование действия
                string action = _currentComputer.ComputerID == 0 ? "создан" : "изменен";
                var currentUser = _context.Users.FirstOrDefault(); // Временно берем первого пользователя
                _context.ActionLogs.Add(new ActionLog
                {
                    ComputerID = _currentComputer.ComputerID,
                    UserID = currentUser?.UserID ?? 1, // В реальном приложении здесь будет ID залогиненного пользователя
                    LogText = $"Компьютер '{_currentComputer.Name}' был {action}."
                });
                _context.SaveChanges();

                this.DialogResult = true; // Сигнализируем родительскому окну об успехе
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
