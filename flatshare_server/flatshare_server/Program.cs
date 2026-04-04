using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Responses;
using Azure.Storage.Blobs;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

/* Register Global Exception Handler */
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

/* Database setup */
var connectionString = builder.Configuration.GetConnectionString("PostgreDB");

if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<FlatshareDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDB"))
    );
    /* Blob connection */ 
    var blobConnection = builder.Configuration.GetConnectionString("AzureBlobStorage");
    builder.Services.AddSingleton(x => new BlobServiceClient(blobConnection));
    builder.Services.AddScoped<IStorageService, BlobStorageService>();
}


/* Add services */
builder.Services.AddScoped<IUserRepository, DbUserRepository>();
builder.Services.AddScoped<ISessionRepository, DbSessionRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<ListingPhotoService>();

/* Setup JwtOptions */
var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.OptionsKey)
    .Get<JwtOptions>();

if (jwtOptions == null || string.IsNullOrEmpty(jwtOptions.Secret))
{
    throw new InvalidOperationException("JWT configuration is missing from appsettings.json.");
}

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.OptionsKey));

/* Setup Jwt Auhtentication */
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)
            )
        };
    });

builder.Services.AddAuthorization();

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
if (!app.Environment.IsEnvironment("Testing"))
{
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
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

public partial class Program { }
