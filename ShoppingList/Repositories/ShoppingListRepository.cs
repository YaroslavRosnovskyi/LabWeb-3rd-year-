using LabWeb.Context;
using LabWeb.Models;
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
                .Include(sl => sl.User);
        }
    }
}
