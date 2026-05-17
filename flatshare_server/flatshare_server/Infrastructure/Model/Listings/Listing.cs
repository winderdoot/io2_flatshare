using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using System.Resources;

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
    public List<Unavailability> Unavailabilities { get; private set; } = [];

    public User? Owner { get; private set; }

    public List<Guid> Photos { get; init; }

    /* Methods */
    public ListingDTO IntoDTO()
    {
        return new ListingDTO
        {
            Status = Status,
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
            Title = Title,
            Unavailabilities = [.. Unavailabilities]
        };
    }
    public void AddUnavailability(Unavailability unavailability)
    {
        Unavailabilities.Add(unavailability);
    }
    public void RemoveUnavailability(DateOnly since, DateOnly until)
    {
        if (until <= since)
        {
            throw ErrorResponse.Generate($"'until' date must be later than 'since' date", StatusCodes.Status400BadRequest);
        }

        List<Unavailability> toRemove = [.. Unavailabilities.Where(u => u.Since == since && u.Until == until)];
        if (!toRemove.Any())
        {
            throw ErrorResponse.Generate($"No unavailability found for given date range", StatusCodes.Status404NotFound);
        }
        foreach (var unavailability in toRemove)
        {
            Unavailabilities.Remove(unavailability);
        }
    }

    public static Listing TryCreate(CreateListingRequest request, User owner)
    {
        var errors = new List<FieldError>();

        // Required simple fields
        ValidateRequiredString(request.Title, nameof(request.Title), errors);
        ValidateRequiredString(request.Description, nameof(request.Description), errors);
        ValidateRequiredString(request.OwnerContact, nameof(request.OwnerContact), errors);

        // Price / currency
        var price = TryParsePrice(request.Price, request.Currency, errors);

        // Dates
        ValidateDateRange(request.AvailableSince, request.AvailableUntil, errors);

        // Area
        ValidateArea(request.Area, errors);

        // Location
        ValidateLocation(request.Location, errors);

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
            Owner = owner,
            Unavailabilities = []
        };

        return listing;
    }

    public void ApplyUpdate(UpdateListingRequest request)
    {
        if (Status != ListingStatus.Draft && Status != ListingStatus.Hidden)
        {
            throw ErrorResponse.Generate($"Only listings in '{nameof(ListingStatus.Draft)}' or '{nameof(ListingStatus.Hidden)}' status can be updated", StatusCodes.Status400BadRequest);
        }

        var errors = new List<FieldError>();

        ValidateCurrency(request.Price, request.Currency, errors);

        if (request.AvailableSince.HasValue && request.AvailableUntil.HasValue)
        {
            ValidateDateRange(request.AvailableSince.Value, request.AvailableUntil.Value, errors);
        }

        if (request.Area.HasValue)
        {
            ValidateArea(request.Area.Value, errors);
        }

        if (request.Location is not null)
        {
            ValidateLocation(request.Location, errors);
        }

        if (errors.Any())
        {
            throw ErrorResponse.Generate("Listing Update Error", fields: errors);
        }

        if (request.Title is not null)
            Title = request.Title;

        if (request.Description is not null)
            Description = request.Description;

        if (request.Price.HasValue)
        {
            /* we've already validated currency presence / correctness above */
            var curr = Money.ParseCurrency(request.Currency!)!.Value;
            Price = new Money { Curr = curr, Value = request.Price.Value };
        }

        if (request.AvailableSince.HasValue)
            AvailableSince = request.AvailableSince.Value;

        if (request.AvailableUntil.HasValue)
            AvailableUntil = request.AvailableUntil.Value;

        if (request.OwnerContact is not null)
            OwnerContact = request.OwnerContact;

        if (request.Area.HasValue)
            AreaMeterSq = request.Area.Value;

        if (request.Location is not null)
            Address = request.Location;

        if (request.Attributes is not null)
            Attributes = request.Attributes;
    }

    /* Private validation helpers */
    private static void ValidateRequiredString(string? value, string fieldName, List<FieldError> errors)
    {
        if (string.IsNullOrEmpty(value))
        {
            errors.Add(new FieldError(fieldName, $"'{value}' must not be empty"));
        }
    }

    private static Money? TryParsePrice(decimal priceValue, string currency, List<FieldError> errors)
    {
        var curr = Money.ParseCurrency(currency);
        if (curr is null)
        {
            errors.Add(new FieldError("Currency", $"Invalid currency string: {currency}"));
            return null;
        }
        return new Money { Curr = curr.Value, Value = priceValue };
    }

    private static void ValidateCurrency(decimal? price, string? currency, List<FieldError> errors)
    {
        if (price.HasValue && string.IsNullOrWhiteSpace(currency))
        {
            errors.Add(new FieldError("Currency", "Currency must be provided when updating price"));
        }

        if (!string.IsNullOrWhiteSpace(currency))
        {
            var parsed = Money.ParseCurrency(currency);
            if (parsed is null)
            {
                errors.Add(new FieldError("Currency", $"Invalid currency string: {currency}"));
            }
        }
    }

    private static void ValidateDateRange(DateOnly since, DateOnly until, List<FieldError> errors)
    {
        if (until <= since)
        {
            errors.Add(new FieldError(nameof(AvailableUntil), $"'{nameof(AvailableUntil)}' date must be later than '{nameof(AvailableSince)}'"));
            errors.Add(new FieldError(nameof(AvailableSince), $"'{nameof(AvailableUntil)}' date must be later than '{nameof(AvailableSince)}'"));
        }
    }

    private static void ValidateArea(float area, List<FieldError> errors)
    {
        if (area < 0 || area > 300)
        {
            errors.Add(new FieldError(nameof(AreaMeterSq), $"'{nameof(AreaMeterSq)}' must be in range (0, 300)"));
        }
    }

    private static void ValidateLocation(Address location, List<FieldError> errors)
    {
        if (location is null)
        {
            errors.Add(new FieldError(nameof(location), $"'{nameof(location)}' must not be empty"));
            return;
        }

        ValidateRequiredString(location.City, nameof(Address.City), errors);
        ValidateRequiredString(location.District, nameof(Address.District), errors);
        ValidateRequiredString(location.Street, nameof(Address.Street), errors);
        ValidateRequiredString(location.AptNumber, nameof(Address.AptNumber), errors);
    }

    /* State changes */ 
    public void SubmitForReview()
    {
        if (Status != ListingStatus.Draft)
        {
            throw ErrorResponse.Generate($"Listing must be in '{nameof(ListingStatus.Draft)}' status to submit for review", StatusCodes.Status400BadRequest);
        }
        Status = ListingStatus.UnderReview;
    }
    public void RequestFixes()
    {
        if (Status != ListingStatus.UnderReview)
        {
            throw ErrorResponse.Generate($"Listing must be in '{nameof(ListingStatus.UnderReview)}' status to request fixes", StatusCodes.Status400BadRequest);
        }
        Status = ListingStatus.Draft;
    }
    public void Approve()
    {
        if (Status != ListingStatus.UnderReview)
        {
            throw ErrorResponse.Generate($"Listing must be in '{nameof(ListingStatus.UnderReview)}' status to approve", StatusCodes.Status400BadRequest);
        }
        Status = ListingStatus.Active;
    }
    public void Publish()
    {
        if (Status != ListingStatus.Hidden && Status != ListingStatus.Draft)
        {
            throw ErrorResponse.Generate($"Listing must be in '{nameof(ListingStatus.Draft)}' or '{nameof(ListingStatus.Hidden)}' status to publish", StatusCodes.Status400BadRequest);
        }
        Status = ListingStatus.Active;
    }
    public void HideByModeration()
    {
        if (Status != ListingStatus.Active)
        {
            throw ErrorResponse.Generate($"Listing must be in '{nameof(ListingStatus.Active)}' status to hide by moderation", StatusCodes.Status400BadRequest);
        }
        Status = ListingStatus.HiddenByModeration;
    }
    public void Reinstate()
    {
        if (Status != ListingStatus.HiddenByModeration)
        {
            throw ErrorResponse.Generate($"Listing must be in '{nameof(ListingStatus.HiddenByModeration)}' status to reinstate", StatusCodes.Status400BadRequest);
        }
        Status = ListingStatus.Active;
    }
    public void Hide()
    {
        if (Status != ListingStatus.Active)
        {
            throw ErrorResponse.Generate($"Listing must be in '{nameof(ListingStatus.Active)}' status to hide", StatusCodes.Status400BadRequest);
        }
        Status = ListingStatus.Hidden;
    }
    public void Archive()
    {
        if (Status != ListingStatus.Active && Status != ListingStatus.Hidden && Status != ListingStatus.HiddenByModeration)
        {
            throw ErrorResponse.Generate(
                $"Listing must be in '{nameof(ListingStatus.Active)}' or '{nameof(ListingStatus.Hidden)}'" +
                $" or '{nameof(ListingStatus.HiddenByModeration)}' status to archive", StatusCodes.Status400BadRequest
            );
        }
        Status = ListingStatus.Archived;
    }
}
