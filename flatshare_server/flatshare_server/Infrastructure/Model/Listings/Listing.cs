using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;

namespace flatshare_server.Infrastructure.Model.Listings;

public class Listing
{
    public enum ListingStatus
    {
        Draft,
        UnderReview,
        Active,
        Hidden,
        HiddenByModeration,
        Archived
    }
    private Listing() { }

    public required Guid Id { get; init; }
    public ListingStatus Status { get; private set; }
    public Money Price { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateOnly AvailableSince { get; private set; }
    public DateOnly AvailableUntil { get; private set; }
    public string OwnerContact { get; private set; }
    public float AreaMeterSq { get; private set; }
    public Address Address { get; private set; }
    public ListingAttributes Attributes { get; private set; }

    public User? Owner { get; private set; }

    public List<Guid> Photos { get; init; }

    /* Methods */
    public ListingDTO IntoDTO()
    {
        return new ListingDTO
        {
            Area = AreaMeterSq,
            Attributes = Attributes,
            AvailableSince = AvailableSince,
            AvailableUntil = AvailableUntil,
            Currency = Price.CurrencyStr(),
            Price = Price.Value,
            Description = Description,
            Id = Id,
            Location = Address,
            OwnerContact = OwnerContact,
            Title = Title
        };
    }
    public static Listing TryCreate(CreateListingRequest request, User owner)
    {
        var errors = new List<FieldError>();

        if (string.IsNullOrEmpty(request.Title))
        {
            errors.Add(new FieldError(nameof(request.Title), $"'{request.Title}' must not be empty"));
        }
        if (string.IsNullOrEmpty(request.Description))
        {
            errors.Add(new FieldError(nameof(request.Description), $"'{request.Description}' must not be empty"));
        }
        if (string.IsNullOrEmpty(request.OwnerContact))
        {
            errors.Add(new FieldError(nameof(request.OwnerContact), $"'{request.OwnerContact}' must not be empty"));
        }

        Money? price = null;
        var curr = Money.ParseCurrency(request.Currency);
        if (curr is null)
        {
            errors.Add(new FieldError(nameof(request.Currency), $"Invalid currency string: {request.Currency}"));
        }
        else
        {
            price = new Money { Curr = curr.Value, Value = request.Price };
        }

        if (request.AvailableUntil <= request.AvailableSince)
        {
            errors.Add(new (nameof(request.AvailableUntil), $"'{nameof(request.AvailableUntil)}' date must be later than '{nameof(request.AvailableSince)}'"));
            errors.Add(new (nameof(request.AvailableSince), $"'{nameof(request.AvailableUntil)}' date must be later than '{nameof(request.AvailableSince)}'"));
        }

        if (request.Area < 0 || request.Area > 300)
        {
            errors.Add(new(nameof(request.Area), $"'{nameof(request.Area)}' must be in range (0, 300)"));
        }

        /* Walidacja adresu */
        if (string.IsNullOrEmpty(request.Location.City))
        {
            errors.Add(new (nameof(request.Location.City), $"'{nameof(request.AvailableUntil)}' date must be later than '{nameof(request.Location.City)}'"));
        }

        if (string.IsNullOrEmpty(request.Location.City))
        {
            errors.Add(new FieldError(nameof(request.Location.City), $"'{request.Location.City}' must not be empty"));
        }
        if (string.IsNullOrEmpty(request.Location.District))
        {
            errors.Add(new FieldError(nameof(request.Location.District), $"'{request.Location.District}' must not be empty"));
        }
        if (string.IsNullOrEmpty(request.Location.Street))
        {
            errors.Add(new FieldError(nameof(request.Location.Street), $"'{request.Location.Street}' must not be empty"));
        }
        if (string.IsNullOrEmpty(request.Location.AptNumber))
        {
            errors.Add(new FieldError(nameof(request.Location.AptNumber), $"'{request.Location.AptNumber}' must not be empty"));
        }

        if (errors.Any())
        {
            throw ErrorResponse.Generate("Listing Error", fields: errors);
        }

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            Address = new Address(request.Location.City, request.Location.District, request.Location.Street, request.Location.AptNumber),
            Attributes = new ListingAttributes
            {
                PetsAllowed = request.Attributes.PetsAllowed,
                NonSmokingOnly = request.Attributes.NonSmokingOnly,
                CloseToShops = request.Attributes.CloseToShops,
                Profile = request.Attributes.Profile
            },
            Status = ListingStatus.Draft,
            Price = price!,
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            AvailableSince = request.AvailableSince,
            AvailableUntil = request.AvailableUntil,
            OwnerContact = request.OwnerContact,
            AreaMeterSq = request.Area,
            Photos = [],
            Owner = owner
        };

        return listing;
     }
}
