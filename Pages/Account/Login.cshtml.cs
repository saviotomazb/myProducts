using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;
using myProducts.Services;
using Log = Serilog.Log;

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Tentativa de login com dados inválidos: {Username}", Input.Username);
                return Page();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == Input.Username);

            //Valida se o usuário existe e se a senha fornecida corresponde ao hash armazenado.
            if (user == null || !PasswordService.VerifyPassword(Input.Password, user.PasswordHash))
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Account.Login").Information
                    ("Falha de login para o usuário: {Username}", Input.Username);

                ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos");

                //Limpa os campos para que não fique nenhum input preenchido ao recarregar a página.
                ModelState.Remove("Input.Username");
                ModelState.Remove("Input.Password");

                Input.Username = string.Empty;
                Input.Password = string.Empty;

                return Page();
            }

            //Gera um token JWT que será utilizado para autenticação nas requisições subsequentes.
            var tokenString = TokenService.GenerateJwtToken(_configuration, user.Username, user.UserId, user.FullName);

            Response.Cookies.Append("AuthToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            //Para que o usuário não precise ficar refazendo o login a cada uma hora, é gerado o refresh token para que dure por 7 dias.
            var refreshToken = await _userSessionService.CreateSessionAsync(user, Request);

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            Log.ForContext("SourceContext", "myProducts.Pages.Account.Login").Information
                ("Login bem-sucedido para o usuário: {UserId} - {Username}", user.UserId, user.Username);

            return RedirectToPage("/Home/Index");
        }
    }
}