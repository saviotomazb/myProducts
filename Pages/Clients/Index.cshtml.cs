using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using myProducts.Models;
using myProducts.Models.ViewModels.Clients;
using System.Text.RegularExpressions;
using Log = Serilog.Log;

namespace myProducts.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly MyproductsContext _db;

        public List<SelectListItem> States { get; set; } = new();

        public void OnGet()
        {
            LoadStates();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadStates();

            if (!ModelState.IsValid)
                return Page();

            var cleanPhone = string.IsNullOrWhiteSpace(Input.PhoneNumber)
            ? null
            : Regex.Replace(Input.PhoneNumber, @"\D", "");

            var client = new Client
            {
                FullName = Input.FullName,
                PhoneNumber = cleanPhone,
                City = Input.City,
                State = string.IsNullOrWhiteSpace(Input.State) ? null : Input.State,
                Street = Input.Street,
                District = Input.District,
                Complement = Input.Complement,
                HouseNumber = Input.HouseNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            Log.ForContext("SourceContext", "myProducts.Pages.Clients.Index").Information
                    ("Cliente cadastrado com sucesso: {Name}", Input.FullName);

            TempData["SuccessMessage"] = "Cliente cadastrado com sucesso!";

            return RedirectToPage();
        }

        public IndexModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty]
        public ClientViewModel Input { get; set; } = new();

        private void LoadStates()
        {
            States = new List<SelectListItem>
            {
                new() { Value = "", Text = "Selecione", Disabled = true },
                new() { Value = "AC", Text = "AC" },
                new() { Value = "AL", Text = "AL" },
                new() { Value = "AP", Text = "AP" },
                new() { Value = "AM", Text = "AM" },
                new() { Value = "BA", Text = "BA" },
                new() { Value = "CE", Text = "CE" },
                new() { Value = "DF", Text = "DF" },
                new() { Value = "ES", Text = "ES" },
                new() { Value = "GO", Text = "GO" },
                new() { Value = "MA", Text = "MA" },
                new() { Value = "MT", Text = "MT" },
                new() { Value = "MS", Text = "MS" },
                new() { Value = "MG", Text = "MG" },
                new() { Value = "PA", Text = "PA" },
                new() { Value = "PB", Text = "PB" },
                new() { Value = "PR", Text = "PR" },
                new() { Value = "PE", Text = "PE" },
                new() { Value = "PI", Text = "PI" },
                new() { Value = "RJ", Text = "RJ" },
                new() { Value = "RN", Text = "RN" },
                new() { Value = "RS", Text = "RS" },
                new() { Value = "RO", Text = "RO" },
                new() { Value = "RR", Text = "RR" },
                new() { Value = "SC", Text = "SC" },
                new() { Value = "SP", Text = "SP" },
                new() { Value = "SE", Text = "SE" },
                new() { Value = "TO", Text = "TO" }
            };
        }
    }
}