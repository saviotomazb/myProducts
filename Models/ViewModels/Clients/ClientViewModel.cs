using System.ComponentModel.DataAnnotations;
using System.Globalization;
using myProducts.Helpers;

namespace myProducts.Models.ViewModels.Clients
{
    public class ClientViewModel : IValidatableObject
    {
        public int ClientId { get; set; }

        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome completo deve ter no máximo 150 caracteres.")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "A cidade deve ter no máximo 50 caracteres.")]
        public string? City { get; set; }

        [StringLength(2, MinimumLength = 2, ErrorMessage = "O estado deve conter 2 caracteres.")]
        public string? State { get; set; }

        [StringLength(50, ErrorMessage = "A rua deve ter no máximo 50 caracteres.")]
        public string? Street { get; set; }

        [StringLength(50, ErrorMessage = "O bairro deve ter no máximo 50 caracteres.")]
        public string? District { get; set; }

        [StringLength(50, ErrorMessage = "O complemento deve ter no máximo 50 caracteres.")]
        public string? Complement { get; set; }

        [StringLength(15, ErrorMessage = "O número deve ter no máximo 15 caracteres.")]
        public string? HouseNumber { get; set; }

        [RegularExpression(@"^\(\d{2}\)\s\d{4,5}-\d{4}$", ErrorMessage = "Informe um telefone válido no formato (00) 00000-0000.")]
        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres.")]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedAtFormatted => TimeZoneInfo.ConvertTimeFromUtc(CreatedAt, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                                                      .ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));

        public bool IsActive { get; set; }

        public string IsActiveFormatted => IsActive ? "Sim" : "Não";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!StateHelper.IsValid(State))
            {
                yield return new ValidationResult(
                    "Selecione um estado válido.",
                    new[] { nameof(State) }
                );
            }
        }
    }
}
