using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using myProducts.Models;
using myProducts.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace myProducts.Pages.Account
{
    [IgnoreAntiforgeryToken]
    public class RefreshTokenModel : PageModel
    {
        private readonly MyproductsContext _db;
        private readonly IConfiguration _configuration;

        public RefreshTokenModel(MyproductsContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return new JsonResult(new { message = "Refresh token ausente" })
                { StatusCode = 401 };
            }
            var refreshTokenHash = TokenService.ComputeHash(refreshToken);

            var session = await _db.UserSessions.Include(s => s.User).FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow);

            if (session == null)
            {
                return new JsonResult(new { message = "Refresh token inválido ou expirado" })
                { StatusCode = 401 };
            }

            var newRefreshToken = TokenService.GenerateRefreshToken();
            var newRefreshTokenHash = TokenService.ComputeHash(newRefreshToken);

            var newSession = new UserSession
            {
                UserId = session.UserId,
                RefreshTokenHash = newRefreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                DeviceInfo = Request.Headers["User-Agent"].ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _db.UserSessions.Add(newSession);
            await _db.SaveChangesAsync();

            //Define a sessão antiga como revogada
            session.RevokedAt = DateTime.UtcNow;
            session.ReplacedBySessionId = newSession.SessionId;

            await _db.SaveChangesAsync();

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não foi configurada.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, session.User.Username),
                new Claim(ClaimTypes.NameIdentifier, session.User.UserId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            Response.Cookies.Append("AuthToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return new JsonResult(new { message = "Tokens atualizados" });
        }

        public IActionResult OnGet()
        {
            return Forbid();
        }
    }
}