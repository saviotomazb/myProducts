using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;
using Log = Serilog.Log;

namespace myProducts.Pages.Account
{
    public class VerifyCodeModel : PageModel
    {
        private readonly MyproductsContext _db;

        public VerifyCodeModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty]
        public VerifyCodeViewModel Input { get; set; } = new VerifyCodeViewModel();

        public IActionResult OnGet()
        {
            bool forgotPasswordStarted = TempData["ForgotPasswordStarted"] as bool? ?? false;

            if (!forgotPasswordStarted)
            {
                Log.Warning("Acesso inválido à página sem iniciar o processo de esqueceu a senha");
                TempData["Message"] = "Acesso inválido. Solicite um código primeiro.";
                return RedirectToPage("/Account/ForgotPassword");
            }

            TempData.Keep("ForgotPasswordStarted");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Tentativa de verificação de código inválido");
                return Page();
            }

            var code = await _db.PasswordResetCodes.Where(c => c.Code == Input.Code && c.IsActive).OrderByDescending(c => c.Expiration).FirstOrDefaultAsync();

            if (code == null)
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Account.VerifyCode").Information
                    ("Código inválido ou inexistente: {InputCode}", Input.Code);
                ModelState.AddModelError("Input.Code", "Código inválido ou inexistente.");
                return Page();
            }

            if (DateTime.UtcNow > code.Expiration)
            {
                Log.Warning("Código expirado para o usuário: {UserId}", code.UserId);
                ModelState.AddModelError("Input.Code", "O código expirou. Solicite um novo.");
                code.IsActive = false;
                await _db.SaveChangesAsync();
                return Page();
            }

            TempData["UserId"] = code.UserId;
            TempData["Message"] = "Código validado com sucesso. Defina sua nova senha";

            Log.ForContext("SourceContext", "myProducts.Pages.Account.VerifyCode").Information
                ("Código validado com sucesso para o usuário: {UserId}. Código de verificação: {InputCode}", code.UserId, Input.Code);

            return RedirectToPage("/Account/ResetPassword");
        }
    }
}