using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using flatshare_server.Infrastructure.Model.Requests.Booking;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Listings;
using System.Security.Claims;
using Org.BouncyCastle.Asn1.Pkcs;

namespace flatshare_server.Infrastructure.Services;

public class BookingService(FlatshareDbContext dbContext, ListingService listingService)
{
    private static int MonthsBetweenInclusive(DateOnly start, DateOnly end)
    {
        var months = (end.Year - start.Year) * 12 + (end.Month - start.Month);
        if (end.Day >= start.Day) months += 1;
        return months;
    }
    private Money CalculatePrice(Money basePrice, DateOnly startDate, DateOnly endDate)
    {
        /* Cena listingPrice jest podawana za 30 dni
         * Jak okres pobytu jest poniżej 2 tygodni to liczymy im za każdy dzień osobno stawką listingPrice/30 x liczba_dni
         * Jak okres pobytu jest powyżej 2 tygodni to liczymy im za każdy zaczęty miesiąc tzn ceil(liczba_dni / 30) x listingPrice
         */
        var total = new Money { Curr = basePrice.Curr, Value = 0m };
        var dayPrice = new Money { Curr = basePrice.Curr, Value = basePrice.Value / 30m };
        var daysBetween = endDate.DayNumber - startDate.DayNumber;

        if (daysBetween < 14)
        {
            total.Value = dayPrice.Value * daysBetween;
        }
        else
        {
            total.Value = (decimal)Math.Ceiling(daysBetween / 30.0) * basePrice.Value;
        }

        return total;
    }

    public async Task<BookingCreatedResponse> Create(CreateBookingRequest request, Guid userId)
    {
        if (request.EndDate <= request.StartDate)
            throw ErrorResponse.Generate("InvalidDateRange", StatusCodes.Status400BadRequest);

        var listing = await listingService.GetByIdAsync(request.ListingId);

        if (request.StartDate < listing.AvailableSince || request.EndDate > listing.AvailableUntil)
            throw ErrorResponse.Generate("RoomNotAvailable", StatusCodes.Status409Conflict);

        /* Check for date conflicts with existing bookings for the same listing */
        var conflict = await dbContext.Bookings
            .Where(b => b.ListingId == request.ListingId
                        && (b.Status == Booking.BookingStatus.Confirmed || b.Status == Booking.BookingStatus.PendingPayment))
            .AnyAsync(b => !(request.EndDate <= b.StartDate || request.StartDate >= b.EndDate));

        if (conflict)
        {
            throw ErrorResponse.Generate("Room Occupied", StatusCodes.Status409Conflict);
        }

        var unavailable = listing.Unavailabilities.Find(u => !(request.EndDate < u.Since || request.StartDate > u.Until));
        if (unavailable is not null)
        {
            throw ErrorResponse.Generate($"Room Unavailable between {unavailable.Since} to {unavailable.Until}: {unavailable.Message}", StatusCodes.Status409Conflict);
        }

        Money totalPrice = CalculatePrice(listing.Price, request.StartDate, request.EndDate);

        var booking = Booking.TryCreate(request, userId, totalPrice);

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        return new BookingCreatedResponse(
            booking.BookingId.ToString(),
            booking.Status.ToString(),
            booking.CreatedAt,
            booking.TotalPrice.Value,
            booking.TotalPrice.CurrencyStr(),
            $"/api/v1/bookings/{booking.BookingId}"
        );
    }

    public async Task<AcceptBookingResponse> Accept(Guid bookingId, Guid ownerId)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null)
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);

        var listing = await listingService.GetByIdAsync(booking.ListingId, attachOwner: true);
        if (listing.Owner?.Id != ownerId)
            throw ErrorResponse.Generate("Forbidden", StatusCodes.Status403Forbidden);

        if (booking.Status != Booking.BookingStatus.PendingApproval)
            throw ErrorResponse.Generate($"Cannot accept booking from status {booking.Status}", StatusCodes.Status400BadRequest);

        var unavailable = listing.Unavailabilities.Find(u => !(booking.EndDate < u.Since || booking.StartDate > u.Until));
        if (unavailable is not null)
        {
            throw ErrorResponse.Generate($"Room no longer available between {unavailable.Since} to {unavailable.Until}: {unavailable.Message}", StatusCodes.Status409Conflict);
        }
        await listingService.AddUnavailabilityAsync(
            listing.Id, 
            new Unavailability 
            { 
                Since = booking.StartDate, 
                Until = booking.EndDate , 
                Message = $"Booking {booking.BookingId} accepted" 
            }
        );

        booking.OwnerAccept();
        await dbContext.SaveChangesAsync();

        var acceptedAt = DateTime.UtcNow;
        var paymentRequiredUntil = acceptedAt.AddDays(1);

        return new AcceptBookingResponse(
            booking.BookingId.ToString(),
            booking.Status.ToString(),
            acceptedAt,
            paymentRequiredUntil
        );
    }

    public async Task<RejectBookingResponse> Reject(Guid bookingId, Guid userId, RejectBookingRequest request)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null)
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);

        var listing = await listingService.GetByIdAsync(booking.ListingId, attachOwner: true);
        if (listing.Owner?.Id != userId)
            throw ErrorResponse.Generate("Forbidden", StatusCodes.Status403Forbidden);

        if (booking.Status != Booking.BookingStatus.PendingApproval)
            throw ErrorResponse.Generate($"Cannot reject booking from status {booking.Status}", StatusCodes.Status400BadRequest);

        booking.OwnerReject();
        await dbContext.SaveChangesAsync();

        return new RejectBookingResponse(
            booking.BookingId.ToString(),
            booking.Status.ToString(),
            DateTime.UtcNow,
            request.Reason
        );
    }

    public async Task<CancelBookingResponse> Cancel(Guid bookingId, Guid userId, CancelBookingRequest request)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null)
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);

        if (booking.TenantId != userId)
            throw ErrorResponse.Generate("Forbidden", StatusCodes.Status403Forbidden);

        /* If the booking period already started, disallow cancellation per spec */
        var nowDate = DateOnly.FromDateTime(DateTime.UtcNow);
        if (nowDate >= booking.StartDate)
            throw ErrorResponse.Generate("CancellationNotAllowed", StatusCodes.Status409Conflict);

        /* This will validate allowed statuses inside domain method */
        booking.TenantCancel();
        await dbContext.SaveChangesAsync();

        return new CancelBookingResponse(
            booking.BookingId.ToString(),
            booking.Status.ToString(),
            DateTime.UtcNow,
            "NOT_APPLICABLE"
        );
    }

    public async Task<BookingDTO> GetById(Guid bookingId, Guid userId)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null)
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);

        var listing = await listingService.GetByIdAsync(booking.ListingId, attachOwner: true);
        bool isOwner = listing.Owner?.Id == userId;
        bool isTenant = booking.TenantId == userId;

        if (!isOwner && !isTenant)
            throw ErrorResponse.Generate("Forbidden", StatusCodes.Status403Forbidden);

        return booking.IntoDTO();
    }

    public async Task<List<BookingDTO>> Get(Guid? tenantId, Guid? listingId)
    {
        var query = dbContext.Bookings.AsQueryable().AsNoTracking();

        if (tenantId.HasValue)
        {
            query = query.Where(b => b.TenantId == tenantId.Value);
        }

        if (listingId.HasValue)
        {
            query = query.Where(b => b.ListingId == listingId.Value);
        }

        var entities = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return entities.Select(b => b.IntoDTO()).ToList();
    }

    public async Task VerifyUnavailabilityCollisionsAsync(Guid listingId, DateOnly since, DateOnly until)
    {
        var conflict = await dbContext.Bookings
            .Where(b => b.ListingId == listingId
                        && (b.Status == Booking.BookingStatus.Confirmed || b.Status == Booking.BookingStatus.PendingPayment))
            .AnyAsync(b => !(until < b.StartDate || since > b.EndDate));
        if (conflict)
        {
            throw ErrorResponse.Generate("Room Occupied", StatusCodes.Status409Conflict);
        }
    }

    public async Task<List<Booking>> GetUserBookingAsync(ClaimsPrincipal? user)
    {
        if (user is null)
        {
            throw ErrorResponse.Generate("Unauthorized", StatusCodes.Status401Unauthorized);
        }

        Guid userId = AuthService.GetUserId(user);

        if (user.IsInRole(AuthService.LandlordRole))
        {
            /* Find all bookings that connect to a listing owned by the landlord user */  
            return await dbContext.Bookings
                .Where(b => dbContext.Listings
                    .Include(l => l.Owner)
                    .Any(l => l.Id == b.ListingId && l.Owner != null && l.Owner.Id == userId))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
        else if (user.IsInRole(AuthService.TenantRole))
        {
            /* Find all bookings made by the tenant user */
            return await dbContext.Bookings
                .Where(b => b.TenantId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        throw ErrorResponse.Generate($"Invalid user role", StatusCodes.Status400BadRequest);
    }
}
