using Hangfire.Dashboard;

namespace ControleCerto.Utils
{
    public class DevOnlyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IWebHostEnvironment _env;

        public DevOnlyAuthorizationFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        public bool Authorize(DashboardContext context)
        {
            // Permitir apenas se estiver em Development
            return _env.IsDevelopment();
        }
    }
}
