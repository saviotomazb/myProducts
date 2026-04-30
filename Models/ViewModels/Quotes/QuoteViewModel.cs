using System.ComponentModel.DataAnnotations;

namespace myProducts.Models.ViewModels.Quotes

{
    public class QuoteViewModel
    {
        [Required(ErrorMessage = "Selecione um cliente.")]
        public int? ClientId { get; set; }

        [Required(ErrorMessage = "Informe a data de validade.")]
        [DataType(DataType.Date)]
        public DateTime? ValidUntil { get; set; }

        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres.")]
        public string? Notes { get; set; }
    }
}
