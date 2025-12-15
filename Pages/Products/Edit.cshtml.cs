using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Products;
using Log = Serilog.Log;

namespace myProducts.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly MyproductsContext _db;

        [BindProperty]
        public ProductViewModel ProductVM { get; set; } = null!;

        public List<SelectListItem> CategoriesList { get; set; } = new List<SelectListItem>();

        public EditModel(MyproductsContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {

            await LoadCategoriesAsync();

            //Busca o produto correspondente ao Id para preencher o formulário de edição.
            var product = await _db.Products.FindAsync(id);

            if(product == null)
            {
                return NotFound();
            }

            ProductVM = new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CreatedAt = product.CreatedAt,
                LastModified = product.LastModified
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = User.Identity?.Name ?? "";

            var product = await _db.Products.FindAsync(ProductVM.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            ProductVM.CreatedAt = product.CreatedAt;
            ProductVM.LastModified = product.LastModified;

            if (!ModelState.IsValid)
            {
                Log.Warning("Dados inválidos ao acessar a página para editar o produto {ProductId}. Usuário: {username}", ProductVM.ProductId, username);
                await LoadCategoriesAsync();
                return Page();
            }

            product.Name = ProductVM.Name;
            product.Description = ProductVM.Description;
            product.CategoryId = ProductVM.CategoryId;
            product.IsActive = ProductVM.IsActive;
            product.LastModified = DateTime.UtcNow;

            try
            {
                _db.Update(product);
                await _db.SaveChangesAsync();

                Log.ForContext("SourceContext", "myProducts.Pages.Products.Edit")
                    .Information("Produto {ProductId} atualizado por {username}", product.ProductId, username);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao atualizar o produto {ProductId} pelo usuário {username}", product.ProductId, username);
                ModelState.AddModelError(string.Empty, "Erro ao salvar alterações. Tente novamente!");

                await LoadCategoriesAsync();
                return Page();
            }

            return RedirectToPage("/Products/Index");
        }

        private async Task LoadCategoriesAsync()
        {
            CategoriesList = await _db.Categories
            .Where(c => c.IsActive)
            .Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            })
            .ToListAsync();
        }
    }
}