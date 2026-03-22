using flatshare_server.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Flatshare.Tests;

public class FlatshareApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<FlatshareDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestsDb");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            db.Database.EnsureCreated();
        });
    }
}