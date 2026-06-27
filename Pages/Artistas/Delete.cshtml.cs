using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Artistas
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

        // propriedade que recebe os dados do artista por binding
        [BindProperty]
        public Artista? Artista { get; set; }

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do artista não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura o artista pelo id com os dados relacionados
            Artista = await _context.Artistas
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .Include(a => a.Albuns) // inclui os albuns associados
                .FirstOrDefaultAsync(m => m.Id == id);

            // se o artista nao existir, retorna erro
            if (Artista == null)
            {
                TempData["ErrorMessage"] = "Artista não encontrado.";
                return NotFound(); // retorna erro 404
            }

            // aviso se houver albuns associados ao artista
            if (Artista.Albuns != null && Artista.Albuns.Any())
            {
                TempData["WarningMessage"] = $"Este artista tem {Artista.Albuns.Count} álbum(ns) associados. Ao eliminar, estes álbuns também serão removidos.";
            }

            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario de confirmacao e submetido via post
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do artista não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura o artista pelo id com os albuns associados
            var artista = await _context.Artistas
                .Include(a => a.Albuns) // inclui os albuns associados
                .FirstOrDefaultAsync(m => m.Id == id);

            // se o artista nao existir, retorna erro
            if (artista == null)
            {
                TempData["ErrorMessage"] = "Artista não encontrado.";
                return NotFound(); // retorna erro 404
            }

            try
            {
                // guarda o nome do artista e o numero de albuns antes de eliminar
                var nome = artista.NomeExibicao;
                var numAlbuns = artista.Albuns?.Count ?? 0;

                // remove os albuns associados ao artista
                if (artista.Albuns != null && artista.Albuns.Any())
                {
                    _context.Albuns.RemoveRange(artista.Albuns); // remove todos os albuns
                }

                // remove o artista
                _context.Artistas.Remove(artista);
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Artista",
                    Acao = "Deletado",
                    Mensagem = $"Artista '{nome}' foi removido!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Artista \"{nome}\" eliminado com sucesso! ({numAlbuns} álbum(ns) removidos)";
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao eliminar artista: {ex.Message}";
            }

            return RedirectToPage("./Index"); // redireciona para a lista de artistas
        }
    }
}