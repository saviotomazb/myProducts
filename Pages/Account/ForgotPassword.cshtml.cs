using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;
using myProducts.Services;

namespace myProducts.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly MyproductsContext _db;

        public ForgotPasswordModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty]
        public ForgotPasswordViewModel Input { get; set; } = new ForgotPasswordViewModel();

        public void OnGet()
        {
            TempData.Remove("Message");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == Input.User_email || u.Email == Input.User_email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuário ou e-mail não cadastrado");

                ModelState.Remove("Input.User_email");

                Input.User_email = string.Empty;

                return Page();
            }

            var randomCode = new Random();

            string code = randomCode.Next(10000, 99999).ToString();

            var passwordResetCode = new PasswordResetCode
            {
                UserId = user.UserId,
                Code = code,
                Expiration = DateTime.UtcNow.AddMinutes(15),
                IsActive = true
            };

            _db.PasswordResetCodes.Add(passwordResetCode);
            await _db.SaveChangesAsync();

            await EmailService.SendPasswordResetEmailAsync(user.Email, code);

            TempData["Message"] = "Código de redefinição enviado para o seu e-mail";

            TempData["ForgotPasswordStarted"] = true;

            return RedirectToPage("/Account/VerifyCode");
        }
    }
}