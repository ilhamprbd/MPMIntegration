﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MPMIntegration
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DashBoardMPMEntities1 : DbContext
    {
        public DashBoardMPMEntities1()
            : base("name=DashBoardMPMEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<api_application> api_application { get; set; }
        public virtual DbSet<api_client_configuration> api_client_configuration { get; set; }
        public virtual DbSet<api_log_request> api_log_request { get; set; }
        public virtual DbSet<api_parameter> api_parameter { get; set; }
        public virtual DbSet<api_url> api_url { get; set; }
        public virtual DbSet<it_task_register> it_task_register { get; set; }
        public virtual DbSet<it_task_schedule> it_task_schedule { get; set; }
        public virtual DbSet<tbl_batch_status> tbl_batch_status { get; set; }
        public virtual DbSet<tbl_participant_list> tbl_participant_list { get; set; }
        public virtual DbSet<tbl_participant_list_history> tbl_participant_list_history { get; set; }
        public virtual DbSet<tbl_participant_status> tbl_participant_status { get; set; }
        public virtual DbSet<tbl_placing_batch> tbl_placing_batch { get; set; }
        public virtual DbSet<it_report_list> it_report_list { get; set; }
        public virtual DbSet<tbl_policies_holder> tbl_policies_holder { get; set; }
        public virtual DbSet<tbl_cover_notes> tbl_cover_notes { get; set; }
    }
}
