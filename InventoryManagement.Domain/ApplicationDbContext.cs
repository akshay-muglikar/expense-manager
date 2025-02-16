using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain;

public class ApplicationDbContext: DbContext
{
    public DbSet<Item> Items { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<BillItem> BillItems { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<History> Histories { get; set; }


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
       //optionsBuilder.UseSqlite("Test.db");
    }


     protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints

            // Expense table (no relationships needed)
            modelBuilder.Entity<Expense>()
                .Property(e => e.Description)
                .HasMaxLength(500);

            // Bill table relationships
            modelBuilder.Entity<Bill>()
                .HasKey(x => x.Id);

    
            // BillItem relationships
            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Item)
                .WithMany()
                .HasForeignKey(bi => bi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Bill)
                .WithMany()
                .HasForeignKey(bi => bi.BillId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item table (no specific relationships)
            modelBuilder.Entity<Item>()
                .HasKey(x=>x.Id);

            // History table (generic entity, no relationships)
            modelBuilder.Entity<History>()
                .HasKey(x=>x.Id);

        }
}