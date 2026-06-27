using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace K_Shelf.Pages
{
    // pagina de erro personalizada
    // nao guarda em cache para garantir que a pagina de erro e sempre atualizada
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken] // ignora o token antiforgery (pagina publica)
    public class ErrorModel : PageModel
    {
        // id do pedido que originou o erro
        public string? RequestId { get; set; }

        // indica se o request id deve ser mostrado
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // codigo de status do erro (ex: 404, 500, etc)
        public int? ErrorStatusCode { get; set; }

        // titulo do erro
        public string ErrorTitle { get; set; } = "Erro Inesperado";

        // mensagem de erro
        public string ErrorMessage { get; set; } = "Ocorreu um erro ao processar o seu pedido.";

        // sugestao para resolver o erro
        public string ErrorSuggestion { get; set; } = "Tente novamente ou contacte o suporte.";

        // metodo executado quando a pagina e carregada via get
        // recebe o codigo de status como parametro opcional
        public void OnGet(int? statusCode)
        {
            // obtem o id do pedido atual
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            // guarda o codigo de status
            ErrorStatusCode = statusCode;

            // define a mensagem com base no codigo de status
            if (statusCode == 400)
            {
                // requisicao invalida
                ErrorTitle = "Requisição Inválida (400)";
                ErrorMessage = "A requisição enviada contém dados inválidos ou mal formatados.";
                ErrorSuggestion = "Verifique os dados que enviou e tente novamente.";
            }
            else if (statusCode == 401)
            {
                // nao autenticado
                ErrorTitle = "Não Autenticado (401)";
                ErrorMessage = "Precisa de iniciar sessão para aceder a este recurso.";
                ErrorSuggestion = "Faça login na sua conta e tente novamente.";
            }
            else if (statusCode == 403)
            {
                // acesso proibido
                ErrorTitle = "Acesso Proibido (403)";
                ErrorMessage = "Não tem permissões suficientes para aceder a esta página.";
                ErrorSuggestion = "Contacte o administrador se acredita que deveria ter acesso.";
            }
            else if (statusCode == 404)
            {
                // pagina nao encontrada
                ErrorTitle = "Página Não Encontrada (404)";
                ErrorMessage = "A página que procura não existe, foi movida ou está temporariamente indisponível.";
                ErrorSuggestion = "Verifique o URL ou utilize a navegação do site.";
            }
            else if (statusCode == 405)
            {
                // metodo nao permitido
                ErrorTitle = "Método Não Permitido (405)";
                ErrorMessage = "O método HTTP utilizado não é suportado para este recurso.";
                ErrorSuggestion = "Utilize o método correto para esta operação.";
            }
            else if (statusCode == 415)
            {
                // tipo de dados nao suportado
                ErrorTitle = "Tipo de Dados Não Suportado (415)";
                ErrorMessage = "O formato dos dados enviados não é suportado pelo servidor.";
                ErrorSuggestion = "Utilize um formato de dados suportado (ex: JSON).";
            }
            else if (statusCode == 429)
            {
                // demasiados pedidos
                ErrorTitle = "Demasiados Pedidos (429)";
                ErrorMessage = "Enviou demasiados pedidos num curto período de tempo.";
                ErrorSuggestion = "Aguarde alguns segundos e tente novamente.";
            }
            else if (statusCode >= 500)
            {
                // erro do servidor
                ErrorTitle = $"Erro do Servidor ({statusCode})";
                ErrorMessage = "Ocorreu um erro inesperado no servidor.";
                ErrorSuggestion = "A equipa foi notificada. Por favor, tente mais tarde.";
            }
            else if (statusCode == 0 || statusCode == null)
            {
                // erro desconhecido
                ErrorTitle = "Erro Desconhecido";
                ErrorMessage = "Ocorreu um erro inesperado ao processar o seu pedido.";
                ErrorSuggestion = "Tente novamente ou contacte o suporte.";
            }
        }
    }
}