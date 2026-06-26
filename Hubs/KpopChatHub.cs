using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace K_Shelf.Hubs
{
    /// <summary>
    /// Hub do SignalR para gerir a comunicação em tempo real da aplicação K-Shelf.
    /// Trata das mensagens do Chat Global de K-Pop, contador de utilizadores online,
    /// indicador de "a escrever..." e mensagens do sistema.
    /// </summary>
    public class KpopChatHub : Hub
    {
        // Contador global de utilizadores ligados
        private static int _onlineUsers = 0;
        private static readonly object _lock = new();

        // Lista de utilizadores online (ConnectionId -> Username)
        private static readonly ConcurrentDictionary<string, string> _onlineUsersList = new();

        /// <summary>
        /// Método executado quando um utilizador estabelece ligação ao Hub.
        /// Incrementa o contador online, adiciona à lista e propaga a todos.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            lock (_lock)
            {
                _onlineUsers++;
            }

            var username = GetUsername();
            _onlineUsersList.TryAdd(Context.ConnectionId, username);

            // Avisa todos os browsers conectados sobre o novo número de utilizadores online
            await Clients.All.SendAsync("UpdateOnlineCount", _onlineUsers);
            await Clients.All.SendAsync("UpdateOnlineUsers", _onlineUsersList.Values);

            // Mensagem de sistema: utilizador entrou
            await Clients.All.SendAsync("SystemMessage", $"{username} entrou no chat");

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Método executado quando um utilizador fecha a página ou se desconecta do Hub.
        /// Decrementa o contador online, remove da lista e avisa todos os browsers.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = GetUsername();
            _onlineUsersList.TryRemove(Context.ConnectionId, out _);

            lock (_lock)
            {
                _onlineUsers = Math.Max(0, _onlineUsers - 1);
            }

            // Avisa todos os browsers conectados sobre o novo número de utilizadores online
            await Clients.All.SendAsync("UpdateOnlineCount", _onlineUsers);
            await Clients.All.SendAsync("UpdateOnlineUsers", _onlineUsersList.Values);

            // Mensagem de sistema: utilizador saiu
            await Clients.All.SendAsync("SystemMessage", $"{username} saiu do chat");

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Recebe uma mensagem do chat enviada por um browser cliente e transmite-a a todos
        /// os utilizadores ativos no chat, juntamente com o nome do remetente e data.
        /// </summary>
        /// <param name="message">Conteúdo textual da mensagem.</param>
        [Authorize(Roles = "User,Admin")]
        public async Task SendMessage(string message)
        {
            var username = GetUsername();
            var timestamp = DateTime.Now.ToString("HH:mm");

            // Envia a mensagem a todos os clientes conectados
            await Clients.All.SendAsync("ReceiveMessage", username, message, timestamp);
        }

        /// <summary>
        /// Notifica os outros utilizadores que alguém está a escrever.
        /// </summary>
        /// <param name="user">Nome do utilizador que está a escrever.</param>
        public async Task SendTyping(string user)
        {
            await Clients.Others.SendAsync("UserTyping", user);
        }

        /// <summary>
        /// Notifica os outros utilizadores que alguém parou de escrever.
        /// </summary>
        /// <param name="user">Nome do utilizador que parou de escrever.</param>
        public async Task StopTyping(string user)
        {
            await Clients.Others.SendAsync("UserStoppedTyping", user);
        }

        /// <summary>
        /// Notifica os outros utilizadores que alguém entrou no chat.
        /// </summary>
        /// <param name="user">Nome do utilizador que entrou.</param>
        public async Task UserJoined(string user)
        {
            await Clients.Others.SendAsync("SystemMessage", $"{user} entrou no chat");
        }

        /// <summary>
        /// Obtém o nome do utilizador a partir do contexto da ligação.
        /// </summary>
        private string GetUsername()
        {
            var username = Context.User?.Identity?.Name ?? "Anónimo";
            if (username.Contains("@"))
            {
                username = username.Split('@')[0];
            }
            return username;
        }
    }
}