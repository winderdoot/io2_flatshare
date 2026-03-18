using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Exceptions;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : Controller
{
    private UserService _userService;
    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost()]
    public async Task<IActionResult> RegisterNewUser([FromBody] CreateUserRequest request)
    {
        /* ServerResponseExceptions are caught automatically and need not be caught in the controllers. */

        UserDTO user = await _userService.Create(request);
        var responseBody = new UserCreatedResponse("New user created", user);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { user.Id },
            value: responseBody
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetById(Guid id)
    {
        UserDTO user = await _userService.GetById(id);

        return user;
    }
}
