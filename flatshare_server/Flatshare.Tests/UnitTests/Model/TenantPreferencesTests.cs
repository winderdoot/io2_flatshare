using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Users;
using FluentAssertions;

namespace Flatshare.Tests.UnitTests.Model;

public class TenantPreferencesTests
{
    [Fact]
    public void UpdatePreferences_ShouldUpdateAllFields_WhenValidDtoProvided()
    {
        // Arrange
        var preferences = new TenantPreferences();
        var dto = new TenantPreferencesDTO(
            MaxPrice: 1500m,
            Currency: "PLN",
            SmokingAllowed: false,
            PetsAllowed: true,
            PreferredDistricts: new List<string> { "Wola", "Bemowo" }
        );

        // Act
        preferences.UpdatePreferences(dto);

        // Assert
        preferences.MaxPrice.Should().Be(1500m);
        preferences.Currency.Should().Be("PLN");
        preferences.SmokingAllowed.Should().BeFalse();
        preferences.PetsAllowed.Should().BeTrue();
        preferences.PreferredDistricts.Should().BeEquivalentTo("Wola", "Bemowo");
    }

    [Fact]
    public void UpdatePreferences_ShouldSetEmptyList_WhenDistrictsAreNull()
    {
        // Arrange
        var preferences = new TenantPreferences();
        var dto = new TenantPreferencesDTO(null, null, null, null, null);

        // Act
        preferences.UpdatePreferences(dto);

        // Assert
        preferences.PreferredDistricts.Should().NotBeNull();
        preferences.PreferredDistricts.Should().BeEmpty();
        preferences.MaxPrice.Should().BeNull();
    }
}