using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Dochazka.Models;

namespace Dochazka.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Dochazka.Models.Contact> Contact { get; set; }
        public DbSet<PresenceRecord> PresenceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PresenceRecord>()
                .HasKey(p => new { p.employeeId, p.WorkDay, p.DayTimeSlot });
            //.HasIndex(p => new { p.employeeId, p.WorkDay, p.DayTimeSlot })
            //.HasIndex(p => new { p.employeeId })
            //.IsUnique();
        }
    }
}
