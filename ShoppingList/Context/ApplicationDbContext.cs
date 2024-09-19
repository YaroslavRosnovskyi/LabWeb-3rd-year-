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
}