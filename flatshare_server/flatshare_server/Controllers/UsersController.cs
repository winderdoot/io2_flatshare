using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Requests;

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
        try
        {
            UserDTO user = await _userService.Create(request);
            var responseBody = new UserCreatedResponse("New user created", user);
            return CreatedAtAction(
                actionName: nameof(GetUser),
                routeValues: new { user.Id },
                value: responseBody
            );
        }
        catch (Exception ex)
        {
            /* TODO: Return proper errors */
            return BadRequest(ex.Message);

            throw new NotImplementedException("TODO: Use Problem Details standard to somehow implement the weird error messages we have to implement.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUser(Guid id)
    {
        throw new NotImplementedException();
    }
}
