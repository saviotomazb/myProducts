using System.ComponentModel.DataAnnotations;

namespace myProducts.Models.ViewModels.Quotes

{
    public class QuoteViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Selecione um cliente.")]
        public int? ClientId { get; set; }

        [MinLength(1, ErrorMessage = "Adicione pelo menos um item ao orçamento.")]
        public List<QuoteItemViewModel> Items { get; set; } = new();

        [DataType(DataType.Date)]
        public DateTime? ValidUntil { get; set; }

        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres.")]
        public string? Notes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Items == null || !Items.Any())
            {
                yield return new ValidationResult(
                    "O orçamento deve possuir pelo menos um item.",
                    new[] { nameof(Items) }
                );
            }
        }
    }
}
