using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;

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
                return Page();
            }

            var code = await _db.PasswordResetCodes.Where(c => c.Code == Input.Code && c.IsActive).OrderByDescending(c => c.Expiration).FirstOrDefaultAsync();

            if (code == null)
            {
                ModelState.AddModelError("Input.Code", "Código inválido ou inexistente.");
                return Page();
            }

            if (code.Expiration < DateTime.UtcNow)
            {
                ModelState.AddModelError("Input.Code", "O código expirou. Solicite um novo.");
                code.IsActive = false;
                await _db.SaveChangesAsync();
                return Page();
            }

            code.IsActive = false;
            await _db.SaveChangesAsync();

            TempData["UserId"] = code.UserId;
            TempData["Message"] = "Código validado com sucesso. Defina sua nova senha";

            return RedirectToPage("/Account/ResetPassword");
        }
    }
}