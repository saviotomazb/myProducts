using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Clients;

namespace myProducts.Pages.Clients
{
    public class ListModel : PageModel
    {
        private readonly MyproductsContext _db;
        public ListModel(MyproductsContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchClient { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        public List<ClientViewModel> ClientsVM { get; set; } = new();


        public async Task OnGetAsync()
        {
            var query = _db.Clients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchClient))
            {
                query = query.Where(c => EF.Functions.Like(c.FullName, $"%{SearchClient}%"));
            }

            var totalItems = await query.CountAsync();

            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            PageNumber = PageNumber < 1 ? 1 : PageNumber;

            if (TotalPages > 0 && PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
            }

            ClientsVM = await query
                .OrderBy(c => c.FullName)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ClientViewModel
                {
                    ClientId = c.ClientId,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    City = c.City,
                    State = c.State,
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

    }
}
