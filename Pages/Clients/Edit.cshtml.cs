using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using myProducts.Models;
using myProducts.Models.ViewModels.Clients;
using System.Text.RegularExpressions;
using Log = Serilog.Log;

namespace myProducts.Pages.Clients
{
    public class EditModel : PageModel
    {

        private readonly MyproductsContext _db;

        public EditModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty]
        public ClientViewModel ClientVM { get; set; } = new();

        public List<SelectListItem> States { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            LoadStates();

            if (id == null)
                return NotFound();

            var client = await _db.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            ClientVM = new ClientViewModel
            {
                ClientId = client.ClientId,
                FullName = client.FullName,
                PhoneNumber = client.PhoneNumber,
                City = client.City,
                State = client.State,
                Street = client.Street,
                District = client.District,
                Complement = client.Complement,
                HouseNumber = client.HouseNumber,
                CreatedAt = client.CreatedAt,
                IsActive = client.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadStates();

            if (!ModelState.IsValid)
                return Page();

            if (ClientVM.ClientId == 0)
                return NotFound();

            var client = await _db.Clients.FindAsync(ClientVM.ClientId);

            if (client == null)
                return NotFound();

            var cleanPhone = string.IsNullOrWhiteSpace(ClientVM.PhoneNumber)
                ? null
                : Regex.Replace(ClientVM.PhoneNumber, @"\D", "");

            client.FullName = ClientVM.FullName;
            client.PhoneNumber = cleanPhone;
            client.City = ClientVM.City;
            client.State = string.IsNullOrWhiteSpace(ClientVM.State) ? null : ClientVM.State;
            client.Street = ClientVM.Street;
            client.District = ClientVM.District;
            client.Complement = ClientVM.Complement;
            client.HouseNumber = ClientVM.HouseNumber;
            client.IsActive = ClientVM.IsActive;

            await _db.SaveChangesAsync();

            Log.ForContext("SourceContext", "myProducts.Pages.Clients.Edit")
               .Information("Cliente atualizado com sucesso: {Name}", client.FullName);

            TempData["SuccessMessage"] = "Cliente atualizado com sucesso!";

            return RedirectToPage("/Clients/List");
        }

        private void LoadStates()
        {
            States = new List<SelectListItem>
            {
                new() { Value = "", Text = "Selecione" },
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
