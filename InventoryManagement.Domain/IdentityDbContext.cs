using System.Diagnostics.Contracts;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain;

public class IdentityDbContext: DbContext 
{
    public DbSet<User> Users { get; set; }
    public DbSet<ClientModel> Clients { get; set; }
    public DbSet<Contact> ContactUs { get; set; }
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    //     optionsBuilder.UseSqlite("Test.db");
    //  }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
                .HasKey(x => x.Id);

        modelBuilder.Entity<ClientModel>()
                .HasKey(x => x.Id);
        modelBuilder.Entity<Contact>()
                .HasKey(x => x.Id);

    }
}