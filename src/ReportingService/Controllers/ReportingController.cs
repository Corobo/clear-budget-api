using Microsoft.AspNetCore.Mvc;
using ReportingService.Models.DTO;
using ReportingService.Services;
using Shared.Auth.Controllers;

namespace ReportingService.Controllers
{
    [Route("api/[controller]")]
    public class ReportingController : AuthorizedControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportingController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardReportDTO>> GetDashboard([FromQuery] bool force = false)
        {
            var report = await _reportingService.GetDashboardReportAsync(UserId, force);
            return Ok(report);
        }
    }
}
