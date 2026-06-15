using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Utils;
using FluentAssertions;
using Moq;
using System;
using System.Data;

namespace Flatshare.Tests.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IResetCodesRepository> _resetRepoMock;
    private readonly Mock<ISessionRepository> _sessionRepoMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _resetRepoMock = new Mock<IResetCodesRepository>();
        _sessionRepoMock = new Mock<ISessionRepository>();

        _userService = new UserService(_userRepoMock.Object, _resetRepoMock.Object, _sessionRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenIdExists()
    {
        // Arrange
        var fName = "Adam";
        var lName = "Nowak";
        var email = "adam@test.pl";
        var role = CreateUserRequest.Landlord;
        var landlordUser = User.TryCreate(new CreateUserRequest(fName, lName, email, "Pass123!", role));
        var id = landlordUser.Id;

        _userRepoMock.Setup(repo => repo.GetById(id)).ReturnsAsync(landlordUser);

        // Act
        var user = await _userService.GetByIdAsync(id);
        
        // Assert
        _userRepoMock.Verify(repo => repo.GetById(id), Times.Once);
        user.Id.Should().Be(landlordUser.Id);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(fName);
        user.LastName.Should().Be(lName);
        user.Role.ToString().Should().Be(role);
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var fName = "Adam";
        var lName = "Nowak";
        var email = "adam@test.pl";
        var role = CreateUserRequest.Landlord;
        var landlordUser = User.TryCreate(new CreateUserRequest(fName, lName, email, "Pass123!", role));

        _userRepoMock.Setup(repo => repo.GetByEmail(email)).ReturnsAsync(landlordUser);

        // Act
        var user = await _userService.GetByEmail(email);

        // Assert
        _userRepoMock.Verify(repo => repo.GetByEmail(email), Times.Once);
        user.Should().NotBeNull();
        user.Id.Should().Be(landlordUser.Id);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(fName);
        user.LastName.Should().Be(lName);
        user.Role.Should().Be(role);
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnNull_WhenEmailDoesNotExists()
    {
        // Arrange
        var email = "adam@test.pl";

        // Act
        var user = await _userService.GetByEmail(email);

        // Assert
        _userRepoMock.Verify(repo => repo.GetByEmail(email), Times.Once);
        user.Should().BeNull();
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
        var tenantUser = User.TryCreate(new CreateUserRequest("Jan", "Kowalski", "jan@test.pl", "Pass123!", CreateUserRequest.Tenant));
        var userId = tenantUser.Id;

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
        var landlordUser = User.TryCreate(new CreateUserRequest("Adam", "Nowak", "adam@test.pl", "Pass123!", CreateUserRequest.Landlord));
        var userId = landlordUser.Id;

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
        var tenantUser = User.TryCreate(new CreateUserRequest("Jan", "Kowalski", "jan@test.pl", "Pass123!", CreateUserRequest.Tenant));
        var userId = tenantUser.Id;

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
        var landlordUser = User.TryCreate(new CreateUserRequest("Adam", "Nowak", "adam@test.pl", "Pass123!", CreateUserRequest.Landlord));
        var userId = landlordUser.Id;

        _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(landlordUser);

        var dto = new TenantPreferencesDTO(2000m, "PLN", true, false, null);

        // Act
        var act = () => _userService.UpdatePreferencesAsync(userId, dto);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status403Forbidden);

        _userRepoMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreatePasswordResetEntry_ShouldCreate_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = CodeGenerator.GenerateResetCode(10);

        // Act
        await _userService.CreatePasswordResetEntry(userId, code);

        // Assert
        _resetRepoMock.Verify(repo => repo.SaveNew(userId, code), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldReset_WhenValidCodeExists()
    {
        // Arrange
        var email = "jan@test.pl";
        var code = CodeGenerator.GenerateResetCode();
        var newPassword = "NewSecurePassword123!";
        var request = new ConfirmPasswordResetRequest(code, email, newPassword);

        var user = User.TryCreate(new CreateUserRequest("Jan", "Kowalski", email, "OldPass123!", CreateUserRequest.Tenant));

        _userRepoMock.Setup(repo => repo.GetByEmail(email)).ReturnsAsync(user);
        _resetRepoMock.Setup(repo => repo.CheckValidity(user.Id, code)).ReturnsAsync(true);

        // Act
        var result = await _userService.ResetPassword(request);

        // Assert
        result.Should().BeTrue();
        _userRepoMock.Verify(repo => repo.UpdatePassword(user.Id, newPassword), Times.Once);
        _sessionRepoMock.Verify(repo => repo.InvalidateByUserId(user.Id), Times.Once);
        _resetRepoMock.Verify(repo => repo.InvalidateCodes(user.Id), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnFalse_WhenCodeIsInvalid()
    {
        // Arrange
        var email = "jan@test.pl";
        var invalidCode = CodeGenerator.GenerateResetCode();
        var request = new ConfirmPasswordResetRequest(invalidCode, email, "NewPass123!");

        var user = User.TryCreate(new CreateUserRequest("Jan", "Kowalski", email, "OldPass123!", CreateUserRequest.Tenant));

        _userRepoMock.Setup(repo => repo.GetByEmail(email)).ReturnsAsync(user);
        _resetRepoMock.Setup(repo => repo.CheckValidity(user.Id, invalidCode)).ReturnsAsync(false);

        // Act
        var result = await _userService.ResetPassword(request);

        // Assert
        result.Should().BeFalse();
        _userRepoMock.Verify(repo => repo.UpdatePassword(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _sessionRepoMock.Verify(repo => repo.InvalidateByUserId(It.IsAny<Guid>()), Times.Never);
        _resetRepoMock.Verify(repo => repo.InvalidateCodes(user.Id), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new ConfirmPasswordResetRequest("123456", "nonexistent@test.pl", "NewPass123!");
        _userRepoMock.Setup(repo => repo.GetByEmail(request.Email)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.ResetPassword(request);

        // Assert
        result.Should().BeFalse();
        _resetRepoMock.Verify(repo => repo.CheckValidity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }
}