using LabWeb.Context;
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
using LabWeb.Extensions;
using LabWeb.SettingOptions;
using LabWeb.Models.IdentityModels;
using LabWeb.Repositories.CachedRepositories;
using LabWeb.Services.AzureServices;
using LabWeb.Services.Interfaces.AzureInterfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();



builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", p =>
    {
        p.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true) // This allows any origin
            .AllowCredentials();
    });
});



builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration.GetSection("Authentication")["Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration.GetSection("Authentication")["Google:ClientSecret"];
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

builder.Services.AddRepositories();

builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IItemCategoryService, ItemCategoryService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticsSettings"));
builder.Services.AddSingleton<IElasticService, ElasticService>();


builder.Services.AddMapster();


builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));

builder.Services.AddSingleton<IEmailMessageSender, EmailMessageSender>();

builder.Services.AddScoped<IAzureBusSenderService, AzureBusSenderService>();

builder.Services.AddHostedService<AzureBusReceiverService>();

builder.Services.Configure<SmtpConfiguration>(builder.Configuration.GetSection("SmtpConfiguration"));

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

app.UseCors("DefaultPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
