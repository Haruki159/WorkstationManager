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
using WorkstationManager.PagesFolder;
using WorkstationManager.ServiceFolder;

namespace WorkstationManager.WindowsFolder
{
    /// <summary>
    /// Логика взаимодействия для AppWindow.xaml
    /// </summary>
    public partial class AppWindow : Window
    {
        public AppWindow()
        {
            InitializeComponent();
            ApplyRoleRestrictions();
            Manager.MainFrame = MainFrame;
        }

        private void ApplyRoleRestrictions()
        {
            // Скрываем кнопку "Пользователи", если текущий пользователь не админ
            if (!CurrentUser.IsAdmin)
            {
                UsersButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ComputersButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ComputersPage());
        }

        private void ProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProfilesPage());
        }

        private void SoftwareButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new SoftwarePage());
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new EmpolyeesPage());
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ReportsPage());
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new UsersPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.Logout();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}
