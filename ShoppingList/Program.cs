using Microsoft.Extensions.Azure;
using LabWeb.Extensions;
using LabWeb.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", p =>
    {
        p.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials();
    });
});

builder.Services.AddConfigurations(builder.Configuration);

builder.Services.AddOAuthAndAuthorization(builder.Configuration);

builder.Services.AddAuthorizationBuilder();

builder.Services.AddRepositories();

builder.Services.AddServices(builder.Configuration);

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

app.UseExceptionHandler();

app.UseCors("DefaultPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
