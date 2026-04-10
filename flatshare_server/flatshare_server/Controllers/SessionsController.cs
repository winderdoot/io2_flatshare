using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SessionsController : Controller
{

    private AuthService _authService;
    public SessionsController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost()]
    public async Task<IActionResult> UserLogIn([FromBody] LoginRequest request)
    {
        (var token, var sessId, var expInSec, var role) = await _authService.Authenticate(request.Email, request.Password);
        var responseBody = new LoggedInResponse(token, sessId, "Bearer", expInSec, role);
        var endpoint = nameof(GetById);

        return CreatedAtAction(
            actionName: endpoint,
            routeValues: new { id = sessId },
            value: responseBody
        );
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var uid = await _authService.GetUserFromSession(id);
        return Ok(new SessionDTO(id, uid));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> SessionRefresh([FromRoute] Guid id)
    {
        (var token, var sessId, var expInSec, var role) = await _authService.Refresh(id);
        var responseBody = new LoggedInResponse(token, sessId, "Bearer", expInSec, role);
        var endpoint = nameof(GetById);

        return CreatedAtAction(
            actionName: endpoint,
            routeValues: new { id = sessId },
            value: responseBody
        );
    }
}

