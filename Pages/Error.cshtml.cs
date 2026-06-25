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
        public string ErrorSuggestion { get; set; } = "Tente novamente ou contacte o suporte.";

        public void OnGet(int? statusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorStatusCode = statusCode;

            if (statusCode == 400)
            {
                ErrorTitle = "Requisição Inválida (400)";
                ErrorMessage = "A requisição enviada contém dados inválidos ou mal formatados.";
                ErrorSuggestion = "Verifique os dados que enviou e tente novamente.";
            }
            else if (statusCode == 401)
            {
                ErrorTitle = "Não Autenticado (401)";
                ErrorMessage = "Precisa de iniciar sessão para aceder a este recurso.";
                ErrorSuggestion = "Faça login na sua conta e tente novamente.";
            }
            else if (statusCode == 403)
            {
                ErrorTitle = "Acesso Proibido (403)";
                ErrorMessage = "Não tem permissões suficientes para aceder a esta página.";
                ErrorSuggestion = "Contacte o administrador se acredita que deveria ter acesso.";
            }
            else if (statusCode == 404)
            {
                ErrorTitle = "Página Não Encontrada (404)";
                ErrorMessage = "A página que procura não existe, foi movida ou está temporariamente indisponível.";
                ErrorSuggestion = "Verifique o URL ou utilize a navegação do site.";
            }
            else if (statusCode == 405)
            {
                ErrorTitle = "Método Não Permitido (405)";
                ErrorMessage = "O método HTTP utilizado não é suportado para este recurso.";
                ErrorSuggestion = "Utilize o método correto para esta operação.";
            }
            else if (statusCode == 415)
            {
                ErrorTitle = "Tipo de Dados Não Suportado (415)";
                ErrorMessage = "O formato dos dados enviados não é suportado pelo servidor.";
                ErrorSuggestion = "Utilize um formato de dados suportado (ex: JSON).";
            }
            else if (statusCode == 429)
            {
                ErrorTitle = "Demasiados Pedidos (429)";
                ErrorMessage = "Enviou demasiados pedidos num curto período de tempo.";
                ErrorSuggestion = "Aguarde alguns segundos e tente novamente.";
            }
            else if (statusCode >= 500)
            {
                ErrorTitle = $"Erro do Servidor ({statusCode})";
                ErrorMessage = "Ocorreu um erro inesperado no servidor.";
                ErrorSuggestion = "A equipa foi notificada. Por favor, tente mais tarde.";
            }
            else if (statusCode == 0 || statusCode == null)
            {
                ErrorTitle = "Erro Desconhecido";
                ErrorMessage = "Ocorreu um erro inesperado ao processar o seu pedido.";
                ErrorSuggestion = "Tente novamente ou contacte o suporte.";
            }
        }
    }
}