using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

/* Database setup */
var connectionString = builder.Configuration.GetConnectionString("PostgreDB");

builder.Services.AddDbContext<FlatshareDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDB"))
);

/* Add services */
builder.Services.AddScoped<IUserRepository, DbUserRepository>();
builder.Services.AddScoped<UserService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

/* Aplly migrations automatically */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<FlatshareDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
