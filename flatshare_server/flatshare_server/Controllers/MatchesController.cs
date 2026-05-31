using flatshare_server.Infrastructure.Model.Requests.Matches;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MatchesController
    (
        MatchingService matchingService
    ) : Controller
{

    [HttpGet]
    [Authorize(Roles = AuthService.TenantRole)]
    public async Task<ActionResult<PageResponse<MatchDTO>>> Get([FromQuery] MatchesFilter filter)
    {
        var userId = AuthService.GetUserId(User);
        var result = await matchingService.GetMatchesAsync(userId, filter);

        return Ok(result);
    }
}
