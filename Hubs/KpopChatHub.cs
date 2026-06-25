using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace K_Shelf.Hubs
{
    /// <summary>
    /// Hub do SignalR para gerir a comunicação em tempo real da aplicação K-Shelf.
    /// Trata das mensagens do Chat Global de K-Pop e do contador global de utilizadores conectados.
    /// </summary>
    public class KpopChatHub : Hub
    {
        // Contador global de utilizadores ligados
        private static int _onlineUsers = 0;
        private static readonly object _lock = new();

        /// <summary>
        /// Método executado quando um utilizador estabelece ligação ao Hub.
        /// Incrementa o contador online e propaga-o a todos os utilizadores ligados.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            lock (_lock)
            {
                _onlineUsers++;
            }

            // Avisa todos os browsers conectados sobre o novo número de utilizadores online
            await Clients.All.SendAsync("UpdateOnlineCount", _onlineUsers);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Método executado quando um utilizador fecha a página ou se desconecta do Hub.
        /// Decrementa o contador online e avisa todos os browsers.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_lock)
            {
                _onlineUsers = Math.Max(0, _onlineUsers - 1);
            }

            // Avisa todos os browsers conectados sobre o novo número de utilizadores online
            await Clients.All.SendAsync("UpdateOnlineCount", _onlineUsers);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Recebe uma mensagem do chat enviada por um browser cliente e transmite-a a todos
        /// os utilizadores ativos no chat, juntamente com o nome do remetente e data.
        /// </summary>
        /// <param name="message">Conteúdo textual da mensagem.</param>
        [Authorize]
        public async Task SendMessage(string message)
        {
            // Obtém o nome ou email do utilizador autenticado (já garantido pelo [Authorize])
            var username = Context.User?.Identity?.Name ?? "Anónimo";

            // Se for um email, podemos tentar retirar a parte antes do @ para ficar mais curto e simpático
            if (username.Contains("@"))
            {
                username = username.Split('@')[0];
            }

            var timestamp = DateTime.Now.ToString("HH:mm");

            // Envia a mensagem a todos os clientes conectados
            await Clients.All.SendAsync("ReceiveMessage", username, message, timestamp);
        }
    }
}
