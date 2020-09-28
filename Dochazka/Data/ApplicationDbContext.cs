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
        public DbSet<Dochazka.Models.Contact> Contact { get; set; }
        public DbSet<PresenceRecordV2> PresenceRecordsV2 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PresenceRecordV2>()
                .HasKey(p => new { p.EmployeeId, p.WorkDay});
        }
    }
}
