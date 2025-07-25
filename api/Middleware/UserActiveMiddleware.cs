using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using api.Data;

namespace api.Middleware
{
    public class UserActiveMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public UserActiveMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only check for authenticated requests
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Skip check for certain endpoints
                var path = context.Request.Path.Value?.ToLower();
                if (path != null && (path.Contains("/auth/login") || path.Contains("/debug/")))
                {
                    await _next(context);
                    return;
                }

                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var user = await dbContext.Users.FindAsync(userId);
                    if (user == null || !user.IsActive)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Your account has been deactivated. Please contact your administrator.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
