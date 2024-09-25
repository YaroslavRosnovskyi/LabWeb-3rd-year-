using LabWeb.Repositories.CachedRepositories;
using LabWeb.Repositories.Interfaces;
using LabWeb.Repositories;
using LabWeb.Context;
using LabWeb.Models.IdentityModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LabWeb.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IItemRepository, ItemRepository>();
        services.Decorate<IItemRepository, CachedItemRepository>();

        services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();
        services.Decorate<IItemCategoryRepository, CachedItemCategoryRepository>();

        services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
        services.Decorate<IShoppingListRepository, CachedShoppingListRepository>();

        return services;
    }
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = services.Configuration.GetConnectionString("AzureSqlDatabase");

        services.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connection);
        });

        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints()
            .AddRoles<ApplicationRole>()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()
            .AddDefaultTokenProviders();


        return services;
    }


}