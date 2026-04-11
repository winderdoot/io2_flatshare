using Flatshare.Tests.Utils;
using flatshare_server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

namespace Flatshare.Tests;

public class FlatshareApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
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
                // Preventing race condition
                options.UseInMemoryDatabase($"IntegrationTestsDb_{Guid.NewGuid()}");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            db.Database.EnsureCreated();
        });
    }
}