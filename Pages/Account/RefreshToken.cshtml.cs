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
        private readonly UserSessionService _userSessionService;

        public RefreshTokenModel(MyproductsContext db, IConfiguration configuration, UserSessionService sessionService)
        {
            _db = db;
            _configuration = configuration;
            _userSessionService = sessionService;
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

            var oldSession = await _db.UserSessions.Include(s => s.User).FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow);

            if (oldSession == null)
            {
                return new JsonResult(new { message = "Refresh token inválido ou expirado" })
                { StatusCode = 401 };
            }

            var newRefreshToken = await _userSessionService.CreateSessionAsync(oldSession.User, Request);

            await _userSessionService.RevokeSessionAsync(oldSession);

            var jwtToken = TokenService.GenerateJwtToken(_configuration, oldSession.User.Username, oldSession.User.UserId, oldSession.User.FullName);

            Response.Cookies.Append("AuthToken", jwtToken, new CookieOptions
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