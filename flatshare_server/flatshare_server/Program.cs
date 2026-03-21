using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json.Serialization;
using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

/* Register Global Exception Handler */
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

/* Database setup */
var connectionString = builder.Configuration.GetConnectionString("PostgreDB");

builder.Services.AddDbContext<FlatshareDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDB"))
);

/* Add services */
builder.Services.AddScoped<IUserRepository, DbUserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();

/* Configure controllers */ 
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var fieldErrors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => 
                    new FieldError(
                        e.Key,
                        e.Value!.Errors.First().ErrorMessage
                    )
                )
                .ToList();

            var errorResponse = new ErrorResponse
            {
                Status = StatusCodes.Status400BadRequest,
                Error = "Validation error",
                FieldErrors = fieldErrors
            };

            return new BadRequestObjectResult(errorResponse);
        };
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

/* This should go first I think */
app.UseExceptionHandler();

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
