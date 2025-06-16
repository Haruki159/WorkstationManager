using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkstationManager.DataBaseFolder;

namespace WorkstationManager.ServiceFolder
{
    public static class CurrentUser
    {
        // Статическое свойство для хранения залогиненного пользователя
        public static User User { get; private set; }

        // Метод для "логина" - сохраняем пользователя
        public static void Login(User user)
        {
            User = user;
        }

        // Метод для "выхода" - очищаем данные
        public static void Logout()
        {
            User = null;
        }

        // Удобное свойство для быстрой проверки, является ли пользователь администратором
        public static bool IsAdmin => User?.RoleID == 1;
    }
}
