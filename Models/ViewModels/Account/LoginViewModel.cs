using System.ComponentModel.DataAnnotations;

namespace myProducts.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Digite o usuário")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite a senha")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
