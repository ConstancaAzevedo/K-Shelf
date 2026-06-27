using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace K_Shelf.Hubs
{
    /// <summary>
    /// hub do signalr para notificacoes em tempo real sobre alteracoes no sistema
    /// notifica os clientes quando artistas, albuns ou colecoes sao criados/editados/removidos
    /// </summary>
    [Authorize] // apenas utilizadores autenticados podem receber notificacoes
    public class NotificacaoHub : Hub
    {
        // notifica todos os clientes que um artista foi criado
        public async Task NotificarArtistaCriado(string artistaNome)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Artista",
                Acao = "Criado",
                Mensagem = $"Novo artista '{artistaNome}' foi adicionado!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que um artista foi editado
        public async Task NotificarArtistaEditado(string artistaNome)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Artista",
                Acao = "Editado",
                Mensagem = $"Artista '{artistaNome}' foi atualizado!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que um artista foi removido
        public async Task NotificarArtistaDeletado(string artistaNome)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Artista",
                Acao = "Deletado",
                Mensagem = $"Artista '{artistaNome}' foi removido!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que um album foi criado
        public async Task NotificarAlbumCriado(string albumTitulo)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Álbum",
                Acao = "Criado",
                Mensagem = $"Novo álbum '{albumTitulo}' foi adicionado!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que um album foi editado
        public async Task NotificarAlbumEditado(string albumTitulo)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Álbum",
                Acao = "Editado",
                Mensagem = $"Álbum '{albumTitulo}' foi atualizado!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que um album foi removido
        public async Task NotificarAlbumDeletado(string albumTitulo)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Álbum",
                Acao = "Deletado",
                Mensagem = $"Álbum '{albumTitulo}' foi removido!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que uma colecao foi criada
        public async Task NotificarColecaoCriada(string colecaoNome)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Coleção",
                Acao = "Criada",
                Mensagem = $"Nova coleção '{colecaoNome}' foi criada!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que uma colecao foi editada
        public async Task NotificarColecaoEditada(string colecaoNome)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Coleção",
                Acao = "Editada",
                Mensagem = $"Coleção '{colecaoNome}' foi atualizada!",
                Data = DateTime.Now
            });
        }

        // notifica todos os clientes que uma colecao foi removida
        public async Task NotificarColecaoDeletada(string colecaoNome)
        {
            await Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Coleção",
                Acao = "Deletada",
                Mensagem = $"Coleção '{colecaoNome}' foi removida!",
                Data = DateTime.Now
            });
        }
    }
}