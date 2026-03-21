using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.User;
using flatshare_server.Infrastructure.Services;
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
        var token = await _authService.Authenticate(request.Email, request.Password);

        throw new NotImplementedException();
        //var responseBody = new LoggedInResponse(token, )
        //return CreatedAtAction()
    }
}

