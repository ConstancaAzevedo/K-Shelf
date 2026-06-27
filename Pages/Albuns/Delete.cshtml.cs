using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Albuns
{
    // restringe o acesso apenas a utilizadores com o role admin
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        // construtor que recebe os servicos por injecao de dependencias
        public DeleteModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // propriedade que recebe os dados do album por binding
        [BindProperty]
        public Album Album { get; set; } = default!;

        // contadores para mostrar nas mensagens
        public int ColecoesCont { get; set; } // numero de colecoes onde o album aparece
        public int MusicasCont { get; set; } // numero de musicas do album

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do álbum não fornecido";
                return NotFound(); // retorna erro 404
            }

            // procura o album pelo id com os dados relacionados
            var album = await _context.Albuns
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .Include(a => a.Musicas) // inclui as musicas
                .Include(a => a.AlbumColecoes) // inclui as relacoes com colecoes
                .FirstOrDefaultAsync(a => a.Id == id);

            // se o album nao existir, retorna erro
            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return NotFound(); // retorna erro 404
            }

            // atribui o album a propriedade da pagina
            Album = album;
            // conta as colecoes e musicas
            ColecoesCont = album.AlbumColecoes?.Count ?? 0;
            MusicasCont = album.Musicas?.Count ?? 0;

            // aviso se o album estiver em colecoes
            if (ColecoesCont > 0)
            {
                TempData["WarningMessage"] = $"Este álbum está em {ColecoesCont} coleção(ões). Ao eliminar, será removido dessas coleções.";
            }

            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario de confirmacao e submetido via post
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do álbum não fornecido";
                return NotFound(); // retorna erro 404
            }

            // procura o album pelo id com as relacoes
            var album = await _context.Albuns
                .Include(a => a.AlbumColecoes) // inclui as relacoes com colecoes
                .Include(a => a.Musicas) // inclui as musicas
                .FirstOrDefaultAsync(a => a.Id == id);

            // se o album nao existir, retorna erro
            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return NotFound(); // retorna erro 404
            }

            try
            {
                // guarda dados para a mensagem de sucesso
                var titulo = album.Titulo;
                var numColecoes = album.AlbumColecoes?.Count ?? 0;
                var numMusicas = album.Musicas?.Count ?? 0;

                // remove as relacoes primeiro para evitar conflitos de chave estrangeira
                if (album.AlbumColecoes != null && album.AlbumColecoes.Any())
                {
                    _context.AlbumColecoes.RemoveRange(album.AlbumColecoes); // remove todas as relacoes com colecoes
                }

                if (album.Musicas != null && album.Musicas.Any())
                {
                    _context.Musicas.RemoveRange(album.Musicas); // remove todas as musicas
                }

                // remove o album
                _context.Albuns.Remove(album);
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Álbum",
                    Acao = "Deletado",
                    Mensagem = $"Álbum '{titulo}' foi removido!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Álbum \"{titulo}\" eliminado com sucesso! ({numMusicas} música(s), {numColecoes} coleção(ões) removidas)";
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao eliminar álbum: {ex.Message}";
            }

            return RedirectToPage("./Index"); // redireciona para a lista de albuns
        }
    }
}