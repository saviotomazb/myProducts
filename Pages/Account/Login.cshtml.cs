using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;
using myProducts.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace myProducts.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly MyproductsContext _db;
        private readonly IConfiguration _configuration;

        public LoginModel(MyproductsContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new LoginViewModel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == Input.Username);

            if (user == null || !PasswordHelper.VerifyPassword(Input.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos");

                ModelState.Remove("Input.Username");
                ModelState.Remove("Input.Password");

                Input.Username = string.Empty;
                Input.Password = string.Empty;

                return Page();
            }

            //Configuração do Token de acesso
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não foi configurada.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("FullName", user.FullName)
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

            //Configuração do Refresh Token
            var refreshToken = TokenService.GenerateRefreshToken();
            var refreshTokenHash = TokenService.ComputeHash(refreshToken);

            var userSession = new UserSession
            {
                UserId = user.UserId,
                RefreshTokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                DeviceInfo = Request.Headers["User-Agent"].ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _db.UserSessions.Add(userSession);
            await _db.SaveChangesAsync();

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return RedirectToPage("/Home/Index");
        }

        private static class PasswordHelper
        {
            public static bool VerifyPassword(string password, byte[] storedPasswordHashWithSalt)
            {
                byte[] salt = new byte[16];
                Buffer.BlockCopy(storedPasswordHashWithSalt, 0, salt, 0, 16);

                byte[] storedHash = new byte[32];
                Buffer.BlockCopy(storedPasswordHashWithSalt, 16, storedHash, 0, 32);

                byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    iterations: 100_000,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: 32
                );

                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
        }
    }
}