// gerado automaticamente pelo identity
// pagina de erro personalizada para a aplicacao

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Areas.Identity.Pages
{
    [AllowAnonymous] // qualquer utilizador pode aceder a esta pagina
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // nao guarda em cache
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; } // id do pedido que originou o erro

        // mostra o request id se existir
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // metodo executado quando a pagina e carregada
        public void OnGet()
        {
            // obtem o id do pedido atual
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}