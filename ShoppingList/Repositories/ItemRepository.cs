using LabWeb.Context;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabWeb.Repositories
{
    public class ItemRepository : GenericRepository<Item>, IItemRepository
    {
        public ItemRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        protected override IQueryable<Item> PrepareDbSet()
        {
            return dbSet
                .Include(i => i.ShoppingList)
                .Include(i => i.Category);
        }
    }
}
