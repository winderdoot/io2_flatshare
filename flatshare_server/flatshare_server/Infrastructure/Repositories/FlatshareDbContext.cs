using Microsoft.EntityFrameworkCore;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model;
using Microsoft.EntityFrameworkCore.Internal;
using flatshare_server.Controllers;
using flatshare_server.Infrastructure.Model.Listings;

namespace flatshare_server.Infrastructure.Repositories;

public class FlatshareDbContext : DbContext
{
    /* Test entity set */
    public DbSet<UserSession> Sessions { get; set; }
    public DbSet<Foo> Foos {  get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<PasswordResetEntry> PasswordResetEntries { get; set; }

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
            })
            .Entity<Listing>(entity =>
            {
                /* Address as owned entity. Also add composite index for address searching. */ 
                entity.OwnsOne(lis => lis.Address, addressBuilder =>
                {
                    addressBuilder
                        .HasIndex(a => new { a.City, a.District, a.Street, a.AptNumber })
                        .HasDatabaseName("IDX_Listing_Address");
                });
                entity.OwnsOne(lis => lis.Attributes);
                entity.OwnsOne(lis => lis.Price);

                /* Configure Owner Relationship and make an index */ 
                entity
                    .HasOne(l => l.Owner)
                    .WithMany()
                    .HasForeignKey("OwnerId")
                    .IsRequired();

                entity.Property<Guid>("OwnerId");

                entity
                    .HasIndex("OwnerId")
                    .HasDatabaseName("IX_Listings_OwnerId");
            })
            .Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .Property(e => e.IsValid)
                    .HasDefaultValue(true);
            })
            .Entity<PasswordResetEntry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity
                    .Property<int>("Id")
                    .ValueGeneratedOnAdd();

                entity
                    .Property(e => e.ResetCode)
                    .IsRequired();

                entity
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasIndex(e => new { e.UserId, e.ResetCode });

                entity
                    .Property(e => e.IsValid)
                    .HasDefaultValue(true);
            });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<AccountStatus.Type>()
            .HaveConversion<string>();
    }
}
