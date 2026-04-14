using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using myProducts.Helpers;
using myProducts.Models;
using myProducts.Models.ViewModels.Clients;
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
            States = StateHelper.GetSelectList();

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
            States = StateHelper.GetSelectList();

            if (!ModelState.IsValid)
                return Page();

            if (ClientVM.ClientId == 0)
                return NotFound();

            var client = await _db.Clients.FindAsync(ClientVM.ClientId);

            if (client == null)
                return NotFound();

            client.FullName = ClientVM.FullName;
            client.PhoneNumber = PhoneHelper.Clean(ClientVM.PhoneNumber);
            client.City = ClientVM.City;
            client.State = StateHelper.Normalize(ClientVM.State);
            client.Street = ClientVM.Street;
            client.District = ClientVM.District;
            client.Complement = ClientVM.Complement;
            client.HouseNumber = ClientVM.HouseNumber;
            client.IsActive = ClientVM.IsActive;

            try
            {
                await _db.SaveChangesAsync();

                Log.ForContext("SourceContext", "myProducts.Pages.Clients.Edit")
                   .Information("Cliente atualizado com sucesso: {Name}", client.FullName);

                TempData["SuccessMessage"] = "Cliente atualizado com sucesso!";

                return RedirectToPage("/Clients/List");
            }
            catch (Exception ex)
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Clients.Edit")
                    .Error(ex, "Erro ao atualizar o cliente: {Name}", client.FullName);

                ModelState.AddModelError(string.Empty, "Erro ao atualizar o cliente. Tente novamente.");

                return Page();
            }
            
        }
    }
}
