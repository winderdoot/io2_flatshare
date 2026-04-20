using FluentAssertions;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model.Exceptions;
using Microsoft.AspNetCore.Http;
using flatshare_server.Infrastructure.Model.Requests;

namespace Flatshare.Tests.Unit;

public class UserTests
{
    [Fact]
    public void TryCreate_ShouldCreateTenant_WhenRoleIsTenant()
    {
        // Arrange
        var request = new CreateUserRequest("Jan", "Kowalski", "jan@test.pl", "StrongPass123!", CreateUserRequest.Tenant);

        // Act
        var user = User.TryCreate(request);

        // Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(request.FirstName);
        user.Role.Should().BeOfType<TenantRole>();
    }

    [Fact]
    public void TryCreate_ShouldCreateLandlord_WhenRoleIsLandlord()
    {
        // Arrange
        var request = new CreateUserRequest("Adam", "Nowak", "adam@landlord.pl", "Pass123456", CreateUserRequest.Landlord);

        // Act
        var user = User.TryCreate(request);

        // Assert
        user.Role.Should().BeOfType<LandlordRole>();
    }

    [Theory]
    [InlineData("", "Kowalski", "test@pl.pl", "pass123", "TENANT", "FirstName")]
    [InlineData("Jan", "K", "test@pl.pl", "pass123", "TENANT", "LastName")]
    [InlineData("Jan", "Kowalski", "invalid-email", "pass123", "TENANT", "Email")]
    [InlineData("Jan", "Kowalski", "test@pl.pl", "123", "TENANT", "Password")]
    [InlineData("Jan", "Kowalski", "test@pl.pl", "pass123", "INVALID_ROLE", "Role")]
    public void TryCreate_ShouldThrowBadRequest_WhenDataIsInvalid(
        string fName, string lName, string email, string pass, string role, string expectedErrorField)
    {
        // Arrange
        var request = new CreateUserRequest(fName, lName, email, pass, role);

        // Act
        var act = () => User.TryCreate(request);

        // Assert
        var exception = act.Should().Throw<ServerResponseException>().Which;

        exception.Response.Status.Should().Be(StatusCodes.Status400BadRequest);
        exception.Response.FieldErrors.Should().Contain(e => e.Field == expectedErrorField);
    }

    [Fact]
    public void TryCreate_ShouldCorrectlyAssignPreferences_ForTenant()
    {
        // Arrange
        var request = new CreateUserRequest("Jan", "Kowalski", "jan@test.pl", "Pass123456", CreateUserRequest.Tenant);

        // Act
        var user = User.TryCreate(request);

        // Assert
        var role = user.Role as TenantRole;
        role.Should().NotBeNull();
        role!.TenantPreferences.Should().NotBeNull();
    }

    [Fact]
    public void TryCreate_ShouldCreateAdmin_WhenRoleIsAdmin()
    {
        // Arrange
        var request = new CreateUserRequest("Alice", "Admin", "alice.admin@test.pl", "AdminPass123!", CreateUserRequest.Admin);

        // Act
        var user = User.TryCreate(request);

        // Assert
        user.Should().NotBeNull();
        user.Role.Should().BeOfType<AdminRole>();
        user.IntoDTO().Role.Should().Be(CreateUserRequest.Admin);
    }
}