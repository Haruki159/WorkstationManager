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
using System.Windows.Shapes;
using WorkstationManager.DataBaseFolder;

namespace WorkstationManager.WindowsFolder
{
    /// <summary>
    /// Логика взаимодействия для ComputerEditWindow.xaml
    /// </summary>
    public partial class ComputerEditWindow : Window
    {
        private Computer _currentComputer;
        private readonly WorkstationManagerDB_v2Entities _context;

        public ComputerEditWindow(Computer computer)
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
                // Загружаем данные для выпадающих списков
                EmployeeComboBox.ItemsSource = _context.Employees.ToList();
                ProfileComboBox.ItemsSource = _context.ConfigurationProfiles.ToList();
                StatusComboBox.ItemsSource = _context.ComputerStatuses.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
        }

        //private void SaveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(_currentComputer.Name) || _currentComputer.StatusID == 0)
        //    {
        //        MessageBox.Show("Поля 'Имя компьютера' и 'Статус' обязательны для заполнения.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    try
        //    {
        //        if (_currentComputer.ComputerID == 0) // Новая запись
        //        {
        //            _context.Computers.Add(_currentComputer);
        //        }

        //        _context.SaveChanges();
        //        // Логирование действия (опционально, но хорошая практика)
        //        string action = _currentComputer.ComputerID == 0 ? "создан" : "изменен";
        //        _context.ActionLogs.Add(new ActionLog
        //        {
        //            ComputerID = _currentComputer.ComputerID,
        //            UserID = 1, // Здесь должен быть ID текущего залогиненного пользователя
        //            LogText = $"Компьютер '{_currentComputer.Name}' был {action}."
        //        });
        //        _context.SaveChanges();

        //        this.DialogResult = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация
            if (string.IsNullOrWhiteSpace(NameTextBox.Text)) // Обращаемся напрямую к контролам
            {
                MessageBox.Show("Имя компьютера не может быть пустым.", "Ошибка валидации");
                return;
            }
            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать статус компьютера.", "Ошибка валидации");
                return;
            }

            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                string action; // Для текста лога

                // 2. Логика: Добавление ИЛИ Редактирование
                if (_currentComputer.ComputerID == 0) // --- Сценарий: ДОБАВЛЕНИЕ НОВОГО ПК ---
                {
                    action = "создан";

                    // Создаем новый объект Computer
                    var newComputer = new Computer();

                    // Заполняем его данными из контролов окна
                    newComputer.Name = NameTextBox.Text;
                    newComputer.InventoryNumber = InventoryNumberTextBox.Text;
                    newComputer.Manufacturer = ManufacturerTextBox.Text;
                    newComputer.Model = ModelTextBox.Text;
                    newComputer.SerialNumber = SerialNumberTextBox.Text;
                    newComputer.EmployeeID = (int?)EmployeeComboBox.SelectedValue > 0 ? (int?)EmployeeComboBox.SelectedValue : null;
                    newComputer.ProfileID = (int?)ProfileComboBox.SelectedValue > 0 ? (int?)ProfileComboBox.SelectedValue : null;
                    newComputer.StatusID = (int)StatusComboBox.SelectedValue;

                    // Добавляем новый объект в контекст
                    context.Computers.Add(newComputer);

                    // Сохраняем, чтобы получить ID для лога
                    context.SaveChanges();

                    // Присваиваем ID для записи в лог
                    _currentComputer.ComputerID = newComputer.ComputerID;
                }
                else // --- Сценарий: РЕДАКТИРОВАНИЕ СУЩЕСТВУЮЩЕГО ПК ---
                {
                    action = "изменен";

                    // Находим существующую запись в базе данных по ID
                    var computerToUpdate = context.Computers.Find(_currentComputer.ComputerID);
                    if (computerToUpdate == null)
                    {
                        MessageBox.Show("Компьютер не найден в базе данных. Возможно, он был удален.", "Ошибка");
                        return;
                    }

                    // Обновляем поля найденного объекта данными из контролов
                    computerToUpdate.Name = NameTextBox.Text;
                    computerToUpdate.InventoryNumber = InventoryNumberTextBox.Text;
                    computerToUpdate.Manufacturer = ManufacturerTextBox.Text;
                    computerToUpdate.Model = ModelTextBox.Text;
                    computerToUpdate.SerialNumber = SerialNumberTextBox.Text;
                    computerToUpdate.EmployeeID = (int?)EmployeeComboBox.SelectedValue > 0 ? (int?)EmployeeComboBox.SelectedValue : null;
                    computerToUpdate.ProfileID = (int?)ProfileComboBox.SelectedValue > 0 ? (int?)ProfileComboBox.SelectedValue : null;
                    computerToUpdate.StatusID = (int)StatusComboBox.SelectedValue;

                    // Сообщаем EF, что объект был изменен
                    context.Entry(computerToUpdate).State = EntityState.Modified;

                    // Сохраняем изменения
                    context.SaveChanges();
                }

                // 3. Логирование (вынесено в отдельный блок)
                var currentUser = context.Users.FirstOrDefault(u => u.Login == "admin") ?? context.Users.FirstOrDefault();
                if (currentUser != null)
                {
                    var logEntry = new ActionLog
                    {
                        ComputerID = _currentComputer.ComputerID,
                        UserID = currentUser.UserID,
                        ActionDate = DateTime.Now,
                        LogText = $"Компьютер '{NameTextBox.Text}' был {action}."
                    };
                    context.ActionLogs.Add(logEntry);
                    context.SaveChanges(); // Сохраняем лог
                }

                // 4. Успешное завершение
                MessageBox.Show("Данные успешно сохранены!", "Успех");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                var innerEx = ex;
                while (innerEx.InnerException != null) innerEx = innerEx.InnerException;
                MessageBox.Show($"Ошибка сохранения: {innerEx.Message}", "Ошибка");
            }
        }
    }
}
