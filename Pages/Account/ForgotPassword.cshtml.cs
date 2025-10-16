using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;
using myProducts.Services;
using System.Security.Cryptography;
using Log = Serilog.Log;

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
                Log.Warning("Tentativa de redefinição de senha com dados inválidos: {Input}", Input.User_email);
                return Page();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == Input.User_email || u.Email == Input.User_email);

            if (user == null)
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Account.ForgotPassword").Information
                    ("Tentativa de redefinição de senha com dados inválidos: {Input}", Input.User_email);

                ModelState.AddModelError(string.Empty, "Usuário ou e-mail não cadastrado");

                ModelState.Remove("Input.User_email");

                Input.User_email = string.Empty;

                return Page();
            }

            if (_db.PasswordResetCodes.Any(i => i.UserId == user.UserId && i.IsActive && i.Expiration > DateTime.UtcNow.AddMinutes(-15)))
            {
                ModelState.AddModelError("", "Já foi enviado um código recentemente para o seu e-mail, por favor verifique!");
                return Page();
            }

            var oldCode = await _db.PasswordResetCodes.Where(i => i.UserId == user.UserId && i.IsActive && i.Expiration <= DateTime.UtcNow).ToListAsync();

            foreach (var i in oldCode)
            {
                i.IsActive = false;
                Log.Debug("Código antigo desativado: {CodeId} para o usuário {UserId}", i.PasswordId, i.UserId);
            }

            await _db.SaveChangesAsync();

            var bytes = RandomNumberGenerator.GetBytes(4);
            int value = Math.Abs(BitConverter.ToInt32(bytes)) % 90000 + 10000;
            string code = value.ToString();

            var passwordResetCode = new PasswordResetCode
            {
                UserId = user.UserId,
                Code = code,
                Expiration = DateTime.UtcNow.AddMinutes(15),
                IsActive = true
            };

            Log.ForContext("SourceContext", "myProducts.Pages.Account.ForgotPassword").Information
                ("Novo código de redefinição gerado para usuário {UserId}", user.UserId);

            _db.PasswordResetCodes.Add(passwordResetCode);
            await _db.SaveChangesAsync();

            try
            {
                await EmailService.SendPasswordResetEmailAsync(user.Email, code);
                TempData["Message"] = "Código de redefinição enviado para o seu e-mail";
                Log.ForContext("SourceContext", "myProducts.Pages.Account.ForgotPassword").Information
                    ("E-mail de redefinição enviado para {UserEmail}", user.Email);
                TempData["ForgotPasswordStarted"] = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Falha ao enviar e-mail de redefinição para {UserEmail}", user.Email);
            }

            return RedirectToPage("/Account/VerifyCode");
        }
    }
}