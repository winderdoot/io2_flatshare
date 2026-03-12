using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using flatshare_server.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

/* Database setup */
var connectionString = builder.Configuration.GetConnectionString("PostgreDB");

builder.Services.AddDbContext<FlatshareDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDB"))
);


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
