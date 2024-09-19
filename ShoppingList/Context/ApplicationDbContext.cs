using LabWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace LabWeb.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
    {
            
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>()
            .HasOne(i => i.ShoppingList)
            .WithMany(sl => sl.Items)
            .HasForeignKey(i => i.ShoppingListId);

        modelBuilder.Entity<ShoppingList>()
            .HasOne(sl => sl.User)
            .WithMany(u => u.ShoppingList)
            .HasForeignKey(sl => sl.UserId);

        base.OnModelCreating(modelBuilder);
    }
}