using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Pages
{
    /// <summary>
    /// modelo de suporte para a pagina de informacoes/creditos (info.cshtml)
    /// como a pagina e maioritariamente estatica, serve apenas de ponto de entrada padrao do razor pages
    /// </summary>
    public class InfoModel : PageModel
    {
        /// <summary>
        /// metodo chamado na requisicao get da pagina
        /// nao executa processamento adicional, uma vez que a pagina renderiza conteudo estatico (bibliotecas, frameworks e creditos)
        /// </summary>
        public void OnGet()
        {
            // pagina estatica - nao precisa de logica
            // todo o conteudo e apresentado diretamente no ficheiro .cshtml
        }
    }
}