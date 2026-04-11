using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using Microsoft.AspNetCore.Authorization;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : Controller
{
    private readonly UserService _userService;
    private readonly AuthService _auth;
    public UsersController(UserService userService, AuthService auth)
    {
        _userService = userService;
        _auth = auth;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterNewUser([FromBody] CreateUserRequest request)
    {
        /* ServerResponseExceptions are caught automatically and need not be caught in the controllers. */

        UserDTO user = (await _userService.Create(request)).IntoDTO();
        var responseBody = new UserCreatedResponse("New user created", user);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { user.Id },
            value: responseBody
        );
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetById(Guid id)
    {
        _auth.AssertUserIs(User, id);
        UserDTO user = (await _userService.GetByIdAsync(id)).IntoDTO();

        return Ok(user);
    }
}
