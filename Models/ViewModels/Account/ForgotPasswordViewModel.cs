using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace myProducts.Models.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Informe um e-mail ou usuário")]
        public string? User_email { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!(User_email is null || !User_email.Contains('@')))
            {
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(User_email, pattern))
                {
                    yield return new ValidationResult("E-mail inválido.", new[] { nameof(User_email) });
                }
            }
        }
    }
}