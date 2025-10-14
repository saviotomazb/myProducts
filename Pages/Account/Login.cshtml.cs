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
        private readonly UserSessionService _userSessionService;

        public LoginModel(MyproductsContext db, IConfiguration configuration, UserSessionService sessionService)
        {
            _db = db;
            _configuration = configuration;
            _userSessionService = sessionService;
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

            if (user == null || !PasswordService.VerifyPassword(Input.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos");

                ModelState.Remove("Input.Username");
                ModelState.Remove("Input.Password");

                Input.Username = string.Empty;
                Input.Password = string.Empty;

                return Page();
            }

            var tokenString = TokenService.GenerateJwtToken(_configuration, user.Username, user.UserId, user.FullName);

            Response.Cookies.Append("AuthToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            var refreshToken = await _userSessionService.CreateSessionAsync(user, Request);

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return RedirectToPage("/Home/Index");
        }
    }
}