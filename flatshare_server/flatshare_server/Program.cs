using Azure.Storage.Blobs;
using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

/* Register Global Exception Handler */
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

/* Add CORS */ 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(ApiRoutes.FrontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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
builder.Services.AddScoped<IResetCodesRepository, DbResetCodesRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<ListingPhotoService>();
builder.Services.AddScoped<EmailService>();

/* Add custom server options */
builder.Services.AddAppOptions(builder.Configuration);

/* Add our custom Jwt authentication */
builder.Services.AddAppJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthService.LandlordPolicy, policy =>
        policy.RequireClaim(AuthService.RoleClaim, AuthService.LandlordRole)
    );
});

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

/* First silently handle ServerErrorResponse exceptions */
app.UseDomainExceptionHandler();
/* Then handle 500s */ 
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

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

public partial class Program { }
