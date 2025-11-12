using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using myProducts.Models;
using myProducts.Models.ViewModels.Categories;
using Log = Serilog.Log;

namespace myProducts.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly MyproductsContext _db;

        [BindProperty]
        public CategoryViewModel CategoryVM { get; set; } = null!;

        public EditModel(MyproductsContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            //Busca a categoria correspondente ao Id para preencher o formulário de edição.
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            CategoryVM = new CategoryViewModel
            {
                Id = category.CategoryId,
                Name = category.Name,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                LastModified = category.LastModified,
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = User.Identity?.Name ?? "";

            var category = await _db.Categories.FindAsync(CategoryVM.Id);

            if (category == null)
            {
                return NotFound();
            }

            CategoryVM.CreatedAt = category.CreatedAt;
            CategoryVM.LastModified = category.LastModified;

            if (!ModelState.IsValid)
            {
                Log.Warning("Dados inválidos ao acessar a página para editar a categoria {CategoryId}. Usuário: {username}", CategoryVM.Id, username);
                return Page();
            }

            category.Name = CategoryVM.Name;
            category.IsActive = CategoryVM.IsActive;
            category.LastModified = DateTime.UtcNow;

            try
            {
                _db.Update(category);
                await _db.SaveChangesAsync();

                Log.ForContext("SourceContext", "myProducts.Pages.Categories.Edit")
                    .Information("Categoria {CategoryId} atualizada por {username}", category.CategoryId, username);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao atualizar a categoria {CategoryId} pelo usuário {username}", category.CategoryId, username);
                ModelState.AddModelError(string.Empty, "Erro ao salvar alterações. Tente novamente!");
                return Page();
            }

            return RedirectToPage("/Categories/Index");
        }
    }
}