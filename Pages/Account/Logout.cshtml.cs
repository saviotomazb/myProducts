using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Log = Serilog.Log;

namespace myProducts.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            var username = User.Identity?.Name ?? "";

            Log.ForContext("SourceContext", "myProducts.Pages.Account.Logout")
               .Information("Logout realizado pelo usuário {Username}", username);

            //Remove os cookies de autenticação, garantindo que o usuário não permaneça autenticado após o logout.
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");

            return RedirectToPage("/Account/Login");
        }
    }
}