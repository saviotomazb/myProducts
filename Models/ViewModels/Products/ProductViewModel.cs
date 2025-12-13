using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace myProducts.Models.ViewModels.Products
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do produto é obrigatório")]
        [StringLength(100, ErrorMessage = "O produto deve ter no máximo 100 caracteres")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "O campo de descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public string IsActiveFormatted => IsActive ? "Sim" : "Não";

        public DateTime CreatedAt { get; set; }

        public string CreatedAtFormatted => TimeZoneInfo.ConvertTimeFromUtc(CreatedAt, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                                                      .ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));

        public DateTime LastModified { get; set; }

        public string LastModifiedFormatted => TimeZoneInfo.ConvertTimeFromUtc(LastModified, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                                                      .ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));
    }
}
