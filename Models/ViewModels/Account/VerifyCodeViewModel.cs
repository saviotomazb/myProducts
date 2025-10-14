using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myProducts.Models.ViewModels.Account
{
    public class VerifyCodeViewModel
    {
        [Required]
        [StringLength(1)]
        public string? Digit1 { get; set; }

        [Required]
        [StringLength(1)]
        public string? Digit2 { get; set; }

        [Required]
        [StringLength(1)]
        public string? Digit3 { get; set; }

        [Required]
        [StringLength(1)]
        public string? Digit4 { get; set; }

        [Required]
        [StringLength(1)]
        public string? Digit5 { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Informe o código enviado por e-mail")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "O código deve ter 5 dígitos")]
        public string Code => $"{Digit1}{Digit2}{Digit3}{Digit4}{Digit5}";
    }
}