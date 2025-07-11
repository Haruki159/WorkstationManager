//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WorkstationManager.DataBaseFolder
{
    using System;
    using System.Collections.Generic;
    
    public partial class Computer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Computer()
        {
            this.ActionLogs = new HashSet<ActionLog>();
            this.InstalledSoftwares = new HashSet<InstalledSoftware>();
        }
    
        public int ComputerID { get; set; }
        public string Name { get; set; }
        public string InventoryNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public Nullable<int> EmployeeID { get; set; }
        public Nullable<int> ProfileID { get; set; }
        public int StatusID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ActionLog> ActionLogs { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ConfigurationProfile ConfigurationProfile { get; set; }
        public virtual ComputerStatus ComputerStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InstalledSoftware> InstalledSoftwares { get; set; }
    }
}
