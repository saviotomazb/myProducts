using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;
using myProducts.Services;
using Log = Serilog.Log;

namespace myProducts.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {

        private readonly MyproductsContext _db;

        public ResetPasswordModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty]
        public ResetPasswordViewModel Input { get; set; } = new ResetPasswordViewModel();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Tentativa de redefinição de senha com dados inválidos");
                return Page();
            }

            int? userId = Convert.ToInt32(TempData["UserId"]);

            if (userId == null)
            {
                Log.Warning("Tentativa de redefinição de senha com UserId inválido");
                TempData["Message"] = "Acesso inválido. Solicite um novo código.";
                return RedirectToPage("/Account/ForgotPassword");
            }

            if(Input.Newpassword != Input.Confirmnewpassword)
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Account.ResetPassword").Information
                    ("Senhas não coincidem para redefinição de senha do usuário: {UserId}", userId);
                ModelState.AddModelError("Input.Confirmnewpassword", "As senhas não coincidem.");
                return Page();
            }

            var activeCode = await _db.PasswordResetCodes.Where
                (c => c.UserId == userId && c.IsActive && c.Expiration > DateTime.UtcNow).OrderByDescending(c => c.Expiration).FirstOrDefaultAsync();

            if (activeCode == null)
            {
                Log.Warning("Código de verificação expirado ou já utilizado para o usuário: {UserId}", userId);
                TempData["Message"] = "O código de verificação expirou ou já foi utilizado.";
                return RedirectToPage("/Account/ForgotPassword");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                Log.Error("Usuário não encontrado para UserId: {UserId}", userId);
                TempData["Message"] = "Usuário não encontrado.";
                return RedirectToPage("/Account/ForgotPassword");
            }

            if(!PasswordService.IsValid(Input.Newpassword, out var error))
            {
                ModelState.AddModelError("Input.Newpassword", error);
                return Page();
            }

            if (string.IsNullOrEmpty(Input.Newpassword))
            {
                Log.Warning("Senha não informada para UserId: {UserId}", userId);
                ModelState.AddModelError("Input.Newpassword", "Informe a nova senha.");
                return Page();
            }

            user.PasswordHash = PasswordService.HashPassword(Input.Newpassword);

            //Após a nova senha ser redefinida, o código gerado é desativado evitando múltiplos códigos.
            activeCode.IsActive = false;

            await _db.SaveChangesAsync();

            Log.ForContext("SourceContext", "myProducts.Pages.Account.ResetPassword")
               .Information("Senha redefinida com sucesso para o usuário: {UserId} - {Username}", user.UserId, user.Username);

            TempData["Message"] = "Senha redefinida com sucesso. Faça o login com a nova senha.";

            return RedirectToPage("/Account/Login");
        }

        public IActionResult OnGet()
        {
            TempData.Remove("Message");

            // Garante que a página só seja acessada a partir do fluxo correto (com UserId no TempData).
            if (TempData["UserId"] == null)
            {
                TempData["Message"] = "Acesso inválido. Solicite um novo código.";
                return RedirectToPage("/Account/ForgotPassword");
            }

            TempData.Keep("UserId");

            return Page();
        }
    }
}