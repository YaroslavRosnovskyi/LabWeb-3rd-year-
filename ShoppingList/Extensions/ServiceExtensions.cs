using System.Text;
using LabWeb.Repositories.CachedRepositories;
using LabWeb.Repositories.Interfaces;
using LabWeb.Repositories;
using LabWeb.Context;
using LabWeb.Models.IdentityModels;
using LabWeb.SettingOptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LabWeb.Services.AzureServices;
using LabWeb.Services.Interfaces.AzureInterfaces;
using LabWeb.Services.Interfaces;
using LabWeb.Services;
using Mapster;

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
        var connection = configuration.GetConnectionString("AzureSqlDatabase");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connection);
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints()
            .AddRoles<ApplicationRole>()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()
            .AddDefaultTokenProviders();

        return services;
    }
    
    public static IServiceCollection AddOAuthAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration.GetSection("Authentication")["Google:ClientId"]!;
                googleOptions.ClientSecret = configuration.GetSection("Authentication")["Google:ClientSecret"]!;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IItemCategoryService, ItemCategoryService>();
        services.AddScoped<IShoppingListService, ShoppingListService>();

        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IAzureBusSenderService, AzureBusSenderService>();

        services.AddScoped<ITokenService, TokenService>();

        services.AddSingleton<IElasticService, ElasticService>();

        services.AddSingleton<IEmailMessageSender, EmailMessageSender>();

        services.AddMapster();
        services.AddMemoryCache();

        services.AddHostedService<AzureBusReceiverService>();

        return services;
    }
    
    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticSettings>(configuration.GetSection("ElasticsSettings"));
        services.Configure<EmailConfiguration>(configuration.GetSection("EmailConfiguration"));
        services.Configure<SmtpConfiguration>(configuration.GetSection("SmtpConfiguration"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        return services;
    }


}