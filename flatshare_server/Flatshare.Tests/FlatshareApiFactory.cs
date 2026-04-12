using Flatshare.Tests.Utils;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

namespace Flatshare.Tests;

public class FlatshareApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"IntegrationTestsDb_{Guid.NewGuid()}";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("EmailOptions:AppName", "Flatshare-Test");
        builder.UseSetting("EmailOptions:Host", "smtp.gmail.com");
        builder.UseSetting("EmailOptions:Port", "587");
        builder.UseSetting("EmailOptions:EmailAddress", "test@test.com");
        builder.UseSetting("EmailOptions:AppPassword", "test-password");

        builder.ConfigureTestServices(services =>
        {
            var storageDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStorageService));
            if (storageDescriptor != null)
            {
                services.Remove(storageDescriptor);
            }
            services.AddSingleton<IStorageService, FakeStorageService>();
            services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.AuthenticationScheme, options => { })
                    // SmartScheme
                    .AddPolicyScheme("SmartScheme", "Bearer or Test", options =>
                    {
                        options.ForwardDefaultSelector = context =>
                        {
                            // Jeśli przekazano testowy nagłówek -> używamy mocka
                            if (context.Request.Headers.ContainsKey("X-Test-User-Id"))
                                return TestAuthHandler.AuthenticationScheme;

                            // W przeciwnym razie -> używamy standardowego JWT Bearer
                            return JwtBearerDefaults.AuthenticationScheme;
                        };
                    });

            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "SmartScheme";
                options.DefaultChallengeScheme = "SmartScheme";
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<FlatshareDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            db.Database.EnsureCreated();
        });
    }
}