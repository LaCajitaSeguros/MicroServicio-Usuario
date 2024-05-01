using Autenticacion.Controllers;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Persistence
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
           .HasKey(u => u.UserId);

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.HasKey(l => l.UserId);
            });
            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.HasKey(r => new { r.UserId, r.RoleId }); 
            });

        }

    }
}
