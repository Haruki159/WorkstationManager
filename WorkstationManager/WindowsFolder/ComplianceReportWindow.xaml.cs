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
using WorkstationManager.ClassFolder;
using WorkstationManager.DataBaseFolder;

namespace WorkstationManager.WindowsFolder
{
    public class ComplianceReportItem
    {
        public string SoftwareName { get; set; }
        public bool IsRequired { get; set; }
        public bool IsInstalled { get; set; } // Для ручного заполнения это поле было бы редактируемым
        public string IsRequiredText => IsRequired ? "Да" : "Нет";

        public enum ComplianceStatus { OK, Missing, Extra }
        public ComplianceStatus Status { get; set; }
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case ComplianceStatus.OK: return "Соответствует";
                    case ComplianceStatus.Missing: return "Отсутствует";
                    case ComplianceStatus.Extra: return "Лишнее";
                    default: return "";
                }
            }
        }
    }
    /// <summary>
    /// Логика взаимодействия для ComplianceReportWindow.xaml
    /// </summary>
    public partial class ComplianceReportWindow : Window
    {
        private readonly int _computerId;
        private readonly WorkstationManagerDB_v2Entities _context;

        public ComplianceReportWindow(int computerId)
        {
            InitializeComponent();
            _computerId = computerId;
            _context = WorkstationManagerDB_v2Entities.GetContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void GenerateReport()
        {
            try
            {
                // 1. Загружаем компьютер, его профиль и все ПО этого профиля
                var computer = _context.Computers
                    .Include("ConfigurationProfile.Softwares")
                    .FirstOrDefault(c => c.ComputerID == _computerId);

                if (computer == null || computer.ConfigurationProfile == null)
                {
                    MessageBox.Show("Компьютер или его профиль не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                ReportTitleTextBlock.Text = $"Отчет для компьютера: {computer.Name}";
                ReportSubtitleTextBlock.Text = $"Профиль: {computer.ConfigurationProfile.Name}";

                // 2. Получаем список ПО, которое ТРЕБУЕТСЯ по профилю
                var requiredSoftware = computer.ConfigurationProfile.Softwares.ToList();

                // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
                // 3. Получаем список ПО, которое ФАКТИЧЕСКИ УСТАНОВЛЕНО на ЭТОМ КОНКРЕТНОМ компьютере
                //    Мы запрашиваем данные из таблицы InstalledSoftware.
                var installedSoftware = _context.InstalledSoftwares
                                                .Where(i => i.ComputerID == _computerId && i.IsInstalled)
                                                .Select(i => i.Software) // Нас интересуют сами объекты Software
                                                .ToList();

                // 4. Формируем итоговый список для отчета
                var reportItems = new List<ComplianceReportItem>();

                // Объединяем оба списка (требуемого и установленного), чтобы не пропустить ни одной программы
                var allMentionedSoftware = requiredSoftware.Union(installedSoftware, new SoftwareComparer())
                                                         .Distinct(new SoftwareComparer());

                foreach (var software in allMentionedSoftware)
                {
                    var item = new ComplianceReportItem
                    {
                        SoftwareName = software.Name,
                        // Проверяем, есть ли это ПО в списке требуемых
                        IsRequired = requiredSoftware.Any(s => s.SoftwareID == software.SoftwareID),
                        // Проверяем, есть ли это ПО в списке фактически установленных
                        IsInstalled = installedSoftware.Any(s => s.SoftwareID == software.SoftwareID)
                    };

                    // Определяем статус на основе этих двух флагов
                    if (item.IsRequired && item.IsInstalled)
                        item.Status = ComplianceReportItem.ComplianceStatus.OK;
                    else if (item.IsRequired && !item.IsInstalled)
                        item.Status = ComplianceReportItem.ComplianceStatus.Missing;
                    else if (!item.IsRequired && item.IsInstalled)
                        item.Status = ComplianceReportItem.ComplianceStatus.Extra;
                    else // На всякий случай (не требуется и не установлено)
                    {
                        // Такие строки можно не показывать, но для полноты картины оставим
                        continue; // Пропускаем ПО, которое не требуется и не установлено
                    }

                    reportItems.Add(item);
                }

                ReportListView.ItemsSource = reportItems.OrderBy(i => i.SoftwareName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
