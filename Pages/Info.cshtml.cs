using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Pages
{
    /// <summary>
    /// Modelo de suporte para a página de Informações/Créditos (Info.cshtml).
    /// Como a página é maioritariamente estática, serve apenas de ponto de entrada padrão do Razor Pages.
    /// </summary>
    public class InfoModel : PageModel
    {
        /// <summary>
        /// Método chamado na requisição GET da página.
        /// Não executa processamento adicional, uma vez que a página renderiza conteúdo estático (bibliotecas, frameworks e créditos).
        /// </summary>
        public void OnGet()
        {
            // Página estática - não precisa de lógica
        }
    }
}
