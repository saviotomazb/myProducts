using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using myProducts.Helpers;
using myProducts.Models;
using myProducts.Models.ViewModels.Clients;
using Log = Serilog.Log;

namespace myProducts.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly MyproductsContext _db;

        public List<SelectListItem> States { get; set; } = new();

        public IndexModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty]
        public ClientViewModel Input { get; set; } = new();

        public void OnGet()
        {
            States = StateHelper.GetSelectList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            States = StateHelper.GetSelectList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = new Client
            {
                FullName = Input.FullName,
                PhoneNumber = PhoneHelper.Clean(Input.PhoneNumber),
                City = Input.City,
                State = StateHelper.Normalize(Input.State),
                Street = Input.Street,
                District = Input.District,
                Complement = Input.Complement,
                HouseNumber = Input.HouseNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                await _db.Clients.AddAsync(client);
                await _db.SaveChangesAsync();

                Log.ForContext("SourceContext", "myProducts.Pages.Clients.Index").Information
                        ("Cliente cadastrado com sucesso: {Name}", Input.FullName);

                TempData["SuccessMessage"] = "Cliente cadastrado com sucesso!";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Clients.Index")
                    .Error(ex, "Erro ao cadastrar cliente: {Name}", Input.FullName);

                ModelState.AddModelError(string.Empty, "Erro ao cadastrar cliente. Tente novamente.");

                return Page();
            }       
        }
    }
}