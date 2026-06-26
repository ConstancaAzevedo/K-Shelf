using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace K_Shelf.Hubs
{
    /// <summary>
    /// Hub do SignalR para notificações em tempo real sobre alterações no sistema.
    /// Notifica os clientes quando Artistas, Álbuns ou Coleções são criados/editados/deletados.
    /// </summary>
    [Authorize]
    public class NotificacaoHub : Hub
    {
        /// <summary>
        /// Notifica todos os clientes que um Artista foi criado.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que um Artista foi editado.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que um Artista foi deletado.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que um Álbum foi criado.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que um Álbum foi editado.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que um Álbum foi deletado.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que uma Coleção foi criada.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que uma Coleção foi editada.
        /// </summary>
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

        /// <summary>
        /// Notifica todos os clientes que uma Coleção foi deletada.
        /// </summary>
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