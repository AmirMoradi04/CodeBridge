using CodeBridge.Domain.Enums;
using Hangfire.Dashboard;

namespace CodeBridge.Presentation.Implementations;

/// <summary>
/// فیلتر دسترسی داشبورد Hangfire؛ فقط کاربران دارای نقش Admin مجازند.
/// </summary>
public sealed class AdminOnlyDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(nameof(UserRole.Admin));
    }
}
