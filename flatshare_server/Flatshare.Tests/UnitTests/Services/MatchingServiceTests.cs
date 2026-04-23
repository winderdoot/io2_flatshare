using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Requests.Matches;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Services.Listings;

namespace Flatshare.Tests.UnitTests.Services
{
    public class MatchingServiceTests
    {
        private FlatshareDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<FlatshareDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new FlatshareDbContext(options);
        }

        private CreateListingRequest GenerateValidRequest(decimal price = 1000m, string city = "City", string district = "District")
        {
            return new CreateListingRequest
            {
                Title = "Unit test apartment",
                Description = "Testing the service logic",
                Price = price,
                Currency = "EUR",
                AvailableSince = DateOnly.FromDateTime(DateTime.Now),
                AvailableUntil = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
                OwnerContact = "test@owner.com",
                Area = 40.0f,
                Location = new Address(city, district, "Szeroka", "1"),
                Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student }
            };
        }

        [Fact]
        public async Task GetMatchesAsync_ReturnsPagedMatchesOrderedByScore_AndCorrectPageMetadata()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            var owner = User.TryCreate(new CreateUserRequest("Test", "Owner", "owner@test.pl", "Pass123!", CreateUserRequest.Tenant));

            var l1 = Listing.TryCreate(GenerateValidRequest(price: 1000m, city: "A", district: "d1"), owner);
            var l2 = Listing.TryCreate(GenerateValidRequest(price: 1500m, city: "A", district: "d1"), owner);
            var l3 = Listing.TryCreate(GenerateValidRequest(price: 800m, city: "A", district: "d1"), owner);

            context.Listings.AddRange(l1, l2, l3);
            await context.SaveChangesAsync();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var mockCalc = new Mock<IMatchScoreCalculator>();

            // Score proportional to listing price (so ordering is predictable)
            mockCalc
                .Setup(m => m.Score(It.IsAny<Listing>(), It.IsAny<MatchesFilter>()))
                .Returns((Listing listing, MatchesFilter _) => (double)listing.Price.Value);

            var service = new MatchingService(memoryCache, context, mockCalc.Object);

            // Act
            var response = await service.GetMatchesAsync(owner.Id, new MatchesFilter(), page: 0, size: 10);

            // Assert
            response.Should().NotBeNull();
            response.Page.TotalElements.Should().Be(3);
            response.Page.TotalPages.Should().Be(1); // ceil(3/10) == 1
            var scores = response.Content.Select(c => c.MatchScore).ToList();
            // Should be ordered desc by score (price)
            scores.Should().BeInDescendingOrder();
            scores.First().Should().Be((double)l2.Price.Value); // highest price first
            response.Content.Select(c => c.Listing.Id).Should().Contain(new[] { l1.Id, l2.Id, l3.Id });
        }

        [Fact]
        public async Task GetCachedListingsAsync_CachesResults_AndIgnoresFilterPageAndSizeInCacheKey()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            var owner = User.TryCreate(new CreateUserRequest("Test", "Owner", "owner@test.pl", "Pass123!", CreateUserRequest.Tenant));
            var l1 = Listing.TryCreate(GenerateValidRequest(price: 900m), owner);
            var l2 = Listing.TryCreate(GenerateValidRequest(price: 1100m), owner);

            context.Listings.AddRange(l1, l2);
            await context.SaveChangesAsync();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var mockCalc = new Mock<IMatchScoreCalculator>();

            // Return constant score but track invocations
            mockCalc
                .Setup(m => m.Score(It.IsAny<Listing>(), It.IsAny<MatchesFilter>()))
                .Returns(1.0);

            var service = new MatchingService(memoryCache, context, mockCalc.Object);

            var filterPage0 = new MatchesFilter(Page: 0, Size: 10, City: "City");
            var filterPage1 = filterPage0 with { Page = 1, Size = 5 }; // only page/size differ

            // Act - first call populates cache (Score called once per listing)
            var first = await service.GetCachedListingsAsync(filterPage0, owner.Id);

            // second call with different Page/Size should use same cache key
            var second = await service.GetCachedListingsAsync(filterPage1, owner.Id);

            // Assert
            first.Should().HaveCount(2);
            second.Should().HaveCount(2);

            // Score should be invoked exactly 2 times (once per listing) despite two GetCachedListingsAsync calls
            mockCalc.Verify(m => m.Score(It.IsAny<Listing>(), It.IsAny<MatchesFilter>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetMatchesAsync_UsesMethodPaging_ForContent_AndMetadata()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            var owner = User.TryCreate(new CreateUserRequest("P", "U", "p@u.test", "Pass123!", CreateUserRequest.Tenant));
            var l1 = Listing.TryCreate(GenerateValidRequest(price: 100m), owner);
            var l2 = Listing.TryCreate(GenerateValidRequest(price: 200m), owner);
            var l3 = Listing.TryCreate(GenerateValidRequest(price: 300m), owner);

            context.Listings.AddRange(l1, l2, l3);
            await context.SaveChangesAsync();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var mockCalc = new Mock<IMatchScoreCalculator>();
            mockCalc.Setup(m => m.Score(It.IsAny<Listing>(), It.IsAny<MatchesFilter>()))
                .Returns((Listing listing, MatchesFilter _) => (double)listing.Price.Value);

            var service = new MatchingService(memoryCache, context, mockCalc.Object);

            // Use filter with some page/size, but call GetMatchesAsync with explicit page/size that should be used.
            var filter = new MatchesFilter(Page: 0, Size: 1);

            // Provide page=1,size=1 as explicit parameters -> content should be second item after ordering
            var response = await service.GetMatchesAsync(owner.Id, filter, page: 1, size: 1);

            // Assert: content is paged according to explicit page/size arguments
            response.Content.Should().HaveCount(1);
            response.Content.First().MatchScore.Should().Be((double)l2.Price.Value);

            // Metadata uses the provided page/size arguments
            response.Page.Number.Should().Be(1);
            response.Page.Size.Should().Be(1);

            // TotalElements and TotalPages are computed from total matches and provided size argument
            response.Page.TotalElements.Should().Be(3);
            response.Page.TotalPages.Should().Be((int)Math.Ceiling(3 / (double)1)); // 3
        }

        [Fact]
        public async Task GetCachedListingsAsync_ReturnsEmptyList_WhenNoListings()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var mockCalc = new Mock<IMatchScoreCalculator>();
            var service = new MatchingService(memoryCache, context, mockCalc.Object);

            var userId = Guid.NewGuid();

            // Act
            var result = await service.GetCachedListingsAsync(new MatchesFilter(), userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            // Score calculator should not be invoked when there are no listings
            mockCalc.Verify(m => m.Score(It.IsAny<Listing>(), It.IsAny<MatchesFilter>()), Times.Never);
        }
    }
}
