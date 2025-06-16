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
using WorkstationManager.WindowsFolder;

namespace WorkstationManager.PagesFolder
{
    /// <summary>
    /// Логика взаимодействия для ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComputersList();
        }

        // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
        private void LoadComputersList()
        {
            try
            {
                var context = WorkstationManagerDB_v2Entities.GetContext();
                var computers = context.Computers.OrderBy(c => c.Name).ToList(); // <-- Поставьте точку останова на следующей строке
                ComputerSelectionComboBox.ItemsSource = computers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка компьютеров: {ex.Message}");
            }
        }

        private void RefreshComputersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadComputersList();
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComputerSelectionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите компьютер.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedComputer = ComputerSelectionComboBox.SelectedItem as Computer;

            if (selectedComputer.ProfileID == null)
            {
                MessageBox.Show($"У компьютера '{selectedComputer.Name}' не назначен профиль конфигурации. Сравнение невозможно.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var reportWindow = new ComplianceReportWindow(selectedComputer.ComputerID);
            reportWindow.ShowDialog();
        }
    }
}
