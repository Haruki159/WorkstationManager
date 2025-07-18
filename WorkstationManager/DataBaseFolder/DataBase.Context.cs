﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class WorkstationManagerDB_v2Entities : DbContext
    {
        private static WorkstationManagerDB_v2Entities _context;
        public WorkstationManagerDB_v2Entities()
            : base("name=WorkstationManagerDB_v2Entities")
        {
        }
        public static WorkstationManagerDB_v2Entities GetContext()
        {
            if (_context == null)
                _context = new WorkstationManagerDB_v2Entities();
            return _context;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ActionLog> ActionLogs { get; set; }
        public virtual DbSet<Computer> Computers { get; set; }
        public virtual DbSet<ComputerStatus> ComputerStatuses { get; set; }
        public virtual DbSet<ConfigurationProfile> ConfigurationProfiles { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<InstalledSoftware> InstalledSoftwares { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Software> Softwares { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
