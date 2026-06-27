using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Pages
{
    /// <summary>
    /// modelo de pagina de suporte para o chat global
    /// exige autenticacao de utilizador ([authorize]) para aceder a sala de conversa em tempo real
    /// </summary>
    [Authorize(Roles = "User,Admin")] // apenas utilizadores com os roles user ou admin podem aceder ao chat
    public class ChatModel : PageModel
    {
        /// <summary>
        /// invocado quando o utilizador acede a pagina de chat
        /// como a comunicacao corre em tempo real via js, serve de ponto de entrada padrao
        /// </summary>
        public void OnGet()
        {
            // pagina de tempo real baseada em signalr
            // a logica do chat esta no lado do cliente (javascript com signalr)
        }
    }
}