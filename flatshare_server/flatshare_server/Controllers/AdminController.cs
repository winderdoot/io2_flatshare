using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace flatshare_server.Controllers;

[ApiController]
[Authorize(Roles = AuthService.AdminRole)]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly ReportService _reportService;

    public AdminController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("reports")]
    public async Task<ActionResult<PageResponse<ViolationReportDTO>>> GetReports([FromQuery] int page = 0, [FromQuery] int size = 20)
    {
        var reportsPage = await _reportService.GetReportsAsync(page, size);
        return Ok(reportsPage);
    }

    [HttpPatch("reports/{id}/open")]
    public async Task<IActionResult> OpenReportCase([FromRoute] Guid id)
    {
        await _reportService.AdminOpenCaseAsync(id);
        return NoContent();
    }

    [HttpPatch("reports/{id}/dismiss")]
    public async Task<IActionResult> DismissReport([FromRoute] Guid id)
    {
        await _reportService.DismissReportAsync(id);
        return NoContent();
    }

    [HttpPost("users/{userId}/ban")]
    public async Task<IActionResult> BanUser([FromRoute] Guid userId, [FromQuery] Guid reportId, [FromBody] BanUserRequest request)
    {
        await _reportService.BanUserAsync(reportId, userId, request.Reason);
        return NoContent();
    }
}