using Azure.Core;
using Microsoft.Extensions.Configuration;
using myProducts.Models;
using Serilog;
using Log = Serilog.Log;

namespace myProducts.Services
{
    public class UserSessionService
    {
        private readonly MyproductsContext _db;

        public UserSessionService(MyproductsContext db)
        {
            _db = db;
        }

        //Método que salva as informações de sessão (refresh token) do usuário via banco. 
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

            Log.ForContext("SourceContext", "myProducts.Services.UserSessionService").Information("Sessão criada para UserId {UserId} com IP {IpAddress} e Device {DeviceInfo}",
                user.UserId, session.IpAddress, session.DeviceInfo);

            return refreshToken;
        }

        //Revoga a sessão existente do usuário, exemplo: após o logout.
        public async Task RevokeSessionAsync(UserSession session, UserSession? replacedBy = null)
        {
            session.RevokedAt = DateTime.UtcNow;
            if (replacedBy != null)
            {
                session.ReplacedBySessionId = replacedBy.SessionId;
            }

            await _db.SaveChangesAsync();

            Log.ForContext("SourceContext", "myProducts.Services.UserSessionService").Information("Sessão revogada SessionId {SessionId} para UserId {UserId}",
                session.SessionId, session.UserId);
        }
    }
}