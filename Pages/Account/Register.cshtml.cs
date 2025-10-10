using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Services;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using myProducts.Models.ViewModels.Account;

namespace myProducts.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly MyproductsContext _db;

        public RegisterModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new RegisterViewModel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }

            if (await _db.Users.AnyAsync(u => u.Username == Input.Username))
            {
                ModelState.AddModelError("Input.Username", "Este nome de usuário já está em uso");
                return Page();
            }

            if (await _db.Users.AnyAsync(u => u.Email == Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Este e-mail já está cadastrado");
                return Page();
            }

            if (!IsPasswordValid(Input.Password))
            {
                ModelState.AddModelError("Input.Password", "A senha não atende aos critérios de segurança");
                return Page();
            }

            var passwordHashWithSalt = PasswordHelper.HashPassword(Input.Password);

            var user = new User
            {
                Username = Input.Username,
                Email = Input.Email,
                FullName = Input.FullName,
                PasswordHash = passwordHashWithSalt,
                CreatedAt = DateTime.Now,
                IsActive = true
            };


            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return RedirectToPage("/Account/Login");
        }

        private bool IsPasswordValid(string password)
        {
            if (password.Length < 8)
                return false;

            if (!password.Any(char.IsUpper))
                return false;

            if (!password.Any(char.IsLower))
                return false;

            if (!password.Any(char.IsDigit))
                return false;

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return false;

            return true;
        }
    }
}