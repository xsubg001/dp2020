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
        public DbSet<AttendanceRecordModel> AttendanceRecords { get; set; }
        public DbSet<TeamModel> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AttendanceRecordModel>()
                .HasKey(p => new { p.EmployeeId, p.WorkDay});

            modelBuilder.Entity<TeamModel>()
                .HasIndex(t => t.TeamName)
                .IsUnique();

        }
    }
}
