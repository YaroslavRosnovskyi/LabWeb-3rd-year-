using LabWeb.Context;
using LabWeb.Repositories;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services;
using LabWeb.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var connection = builder.Configuration.GetConnectionString("AzureSqlDatabase");

if (builder.Environment.IsDevelopment())
{
    connection = builder.Configuration.GetConnectionString("Local");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connection);
});


builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.Decorate<IItemRepository, CachedItemRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.Decorate<IUserRepository, CachedUserRepository>();

builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.Decorate<IShoppingListRepository, CachedShoppingListRepository>();

builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddMapster();

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

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();

app.MapIdentityApi<IdentityUser>();

app.Run();
