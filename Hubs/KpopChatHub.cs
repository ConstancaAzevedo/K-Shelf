using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace K_Shelf.Hubs
{
    /// <summary>
    /// hub do signalr para gerir a comunicacao em tempo real da aplicacao k-shelf
    /// trata das mensagens do chat global de k-pop, contador de utilizadores online e mensagens do sistema
    /// </summary>
    public class KpopChatHub : Hub
    {
        // contador global de utilizadores ligados
        private static int _onlineUsers = 0;
        private static readonly object _lock = new();

        // lista de utilizadores online (connectionid -> username)
        private static readonly ConcurrentDictionary<string, string> _onlineUsersList = new();

        /// <summary>
        /// executado quando um utilizador estabelece ligacao ao hub
        /// incrementa o contador online, adiciona a lista e propaga a todos
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            // incrementa o contador de utilizadores online de forma segura
            lock (_lock)
            {
                _onlineUsers++;
            }

            // obtem o nome do utilizador
            var username = GetUsername();
            // adiciona o utilizador a lista de online
            _onlineUsersList.TryAdd(Context.ConnectionId, username);

            // avisa todos os browsers conectados sobre o novo numero de utilizadores online
            await Clients.All.SendAsync("UpdateOnlineCount", _onlineUsers);
            await Clients.All.SendAsync("UpdateOnlineUsers", _onlineUsersList.Values); // atualiza a lista de utilizadores online

            // mensagem de sistema: utilizador entrou
            await Clients.All.SendAsync("SystemMessage", $"{username} entrou no chat");

            // chama o metodo base para manter o comportamento padrao
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// executado quando um utilizador fecha a pagina ou se desconecta do hub
        /// decrementa o contador online, remove da lista e avisa todos os browsers
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // obtem o nome do utilizador antes de o remover
            var username = GetUsername();
            // remove o utilizador da lista de online
            _onlineUsersList.TryRemove(Context.ConnectionId, out _);

            // decrementa o contador de utilizadores online de forma segura
            lock (_lock)
            {
                _onlineUsers = Math.Max(0, _onlineUsers - 1);
            }

            // avisa todos os browsers conectados sobre o novo numero de utilizadores online
            await Clients.All.SendAsync("UpdateOnlineCount", _onlineUsers);
            await Clients.All.SendAsync("UpdateOnlineUsers", _onlineUsersList.Values); // atualiza a lista de utilizadores online

            // mensagem de sistema: utilizador saiu
            await Clients.All.SendAsync("SystemMessage", $"{username} saiu do chat");

            // chama o metodo base para manter o comportamento padrao
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// recebe uma mensagem do chat enviada por um browser cliente e transmite-a a todos
        /// os utilizadores ativos no chat, juntamente com o nome do remetente e data
        /// </summary>
        /// <param name="message">Conteúdo textual da mensagem.</param>
        [Authorize(Roles = "User,Admin")] // apenas utilizadores autenticados podem enviar mensagens
        public async Task SendMessage(string message)
        {
            // obtem o nome do utilizador
            var username = GetUsername();
            // obtem a hora atual no formato horas:minutos
            var timestamp = DateTime.Now.ToString("HH:mm");

            // envia a mensagem a todos os clientes conectados
            await Clients.All.SendAsync("ReceiveMessage", username, message, timestamp);
        }

        /// <summary>
        /// notifica os outros utilizadores que alguem esta a escrever
        /// </summary>
        public async Task SendTyping(string user)
        {
            // envia para todos exceto o remetente que o utilizador esta a escrever
            await Clients.Others.SendAsync("UserTyping", user);
        }

        /// <summary>
        /// notifica os outros utilizadores que alguem parou de escrever
        /// </summary>
        public async Task StopTyping(string user)
        {
            // envia para todos exceto o remetente que o utilizador parou de escrever
            await Clients.Others.SendAsync("UserStoppedTyping", user);
        }

        /// <summary>
        /// notifica os outros utilizadores que alguem entrou no chat
        /// </summary>
        /// <param name="user">nome do utilizador que entrou</param>
        public async Task UserJoined(string user)
        {
            // envia para todos exceto o remetente uma mensagem de sistema
            await Clients.Others.SendAsync("SystemMessage", $"{user} entrou no chat");
        }

        /// <summary>
        /// obtem o nome do utilizador a partir do contexto da ligacao
        /// se o utilizador nao estiver autenticado, retorna "anonimo"
        /// se o nome tiver um @, remove a parte do dominio
        /// </summary>
        private string GetUsername()
        {
            // tenta obter o nome do utilizador autenticado
            var username = Context.User?.Identity?.Name ?? "Anónimo";
            // se o nome tiver @, fica apenas com a parte antes do @
            if (username.Contains("@"))
            {
                username = username.Split('@')[0];
            }
            return username;
        }
    }
}