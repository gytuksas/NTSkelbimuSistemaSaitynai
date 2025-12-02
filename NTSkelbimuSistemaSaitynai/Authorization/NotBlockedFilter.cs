using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace NTSkelbimuSistemaSaitynai.Authorization
{
    /// <summary>
    /// Action filter that forbids access for blocked brokers or buyers.
    /// </summary>
    public class NotBlockedFilter : IAsyncActionFilter
    {
        private readonly PostgresContext _context;

        public NotBlockedFilter(PostgresContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            var idClaim = user.FindFirst("id")?.Value;
            if (!long.TryParse(idClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Broker")
            {
                var broker = await _context.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.IdUser == userId);
                if (broker?.Blocked == true)
                {
                    context.HttpContext.Response.Headers["X-Account-Blocked"] = "true";
                    await RevokeActiveSessionsAsync(userId);
                    context.Result = new ForbidResult();
                    return;
                }
            }
            else if (role == "Buyer")
            {
                var buyer = await _context.Buyers.AsNoTracking().FirstOrDefaultAsync(b => b.IdUser == userId);
                if (buyer?.Blocked == true)
                {
                    context.HttpContext.Response.Headers["X-Account-Blocked"] = "true";
                    await RevokeActiveSessionsAsync(userId);
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }

        private async Task RevokeActiveSessionsAsync(long userId)
        {
            var activeSessions = await _context.Sessions
                .Where(s => s.FkUseridUser == userId && !s.Revoked && s.Expires > DateTime.UtcNow)
                .ToListAsync();

            if (!activeSessions.Any())
            {
                return;
            }

            foreach (var session in activeSessions)
            {
                session.Revoked = true;
                session.Lastactivity = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
