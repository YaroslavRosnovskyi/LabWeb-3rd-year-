using LabWeb.Context;
using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabWeb.Repositories
{
    public class ShoppingListRepository : GenericRepository<ShoppingList>, IShoppingListRepository
    {
        public ShoppingListRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        protected override IQueryable<ShoppingList> PrepareDbSet()
        {
            return dbSet
                .Include(sl => sl.Items)
                .ThenInclude(i => i.Category)
                .Include(sl => sl.ApplicationUser);
        }
    }
}
