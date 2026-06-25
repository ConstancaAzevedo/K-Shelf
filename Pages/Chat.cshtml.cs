using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Pages
{
    /// <summary>
    /// Modelo de Página de suporte para o Chat Global.
    /// Exige autenticação de utilizador ([Authorize]) para aceder à sala de conversa em tempo real.
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    public class ChatModel : PageModel
    {
        /// <summary>
        /// Invocado quando o utilizador acede à página de Chat.
        /// Como a comunicação corre em tempo real via JS, serve de ponto de entrada padrão.
        /// </summary>
        public void OnGet()
        {
            // Página de tempo real baseada em SignalR
        }
    }
}
