using ReportingService.Models.DTO;

namespace ReportingService.Services
{
    public interface IReportingService
    {
        Task<DashboardReportDTO> GetDashboardReportAsync(Guid userId, bool forceRefresh = false);
    }
}
