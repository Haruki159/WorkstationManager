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
using WorkstationManager.ClassFolder;
using WorkstationManager.DataBaseFolder;

namespace WorkstationManager.WindowsFolder
{
    /// <summary>
    /// Логика взаимодействия для ProfileEditWindow.xaml
    /// </summary>
    public partial class ProfileEditWindow : Window
    {
        private ConfigurationProfile _currentProfile;
        private readonly WorkstationManagerDB_v2Entities _context;

        // Временно храним список установленного ПО для всех ПК с этим профилем
        private List<InstalledSoftware> _installedSoftwareState;

        public ProfileEditWindow(ConfigurationProfile profile)
        {
            InitializeComponent();
            _context = WorkstationManagerDB_v2Entities.GetContext();

            if (profile.ProfileID == 0)
            {
                _currentProfile = profile;
            }
            else
            {
                _currentProfile = _context.ConfigurationProfiles
                                          .Include(p => p.Softwares)
                                          .FirstOrDefault(p => p.ProfileID == profile.ProfileID);
            }

            this.DataContext = _currentProfile;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSoftwareLists();
        }

        private void LoadSoftwareLists()
        {
            try
            {
                var allSoftware = _context.Softwares.OrderBy(s => s.Name).ToList();

                // ПО, которое уже есть в профиле
                var profileSoftwareList = _currentProfile.Softwares.ToList();

                // Получаем информацию о том, какое ПО установлено на компьютерах с этим профилем
                // ВАЖНО: Это усредненный статус. Если на одном ПК установлено, а на другом нет,
                // мы будем считать, что установлено, если установлено хотя бы на одном.
                _installedSoftwareState = _context.InstalledSoftwares
                    .Where(i => i.Computer.ProfileID == _currentProfile.ProfileID && i.IsInstalled)
                    .ToList();

                // --- Создаем ViewModel для правой колонки ---
                var profileSoftwareViewModels = profileSoftwareList.Select(s => new ProfileSoftwareViewModel
                {
                    Software = s,
                    // IsInstalled будет true, если есть хотя бы одна запись в _installedSoftwareState для этого ПО
                    IsInstalled = _installedSoftwareState.Any(i => i.SoftwareID == s.SoftwareID)
                }).OrderBy(vm => vm.Name).ToList();

                // Привязываем ViewModel к новому ListView
                ProfileSoftwareListView.ItemsSource = profileSoftwareViewModels;

                // Доступное ПО - это все ПО минус то, что уже в профиле
                var availableSoftware = allSoftware.Except(profileSoftwareList, new SoftwareComparer());
                AvailableSoftwareListBox.ItemsSource = availableSoftware.OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списков ПО: {ex.Message}", "Ошибка");
            }
        }

        // Логика кнопок Add/Remove теперь должна работать с ViewModel
        private void AddSoftwareButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = AvailableSoftwareListBox.SelectedItems.Cast<Software>().ToList();
            if (!selectedItems.Any()) return;

            var available = AvailableSoftwareListBox.ItemsSource as List<Software>;
            var inProfileVM = ProfileSoftwareListView.ItemsSource as List<ProfileSoftwareViewModel>;

            foreach (var item in selectedItems)
            {
                available.Remove(item);
                inProfileVM.Add(new ProfileSoftwareViewModel { Software = item, IsInstalled = false });
            }

            AvailableSoftwareListBox.ItemsSource = available.OrderBy(s => s.Name).ToList();
            ProfileSoftwareListView.ItemsSource = inProfileVM.OrderBy(vm => vm.Name).ToList();
        }

        private void RemoveSoftwareButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedVMs = ProfileSoftwareListView.SelectedItems.Cast<ProfileSoftwareViewModel>().ToList();
            if (!selectedVMs.Any()) return;

            var available = AvailableSoftwareListBox.ItemsSource as List<Software>;
            var inProfileVM = ProfileSoftwareListView.ItemsSource as List<ProfileSoftwareViewModel>;

            foreach (var vm in selectedVMs)
            {
                inProfileVM.Remove(vm);
                available.Add(vm.Software);
            }

            AvailableSoftwareListBox.ItemsSource = available.OrderBy(s => s.Name).ToList();
            ProfileSoftwareListView.ItemsSource = inProfileVM.OrderBy(vm => vm.Name).ToList();
        }

        // Логика сохранения теперь должна обновлять и таблицу InstalledSoftware
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentProfile.Name))
            {
                MessageBox.Show("Название профиля не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // --- ШАГ 1: Прикрепляем профиль к контексту ---
                if (_currentProfile.ProfileID == 0)
                {
                    _context.ConfigurationProfiles.Add(_currentProfile);
                }
                else
                {
                    if (_context.Entry(_currentProfile).State == EntityState.Detached)
                    {
                        _context.ConfigurationProfiles.Attach(_currentProfile);
                    }
                    _context.Entry(_currentProfile).State = EntityState.Modified;
                }

                // --- ШАГ 2: Синхронизация связей с ПО через навигационное свойство ---
                var desiredSoftwareList = (ProfileSoftwareListView.ItemsSource as List<ProfileSoftwareViewModel>)
                                            .Select(vm => vm.Software)
                                            .ToList();

                _currentProfile.Softwares.Clear();

                foreach (var softwareItem in desiredSoftwareList)
                {
                    if (_context.Entry(softwareItem).State == EntityState.Detached)
                    {
                        _context.Softwares.Attach(softwareItem);
                    }
                    _currentProfile.Softwares.Add(softwareItem);
                }

                // --- УДАЛЕН ЛИШНИЙ БЛОК КОДА ---
                // Весь цикл foreach, который создавал newLink, был удален отсюда.

                // --- ШАГ 3: Сохранение изменений в статусах установки ---
                // Этот блок остается без изменений...
                var computersWithProfile = _context.Computers
                                                   .Where(c => c.ProfileID == _currentProfile.ProfileID)
                                                   .ToList();

                var finalViewModels = ProfileSoftwareListView.ItemsSource as List<ProfileSoftwareViewModel>;

                foreach (var computer in computersWithProfile)
                {
                    foreach (var vm in finalViewModels)
                    {
                        var installedRecord = _context.InstalledSoftwares
                            .FirstOrDefault(i => i.ComputerID == computer.ComputerID && i.SoftwareID == vm.Software.SoftwareID);

                        if (vm.IsInstalled)
                        {
                            if (installedRecord == null)
                            {
                                _context.InstalledSoftwares.Add(new InstalledSoftware
                                {
                                    ComputerID = computer.ComputerID,
                                    SoftwareID = vm.Software.SoftwareID,
                                    IsInstalled = true
                                });
                            }
                            else
                            {
                                installedRecord.IsInstalled = true;
                            }
                        }
                        else
                        {
                            if (installedRecord != null)
                            {
                                _context.InstalledSoftwares.Remove(installedRecord);
                            }
                        }
                    }
                }

                // --- ФИНАЛЬНОЕ СОХРАНЕНИЕ ---
                _context.SaveChanges();

                MessageBox.Show("Профиль успешно сохранен!", "Успех");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
