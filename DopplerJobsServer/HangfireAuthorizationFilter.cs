using System.Diagnostics.CodeAnalysis;
using Hangfire.Dashboard;

namespace Doppler.Jobs.Server
{
    [ExcludeFromCodeCoverage]
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([Hangfire.Annotations.NotNull] DashboardContext context)
        {
            return true;
        }
    }
}