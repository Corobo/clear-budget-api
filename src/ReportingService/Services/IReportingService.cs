using ReportingService.Models.DTO;

namespace ReportingService.Services
{
    public interface IReportingService
    {
        Task<DashboardReportDTO> GetDashboardReportAsync(string userId, bool forceRefresh = false);
    }
}
