using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Products;
using myProducts.Models.ViewModels.Quotes;
using System.Security.Claims;
using System.Text.Json;
using Log = Serilog.Log;

namespace myProducts.Pages.Quotes
{
    public class IndexModel : PageModel
    {
        private readonly MyproductsContext _db;

        [BindProperty]
        public QuoteViewModel Input { get; set; } = new();

        public List<SelectListItem> Clients { get; set; } = new();

        public List<ProductViewModel> Products { get; set; } = new();

        [BindProperty]
        public string ItemsJson { get; set; } = string.Empty;

        public IndexModel(MyproductsContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            await LoadClientsAsync();
            await LoadProductsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadClientsAsync();
            await LoadProductsAsync();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                ModelState.AddModelError("", "Usuário não autenticado.");
                return Page();
            }

            ValidateForm();
            var items = ValidateItems();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (items == null)
                return Page();

            var productIds = items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _db.Products
                .Where(p => productIds.Contains(p.ProductId) && p.IsActive)
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                ModelState.AddModelError("", "Um ou mais produtos são inválidos.");
                return Page();
            }

            var productDict = products.ToDictionary(p => p.ProductId);

            var quoteItems = new List<Quoteitem>();
            decimal total = 0;

            foreach (var item in items)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product))
                {
                    ModelState.AddModelError("", "Produto inválido.");
                    return Page();
                }

                if (item.Quantity is <= 0 or > 1000)
                {
                    ModelState.AddModelError("", "Quantidade inválida.");
                    return Page();
                }

                var subtotal = product.Price * item.Quantity;

                total += subtotal;

                quoteItems.Add(new Quoteitem
                {
                    ProductId = item.ProductId,
                    UnitPrice = product.Price,
                    Subtotal = subtotal,
                    Quantity = item.Quantity
                });
            }

            if (total <= 0)
            {
                ModelState.AddModelError("", "Total inválido.");
                return Page();
            }

            if (Input.ClientId is not int clientId || Input.ValidUntil is not DateTime validUntil)
            {
                return Page();
            }

            var quote = new Quote
            {
                ClientId = clientId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = QuoteStatus.Pendente,
                ValidUntil = validUntil,
                Notes = Input.Notes,
                TotalAmount = total
            };

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _db.Quotes.Add(quote);
                await _db.SaveChangesAsync();

                foreach (var qi in quoteItems)
                {
                    qi.QuoteId = quote.QuoteId;
                    _db.Quoteitems.Add(qi);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.ForContext("SourceContext", "myProducts.Pages.Quotes.Index").Information
                    ("Orçamento cadastrado com sucesso: {QuoteId}", quote.QuoteId);

                TempData["SuccessMessage"] = "Orçamento cadastrado com sucesso!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Quotes.Index").Error
                    (ex, "Erro ao cadastrar orçamento");
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Erro ao salvar orçamento.");
                return Page();
            }
        }

        private async Task LoadClientsAsync()
        {
            Clients = await _db.Clients
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.FullName
                })
                .ToListAsync();

            Clients.Insert(0, new SelectListItem { Value = "", Text = "Selecione" });
        }

        private async Task LoadProductsAsync()
        {
            Products = await _db.Products
                .Where(p => p.IsActive)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price
                })
                .ToListAsync();
        }
        public static class QuoteStatus
        {
            public const string Pendente = "Pendente";
        }

        private void ValidateForm()
        {
            if (!Input.ClientId.HasValue)
                ModelState.AddModelError("Input.ClientId", "Cliente é obrigatório.");

            if (!Input.ValidUntil.HasValue)
                ModelState.AddModelError("Input.ValidUntil", "Data é obrigatória.");

            else if (Input.ValidUntil.Value < DateTime.Today)
                ModelState.AddModelError("Input.ValidUntil", "Data inválida.");
        }

        private List<QuoteItemViewModel>? ValidateItems()
        {
            if (string.IsNullOrWhiteSpace(ItemsJson))
            {
                ModelState.AddModelError("", "Adicione pelo menos um item.");
                return null;
            }

            try
            {
                var items = JsonSerializer.Deserialize<List<QuoteItemViewModel>>(ItemsJson);

                if (items == null || !items.Any())
                {
                    ModelState.AddModelError("", "Adicione pelo menos um item.");
                    return null;
                }

                if (items.Any(i => i.Quantity <= 0))
                {
                    ModelState.AddModelError("", "Itens com quantidade inválida.");
                    return null;
                }

                if (items.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
                {
                    ModelState.AddModelError("", "Produto duplicado.");
                    return null;
                }

                if (items.Count > 50)
                {
                    ModelState.AddModelError("", "Limite de itens excedido.");
                    return null;
                }

                return items;
            }
            catch
            {
                ModelState.AddModelError("", "Itens inválidos.");
                return null;
            }
        }
    }
}