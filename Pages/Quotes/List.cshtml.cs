using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myProducts.Models;
using myProducts.Models.ViewModels.Quotes;

namespace myProducts.Pages.Quotes
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

        public List<QuoteListViewModel> Quotes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _db.Quotes
                .AsNoTracking()
                .Include(q => q.Client)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchClient))
            {
                query = query.Where(q =>
                    EF.Functions.Like(q.Client.FullName, $"%{SearchClient}%"));
            }

            var totalItems = await query.CountAsync();

            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            PageNumber = PageNumber < 1 ? 1 : PageNumber;

            if (TotalPages > 0 && PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
            }

            Quotes = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(q => new QuoteListViewModel
                {
                    QuoteId = q.QuoteId,
                    ClientName = q.Client.FullName,
                    TotalAmount = q.TotalAmount,
                    CreatedAt = q.CreatedAt,
                    ValidUntil = q.ValidUntil,
                    Status = q.Status
                })
                .ToListAsync();
        }
    }
}