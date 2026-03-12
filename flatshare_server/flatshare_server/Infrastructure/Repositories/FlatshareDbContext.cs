using Microsoft.EntityFrameworkCore;
using flatshare_server.Infrastructure.Model.User;
using flatshare_server.Infrastructure.Model;

namespace flatshare_server.Infrastructure.Repositories;

public class FlatshareDbContext : DbContext
{
    /* Test entity set */ 
    public DbSet<Foo> Foos { get; set; }
    public DbSet<User> Users { get; set; }

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
            })
            .Entity<UserRole>(entity =>
            {
                entity
                    .Property<int>("Id")
                    .ValueGeneratedOnAdd();
                entity.HasKey("Id");

                entity.Property<Guid>("UserId");

                entity
                    .HasDiscriminator<string>("RoleType")
                    .HasValue<TenantRole>("TENANT")
                    .HasValue<LandlordRole>("LANDLORD");
            })
            .Entity<TenantRole>(entity =>
            {
                entity.OwnsOne(entity => entity.TenantPreferences);
            })
            .Entity<LandlordRole>(entity =>
            {
                entity.OwnsOne(entity => entity.TenantCriteria);
            })
            .Entity<User>(entity =>
            {
                entity
                    .HasOne(u => u.Role)
                    .WithOne(r => r.User)
                    .HasForeignKey<UserRole>("UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.OwnsOne(e => e.Status);

                /* Add db index to emails for better search */
                entity
                    .HasIndex(u => u.Email)
                    .IsUnique();

                entity
                    .Property(u => u.Email)
                    .HasMaxLength(255)
                    .IsRequired();
            });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<AccountStatus.Type>()
            .HaveConversion<string>();
    }
}
