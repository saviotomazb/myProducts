using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace myProducts.Models.ViewModels.Categories
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(100, ErrorMessage = "A categoria deve ter no máximo 100 caracteres")]
        public required string Name { get; set; }

        public bool IsActive { get; set; }

        public string IsActiveFormatted => IsActive ? "Sim" : "Não";

        public DateTime CreatedAt { get; set; }

        public string CreatedAtFormatted => TimeZoneInfo.ConvertTimeFromUtc(CreatedAt, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                                                      .ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));

        public DateTime LastModified {  get; set; }

        public string LastModifiedFormatted => TimeZoneInfo.ConvertTimeFromUtc(LastModified, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                                                      .ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));
    }
}