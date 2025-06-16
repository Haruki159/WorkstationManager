using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkstationManager.DataBaseFolder;

namespace WorkstationManager.ClassFolder
{
    public class ProfileSoftwareViewModel
    {
        // Ссылка на оригинальный объект ПО
        public Software Software { get; set; }

        // Свойство для привязки к CheckBox
        public bool IsInstalled { get; set; }

        // Свойство, которое будет отображаться в списке (имя ПО)
        public string Name => Software?.Name;
    }
}
