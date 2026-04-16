using System.ComponentModel.DataAnnotations;

namespace myProducts.Models.ViewModels.Quotes
{
    public class QuoteItemViewModel
    {
        [Required(ErrorMessage = "Selecione um produto.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que 0.")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
