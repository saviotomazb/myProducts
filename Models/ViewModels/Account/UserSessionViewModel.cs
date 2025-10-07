using System.ComponentModel.DataAnnotations;

namespace myProducts.Models.ViewModels.Account
{
    public class UserSessionViewModel
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
