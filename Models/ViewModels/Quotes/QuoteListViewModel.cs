using System.Globalization;

namespace myProducts.Models.ViewModels.Quotes
{
    public class QuoteListViewModel
    {
        public int QuoteId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Status { get; set; } = string.Empty;

        public string CreatedAtFormatted =>
            TimeZoneInfo.ConvertTimeFromUtc(
                CreatedAt,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")
            ).ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));

        public string StatusFormatted =>
            Status == "Aprovado"
                ? "Aprovado"
                : (ValidUntil < DateTime.Today ? "Expirado" : "Pendente");
    }
}
