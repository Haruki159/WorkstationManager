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
    /// Логика взаимодействия для SoftwareEditWindow.xaml
    /// </summary>
    public partial class SoftwareEditWindow : Window
    {
        private Software _currentSoftware;

        public SoftwareEditWindow(Software software)
        {
            InitializeComponent();
            _currentSoftware = software;
            this.DataContext = _currentSoftware;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Простая валидация
            if (string.IsNullOrWhiteSpace(_currentSoftware.Name))
            {
                MessageBox.Show("Поле 'Название ПО' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();

                // Проверка на уникальность имени
                if (context.Softwares.Any(s => s.Name == _currentSoftware.Name && s.SoftwareID != _currentSoftware.SoftwareID))
                {
                    MessageBox.Show("Программное обеспечение с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_currentSoftware.SoftwareID == 0) // Если ID=0, это новая запись
                {
                    context.Softwares.Add(_currentSoftware);
                }

                context.SaveChanges();
                this.DialogResult = true; // Устанавливаем результат, чтобы родительское окно знало об успехе
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
        }
    }
}
