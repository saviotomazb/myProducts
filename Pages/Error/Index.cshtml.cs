using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace myProducts.Pages.Error
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            //Garantir que o usuário acesse a página APENAS quando o middleware redirecionar devido a um erro ou status code
            var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (statusFeature == null && exceptionFeature == null)
            {
                if (User.Identity?.IsAuthenticated ?? false)
                {
                    //Redirecionamento para usuários logados
                    Response.Redirect("/Home");
                }
                else
                {
                    //Redirecionamento para usuários não logados
                    Response.Redirect("/Account/Login");
                }
            }
        }
    }
}
