using System.ComponentModel.DataAnnotations;

namespace myProducts.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O nome completo é obrigatório")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}