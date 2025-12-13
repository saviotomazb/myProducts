using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using myProducts.Models;
using myProducts.Models.ViewModels.Categories;
using myProducts.Models.ViewModels.Products;

namespace myProducts.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly MyproductsContext _db;

        [BindProperty]
        public ProductViewModel ProductVM { get; set; } = null!;

        public List<SelectListItem> ProductsList { get; set; } = new List<SelectListItem>();

        public EditModel(MyproductsContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
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
                CategoryId = product.CategoryId,
                CreatedAt = product.CreatedAt,
                LastModified = product.LastModified
            };

            return Page();
        }
    }
}
