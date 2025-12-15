using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Products;
using Log = Serilog.Log;

namespace myProducts.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly MyproductsContext _db;

        public List<ProductViewModel> ProductsVM { get; set; } = [];

        public List<SelectListItem> CategoriesList { get; set; } = new List<SelectListItem>();

        public IndexModel(MyproductsContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            await LoadProductsAsync();
            await LoadCategoriesAsync();
        }

        [BindProperty(SupportsGet = true)]
        public string SearchProduct { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        [BindProperty]
        public ProductViewModel Input { get; set; } = null!;

        private async Task LoadProductsAsync()
        {
            ModelState.Remove(nameof(SearchProduct));

            IQueryable<Product> query = _db.Products;

            if (!string.IsNullOrWhiteSpace(SearchProduct))
            {
                query = query.Where(q => EF.Functions.Like(q.Name, $"%{SearchProduct}%"));
            }

            // Calcula o total de páginas usando o tamanho definido para cada página (PageSize).
            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            PageNumber = Math.Max(1, PageNumber);
            PageNumber = Math.Min(PageNumber, TotalPages > 0 ? TotalPages : 1);

            ProductsVM = await query
                .OrderBy(c => c.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ProductViewModel
                {
                    ProductId = c.ProductId,
                    Name = c.Name,
                    Description = c.Description,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category.Name,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    LastModified = c.LastModified,
                })
                .ToListAsync();
        }

        //Carregar menu com a lista de categorias disponíveis, e ativas
        private async Task LoadCategoriesAsync()
        {
            CategoriesList = await _db.Categories.Where(c => c.IsActive).Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove(nameof(SearchProduct));

            var username = User.Identity?.Name ?? "";

            if (!ModelState.IsValid)
            {
                Log.Warning("Dados inválidos ao acessar a página Produtos. Usuário: {username}", username);
                await LoadProductsAsync();
                await LoadCategoriesAsync();
                return Page();
            }

            if (await _db.Products.AnyAsync(p => p.Name == Input.Name))
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Products.Index").Information
                    ("Tentativa de cadastro de produto já existente: {Name}", Input.Name);
                ModelState.AddModelError("Input.Name", $"O produto '{Input.Name}' já foi criado");
                await LoadProductsAsync();
                await LoadCategoriesAsync();
                return Page();
            }

            var product = new Product
            {
                Name = Input.Name,
                Description = Input.Description,
                CategoryId = Input.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            Log.ForContext("SourceContext", "myProducts.Pages.Products.Index").Information
                    ("Novo produto cadastrado: {Name}", Input.Name);

            TempData["SuccessMessage"] = $"O produto '{Input.Name}' foi cadastrado com sucesso!";

            return RedirectToPage();
        }
    }
}
