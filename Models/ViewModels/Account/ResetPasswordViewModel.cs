using System.ComponentModel.DataAnnotations;

namespace myProducts.Models.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Informe a nova senha")]
        [DataType(DataType.Password)]
        public string? Newpassword { get; set; }

        [Required(ErrorMessage = "Confirme a nova senha")]
        [DataType(DataType.Password)]
        public string? Confirmnewpassword { get; set; }
    }
}
