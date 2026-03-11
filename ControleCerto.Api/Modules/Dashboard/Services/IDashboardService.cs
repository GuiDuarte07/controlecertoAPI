using ControleCerto.Errors;
using ControleCerto.Modules.Dashboard.DTOs;

namespace ControleCerto.Modules.Dashboard.Services
{
    public interface IDashboardService
    {
        Task<Result<HomeDashboardResponse>> GetHomeDashboardAsync(int userId, DateTime startDate, DateTime endDate);
    }
}
