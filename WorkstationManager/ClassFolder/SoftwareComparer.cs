using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkstationManager.DataBaseFolder;

namespace WorkstationManager.ClassFolder
{
    public class SoftwareComparer : IEqualityComparer<Software>
    {
        // Метод для проверки равенства
        public bool Equals(Software x, Software y)
        {
            // Проверяем, не являются ли объекты null
            if (ReferenceEquals(x, y)) return true; // Если это один и тот же объект в памяти
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

            // Главное правило: объекты равны, если их ID равны.
            return x.SoftwareID == y.SoftwareID;
        }

        // Метод для получения хэш-кода
        public int GetHashCode(Software obj)
        {
            // Если объект null, возвращаем 0
            if (ReferenceEquals(obj, null)) return 0;

            // Хэш-код должен быть основан на том же поле, что и сравнение
            return obj.SoftwareID.GetHashCode();
        }
    }
}
