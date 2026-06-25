using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace K_Shelf.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public int? ErrorStatusCode { get; set; }
        public string ErrorTitle { get; set; } = "Erro Inesperado";
        public string ErrorMessage { get; set; } = "Ocorreu um erro ao processar o seu pedido.";
        public string ErrorEmoji { get; set; } = "⚠️";

        public void OnGet(int? statusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorStatusCode = statusCode;

            if (statusCode == 404)
            {
                ErrorEmoji = "🔍";
                ErrorTitle = "Página Não Encontrada (404)";
                ErrorMessage = "A página ou recurso que procura não existe, foi movido ou está temporariamente indisponível.";
            }
            else if (statusCode == 403)
            {
                ErrorEmoji = "🔒";
                ErrorTitle = "Acesso Proibido (403)";
                ErrorMessage = "Não tem permissões suficientes para aceder a esta página ou recurso.";
            }
            else if (statusCode == 401)
            {
                ErrorEmoji = "🔑";
                ErrorTitle = "Não Autenticado (401)";
                ErrorMessage = "Precisa de iniciar sessão para aceder a este recurso.";
            }
            else if (statusCode >= 500)
            {
                ErrorEmoji = "💥";
                ErrorTitle = $"Erro do Servidor ({statusCode})";
                ErrorMessage = "Ocorreu um problema no servidor ao processar o seu pedido. Por favor, tente mais tarde.";
            }
        }
    }

}
