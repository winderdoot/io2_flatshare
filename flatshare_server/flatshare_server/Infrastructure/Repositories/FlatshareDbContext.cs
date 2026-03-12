using Microsoft.EntityFrameworkCore;
using flatshare_server.Infrastructure.Model;

namespace flatshare_server.Infrastructure.Repositories;

public class FlatshareDbContext : DbContext
{
    /* Test entity set */ 
    public DbSet<Foo> Foos { get; set; }
    public FlatshareDbContext(DbContextOptions<FlatshareDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Foo>(entity =>
            {
                entity
                    .Property<int>("Id")
                    .ValueGeneratedOnAdd();

                entity.HasKey("Id");
            });
    }
}
