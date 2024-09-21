using LabWeb.Context;
using LabWeb.Models;
using LabWeb.Repositories;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services;
using LabWeb.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Text;
using LabWeb.SettingOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var connection = builder.Configuration.GetConnectionString("AzureSqlDatabase");

if (builder.Environment.IsDevelopment())
{
    connection = builder.Configuration.GetConnectionString("AzureSqlDatabase");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connection);
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints()
    .AddRoles<ApplicationRole>()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()
    .AddDefaultTokenProviders(); ;

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);




builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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

builder.Services.AddAuthorizationBuilder();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.Decorate<IItemRepository, CachedItemRepository>();

//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.Decorate<IUserRepository, CachedUserRepository>();

builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.Decorate<IShoppingListRepository, CachedShoppingListRepository>();

builder.Services.AddScoped<IItemService, ItemService>();
//builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddMapster();

builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticsSettings"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.ConfigurationOptions = new ConfigurationOptions
    {
        EndPoints = { "localhost:6379" },
        ConnectTimeout = 10000,  
        SyncTimeout = 10000      
    };
});

builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage"]!, preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorage:queue"]!, preferMsi: true);
});


var app = builder.Build();

app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();


app.Run();
