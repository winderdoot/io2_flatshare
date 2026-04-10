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
}