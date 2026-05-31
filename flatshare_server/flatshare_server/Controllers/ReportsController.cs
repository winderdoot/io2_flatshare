using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ViolationReportDTO>> CreateReport([FromBody] CreateReportRequest request)
    {
        var createdReport = await _reportService.CreateReportAsync(request);
        return CreatedAtAction(nameof(CreateReport), new { id = createdReport.Id }, createdReport);
    }
}