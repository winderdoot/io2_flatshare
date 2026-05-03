using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Model.Requests.Listing;

namespace flatshare_server.Infrastructure.Extensions;

public static class DbSeederExtensions
{
    /// <summary>
    /// Seed stub users and listings into the database if they do not already exist.
    /// Safe to call multiple times - will not create duplicates.
    /// Usage: call `await app.Services.SeedStubDataAsync()` from Program.cs (or similar) during startup in a non-production environment.
    /// </summary>
    public static async Task SeedStubDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();

        // Emails used for idempotent checks
        const string tenantEmail = "ewa.nowak@tenant.test";
        const string landlordEmail = "marek.kowalski@landlord.test";

        // If tenant missing -> create
        var tenantExists = await db.Users.AnyAsync(u => u.Email == tenantEmail, cancellationToken);
        if (!tenantExists)
        {
            var tenantReq = new CreateUserRequest(
                FirstName: "Ewa",
                LastName: "Nowak",
                Email: tenantEmail,
                Password: "TenantPass123!",
                Role: CreateUserRequest.Tenant
            );

            var tenantUser = User.TryCreate(tenantReq);

            // Set realistic tenant preferences before saving
            var tenantPrefDto = new TenantPreferencesDTO(
                MaxPrice: 2500m,
                Currency: "PLN",
                SmokingAllowed: false,
                PetsAllowed: true,
                PreferredDistricts: new List<string> { "Wola", "Mokotów" }
            );

            if (tenantUser.Role is TenantRole tenantRole)
            {
                tenantRole.TenantPreferences.UpdatePreferences(tenantPrefDto);
            }

            db.Users.Add(tenantUser);
            await db.SaveChangesAsync(cancellationToken);
        }

        // If landlord missing -> create
        var landlordExists = await db.Users.AnyAsync(u => u.Email == landlordEmail, cancellationToken);
        if (!landlordExists)
        {
            var landlordReq = new CreateUserRequest(
                FirstName: "Marek",
                LastName: "Kowalski",
                Email: landlordEmail,
                Password: "LandlordPass123!",
                Role: CreateUserRequest.Landlord
            );

            var landlordUser = User.TryCreate(landlordReq);

            db.Users.Add(landlordUser);
            await db.SaveChangesAsync(cancellationToken);
        }

        // Ensure we have the landlord entity tracked from DB for ownership relations
        var landlord = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == landlordEmail, cancellationToken);

        if (landlord is null)
        {
            // Should not happen, but guard
            return;
        }

        // Titles used to ensure idempotent listing creation
        var toCreate = new[]
        {
            new CreateListingRequest
            {
                Title = "Przestronne mieszkanie blisko metra",
                Description = "Słoneczne 2-pokojowe mieszkanie, blisko komunikacji, idealne dla pary.",
                Price = 2200m,
                Currency = "PLN",
                AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow),
                AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(9)),
                OwnerContact = "marek.k@example.com",
                Area = 55f,
                Location = new Address("Warszawa", "Wola", "Górczewska", "12"),
                Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student, PetsAllowed = false, NonSmokingOnly = true, CloseToShops = true }
            },
            new CreateListingRequest
            {
                Title = "Klimatyczna kawalerka w Śródmieściu",
                Description = "Mała, zadbana kawalerka idealna dla osoby pracującej. Kamienica, wysoki sufit.",
                Price = 1800m,
                Currency = "PLN",
                AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow),
                AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
                OwnerContact = "marek.k@example.com",
                Area = 28f,
                Location = new Address("Kraków", "Stare Miasto", "Floriańska", "6"),
                Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Tourist, PetsAllowed = false, NonSmokingOnly = false, CloseToShops = true }
            },
            new CreateListingRequest
            {
                Title = "Ciche 3-pokojowe na Kazimierzu",
                Description = "Doskonałe dla studentów lub młodej rodziny. Blisko tramwaj, sklepy, kawiarnie.",
                Price = 2600m,
                Currency = "PLN",
                AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow),
                AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
                OwnerContact = "marek.k@example.com",
                Area = 70f,
                Location = new Address("Kraków", "Kazimierz", "Starowiślna", "8"),
                Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student, PetsAllowed = true, NonSmokingOnly = true, CloseToShops = true }
            },
            new CreateListingRequest
            {
                Title = "Słoneczne studio w sercu Warszawy",
                Description = "Studio po remoncie, idealne dla jednej osoby. Park w pobliżu.",
                Price = 2000m,
                Currency = "PLN",
                AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow),
                AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(10)),
                OwnerContact = "marek.k@example.com",
                Area = 32f,
                Location = new Address("Warszawa", "Śródmieście", "Marszałkowska", "45"),
                Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student, PetsAllowed = false, NonSmokingOnly = false, CloseToShops = true }
            }
        };

        foreach (var req in toCreate)
        {
            // Check if a listing with same title already exists (to avoid duplicates)
            var exists = await db.Listings.AnyAsync(l => l.Title == req.Title, cancellationToken);
            if (exists) continue;

            // Create listing owned by the landlord (tracked)
            var listing = Listing.TryCreate(req, landlord);
            // Mark published/active
            listing.Publish();

            db.Listings.Add(listing);
        }

        // Persist any new listings
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Convenience extension for IHost to call seeding directly.
    /// </summary>
    public static Task SeedStubDataAsync(this IHost host, CancellationToken cancellationToken = default)
        => host.Services.SeedStubDataAsync(cancellationToken);
}