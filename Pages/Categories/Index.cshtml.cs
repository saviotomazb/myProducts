using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Categories;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using Log = Serilog.Log;

namespace myProducts.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly MyproductsContext _db;

        public List<CategoryViewModel> CategoriesVM { get; set; } = [];

        public IndexModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchCategory { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        public async Task OnGet()
        {
            await LoadCategoriesAsync();
        }

        [BindProperty]
        public CategoryViewModel Input { get; set; } = null!;

        private async Task LoadCategoriesAsync()
        {
            ModelState.Remove(nameof(SearchCategory));

            IQueryable<Category> query = _db.Categories;

            if (!string.IsNullOrWhiteSpace(SearchCategory))
            {
                query = query.Where(q => EF.Functions.Like(q.Name, $"%{SearchCategory}%"));
            }

            // Calcula o total de páginas usando o tamanho definido para cada página (PageSize).
            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            PageNumber = Math.Max(1, PageNumber);
            PageNumber = Math.Min(PageNumber, TotalPages > 0 ? TotalPages : 1);

            CategoriesVM = await query
                .OrderBy(c => c.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new CategoryViewModel
                {
                    Id = c.CategoryId,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    LastModified = c.LastModified,
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove(nameof(SearchCategory));

            var username = User.Identity?.Name ?? "";

            if (!ModelState.IsValid)
            {
                Log.Warning("Dados inválidos ao acessar a página Categorias. Usuário: {username}", username);
                await LoadCategoriesAsync();
                return Page();
            }

            if (await _db.Categories.AnyAsync(c => c.Name == Input.Name))
            {
                Log.ForContext("SourceContext", "myProducts.Pages.Categories.Index").Information
                    ("Tentativa de cadastro com categoria já existente: {Name}", Input.Name);
            ModelState.AddModelError("Input.Name", $"A categoria '{Input.Name}' já foi criada");
                await LoadCategoriesAsync();
                return Page();
            }

            var category = new Category
            {
                Name = Input.Name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            Log.ForContext("SourceContext", "myProducts.Pages.Categories.Index").Information
                    ("Nova categoria cadastrada: {Name}", Input.Name);

            TempData["SuccessMessage"] = $"A categoria '{Input.Name}' foi cadastrada com sucesso!";

            return RedirectToPage();
        }
    }
}