using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Account;
using myProducts.Services;

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
                return Page();
            }

            if (TempData["UserId"] == null)
            {
                TempData["Message"] = "Acesso inválido. Solicite um novo código.";
                return RedirectToPage("/Account/ForgotPassword");
            }

            if(Input.Newpassword != Input.Confirmnewpassword)
            {
                ModelState.AddModelError("Input.Confirmnewpassword", "As senhas não coincidem.");
                return Page();
            }

            int userId = Convert.ToInt32(TempData["UserId"]);

            var activeCode = await _db.PasswordResetCodes.Where
                (c => c.UserId == userId && c.IsActive && c.Expiration > DateTime.UtcNow).OrderByDescending(c => c.Expiration).FirstOrDefaultAsync();

            if (activeCode == null)
            {
                TempData["Message"] = "O código de verificação expirou ou já foi utilizado.";
                return RedirectToPage("/Account/ForgotPassword");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
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
                ModelState.AddModelError("Input.Newpassword", "Informe a nova senha.");
                return Page();
            }

            user.PasswordHash = PasswordService.HashPassword(Input.Newpassword);

            activeCode.IsActive = false;

            await _db.SaveChangesAsync();

            TempData["Message"] = "Senha redefinida com sucesso. Faça o login com a nova senha.";

            return RedirectToPage("/Account/Login");
        }

        public IActionResult OnGet()
        {
            TempData.Remove("Message");

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