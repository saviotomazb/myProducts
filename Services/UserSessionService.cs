using Azure.Core;
using Microsoft.Extensions.Configuration;
using myProducts.Models;

namespace myProducts.Services
{
    public class UserSessionService
    {
        private readonly MyproductsContext _db;

        public UserSessionService(MyproductsContext db)
        {
            _db = db;
        }

        public async Task<string> CreateSessionAsync(User user, HttpRequest request)
        {
            var refreshToken = TokenService.GenerateRefreshToken();
            var refreshTokenHash = TokenService.ComputeHash(refreshToken);

            var session = new UserSession
            {
                UserId = user.UserId,
                RefreshTokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                DeviceInfo = request.Headers["User-Agent"].ToString(),
                IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _db.UserSessions.Add(session);
            await _db.SaveChangesAsync();

            return refreshToken;
        }

        public async Task RevokeSessionAsync(UserSession session, UserSession? replacedBy = null)
        {
            session.RevokedAt = DateTime.UtcNow;
            if (replacedBy != null)
            {
                session.ReplacedBySessionId = replacedBy.SessionId;
            }

            await _db.SaveChangesAsync();
        }
    }
}