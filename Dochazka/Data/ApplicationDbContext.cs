using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Dochazka.Models;
using Dochazka.Areas.Identity.Data;

namespace Dochazka.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }        
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AttendanceRecord>()
                .HasKey(p => new { p.EmployeeId, p.WorkDay});

            modelBuilder.Entity<Team>()
                .HasIndex(t => t.TeamName)
                .IsUnique();

        }
    }
}
