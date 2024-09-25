using LabWeb.Models.Entities;
using LabWeb.Models.IdentityModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LabWeb.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
    {
            
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }
    public DbSet<ItemCategory> ItemCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>()
            .HasOne(i => i.ShoppingList)
            .WithMany(sl => sl.Items)
            .HasForeignKey(i => i.ShoppingListId);

        modelBuilder.Entity<ShoppingList>()
            .HasOne(sl => sl.ApplicationUser)
            .WithMany(u => u.ShoppingList)
            .HasForeignKey(sl => sl.UserId);

        modelBuilder.Entity<ItemCategory>()
            .HasMany(ic => ic.Items)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.ItemCategoryId);

        base.OnModelCreating(modelBuilder);
    }
}