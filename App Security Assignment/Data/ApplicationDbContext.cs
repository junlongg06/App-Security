using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App_Security_Assignment.Models;
using Microsoft.AspNetCore.Identity;

namespace App_Security_Assignment.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public string SessionId { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure Email is Unique
            builder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();
        }
    }
}
