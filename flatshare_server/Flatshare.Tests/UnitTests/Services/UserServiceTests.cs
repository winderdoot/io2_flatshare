using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace Flatshare.Tests.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepoMock.Object);
    }

    [Theory]
    [InlineData(CreateUserRequest.Tenant)]
    [InlineData(CreateUserRequest.Landlord)]
    public async Task Create_ShouldHandleDifferentRoles_AndSaveUser(string role)
    {
        // Arrange
        var request = new CreateUserRequest(
            "Adam",
            "Ebacki",
            "adam@example.com",
            "Secret123!",
            role
        );

        // Act
        var result = (await _userService.Create(request)).IntoDTO();

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);

        _userRepoMock.Verify(repo => repo.SaveNew(It.Is<User>(u => u.Role.ToString() == role)), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldThrowException_WhenRoleIsInvalid()
    {
        // Arrange
        var request = new CreateUserRequest("Jan", "K.", "jan@test.pl", "Pass", "INVALID_ROLE");

        // Act & Assert
        await Assert.ThrowsAsync<ServerResponseException>(() => _userService.Create(request));
    }
    [Fact]
    public async Task GetPreferencesAsync_ShouldReturnPreferences_WhenUserIsTenant()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantUser = User.TryCreate(new CreateUserRequest("Jan", "Kowalski", "jan@test.pl", "Pass123!", CreateUserRequest.Tenant));

        _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(tenantUser);

        // Act
        var result = await _userService.GetPreferencesAsync(userId);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPreferencesAsync_ShouldThrowForbidden_WhenUserIsLandlord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var landlordUser = User.TryCreate(new CreateUserRequest("Adam", "Nowak", "adam@test.pl", "Pass123!", CreateUserRequest.Landlord));

        _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(landlordUser);

        // Act
        var act = () => _userService.GetPreferencesAsync(userId);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task UpdatePreferencesAsync_ShouldUpdateAndSave_WhenUserIsTenant()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantUser = User.TryCreate(new CreateUserRequest("Jan", "Kowalski", "jan@test.pl", "Pass123!", CreateUserRequest.Tenant));

        _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(tenantUser);

        var dto = new TenantPreferencesDTO(2000m, "PLN", true, false, new List<string> { "Śródmieście" });

        // Act
        var result = await _userService.UpdatePreferencesAsync(userId, dto);

        // Assert
        result.Should().NotBeNull();
        result.MaxPrice.Should().Be(2000m);
        result.SmokingAllowed.Should().BeTrue();

        _userRepoMock.Verify(repo => repo.Update(tenantUser), Times.Once);
    }

    [Fact]
    public async Task UpdatePreferencesAsync_ShouldThrowForbidden_WhenUserIsLandlord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var landlordUser = User.TryCreate(new CreateUserRequest("Adam", "Nowak", "adam@test.pl", "Pass123!", CreateUserRequest.Landlord));

        _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(landlordUser);

        var dto = new TenantPreferencesDTO(2000m, "PLN", true, false, null);

        // Act
        var act = () => _userService.UpdatePreferencesAsync(userId, dto);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status403Forbidden);

        _userRepoMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Never);
    }
}