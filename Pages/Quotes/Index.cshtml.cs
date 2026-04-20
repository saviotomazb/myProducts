using Microsoft.AspNetCore.Mvc;
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

        [BindProperty]
        public QuoteItemViewModel InputItem { get; set; } = new();

        public List<SelectListItem> Clients { get; set; } = new();
        public List<ProductViewModel> Products { get; set; } = new();

        List<QuoteItemViewModel>? items;

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

            if (!ModelState.IsValid)
                return Page();

            if (string.IsNullOrWhiteSpace(ItemsJson))
                return Fail("Nenhum item enviado.");

            try
            {
                items = JsonSerializer.Deserialize<List<QuoteItemViewModel>>(ItemsJson);
            }
            catch
            {
                return Fail("Formato de itens inválido.");
            }

            if (items == null || !items.Any())
                return Fail("Adicione pelo menos um item.");

            if (items.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
                return Fail("Produto duplicado no orçamento.");

            if (items.Count > 50)
                return Fail("Limite de itens excedido.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Fail("Usuário não autenticado.");

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                Log.Warning("Usuário inválido: {ClaimValue}", userIdClaim.Value);
                return Fail("Usuário inválido.");
            }

            if (!Input.ClientId.HasValue)
            {
                ModelState.AddModelError("Input.ClientId", "Cliente é obrigatório.");
                return Page();
            }

            var clientExists = await _db.Clients
                .AnyAsync(c => c.ClientId == Input.ClientId.Value && c.IsActive);

            if (!clientExists)
                return Fail("Cliente inválido.");

            if (!Input.ValidUntil.HasValue || Input.ValidUntil.Value < DateTime.Today)
            {
                ModelState.AddModelError("Input.ValidUntil", "Data de validade inválida.");
                return Page();
            }

            var productIds = items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _db.Products
                .Where(p => productIds.Contains(p.ProductId) && p.IsActive)
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                Log.Warning(
                    "Inconsistência de produtos. Enviados: {SentCount}, Encontrados: {FoundCount}",
                    productIds.Count,
                    products.Count
                );

                return Fail("Um ou mais produtos são inválidos.");
            }

            var productDict = products.ToDictionary(p => p.ProductId);

            var quoteItems = new List<Quoteitem>();
            decimal total = 0;

            foreach (var item in items)
            {
                if (item.ProductId <= 0)
                {
                    Log.Warning("Produto inválido recebido: {ProductId}", item.ProductId);
                    return Fail("Produto inválido.");
                }

                if (!productDict.TryGetValue(item.ProductId, out var product) || product == null)
                    return Fail("Produto não encontrado.");

                if (item.Quantity is <= 0 or > 1000)
                    return Fail("Quantidade inválida.");

                if (product.Price <= 0)
                    return Fail("Produto com preço inválido.");

                var subtotal = product.Price * item.Quantity;

                var quoteItem = new Quoteitem
                {
                    ProductId = item.ProductId,
                    UnitPrice = product.Price,
                    Subtotal = subtotal,
                    Quantity = item.Quantity
                };

                total += subtotal;
                quoteItems.Add(quoteItem);
            }

            if (total <= 0)
                return Fail("Total inválido.");

            var quote = new Quote
            {
                ClientId = Input.ClientId.Value,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = QuoteStatus.Pendente,
                ValidUntil = Input.ValidUntil.Value,
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

                Log.ForContext("SourceContext", "myProducts.Pages.Quotes.Index").Information(
                    "Orçamento criado {QuoteId} para Cliente {ClientId} por Usuário {UserId} com Total {Total}",
                    quote.QuoteId,
                    quote.ClientId,
                    quote.UserId,
                    quote.TotalAmount
                );
                TempData["SuccessMessage"] = "Orçamento criado com sucesso!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao criar orçamento para Cliente {ClientId} pelo Usuário {UserId}",
                    Input.ClientId,
                    userId);
                await transaction.RollbackAsync();
                return Fail("Ocorreu um erro ao criar o orçamento.");
            }
        }

        private IActionResult Fail(string message)
        {
            ModelState.AddModelError("", message);
            return Page();
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
    }

    public static class QuoteStatus
    {
        public const string Pendente = "Pendente";
    }
}