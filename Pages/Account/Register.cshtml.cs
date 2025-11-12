using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Services;
using myProducts.Models.ViewModels.Account;
using Log = Serilog.Log;

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
                Log.Warning("Tentativa de registro com dados inválidos: {Username}", Input.Username);
                return Page();
            }

            if (await _db.Users.AnyAsync(u => u.Username == Input.Username))
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Account.Register").Information
                    ("Tentativa de registro com nome de usuário já existente: {Username}", Input.Username);
                ModelState.AddModelError("Input.Username", "Este nome de usuário já está em uso");
                return Page();
            }

            if (await _db.Users.AnyAsync(u => u.Email == Input.Email))
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Account.Register").Information
                    ("Tentativa de registro com e-mail já existente: {Email}", Input.Email);
                ModelState.AddModelError("Input.Email", "Este e-mail já está cadastrado");
                return Page();
            }

            if (!PasswordService.IsValid(Input.Password, out var error))
            {
                ModelState.AddModelError("Input.Password", error);
                return Page();
            }

            //Após os critérios definidos serem cumpridos, é gerado um hash da senha informada e atualizada.
            var passwordHashWithSalt = PasswordService.HashPassword(Input.Password);

            var user = new User
            {
                Username = Input.Username,
                Email = Input.Email,
                FullName = Input.FullName,
                PasswordHash = passwordHashWithSalt,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            Log.ForContext("SourceContext", "myProducts.Pages.Account.Register").Information
                ("Novo usuário registrado: {UserId} - {Username}", user.UserId, user.Username);

            return RedirectToPage("/Account/Login");
        }
    }
}