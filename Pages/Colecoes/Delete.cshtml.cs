using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace K_Shelf.Pages.Colecoes
{
    // requer autenticacao para aceder a esta pagina
    [Authorize]
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

        // propriedade que recebe os dados da colecao por binding
        [BindProperty]
        public Colecao Colecao { get; set; } = default!;

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da coleção não fornecido";
                return NotFound(); // retorna erro 404
            }

            // procura a colecao pelo id com os albuns associados
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes!) // inclui a relacao com os albuns
                    .ThenInclude(ac => ac.Album) // inclui os dados do album
                .FirstOrDefaultAsync(c => c.Id == id);

            // se a colecao nao existir, retorna erro
            if (colecao == null)
            {
                TempData["ErrorMessage"] = "Coleção não encontrada";
                return NotFound(); // retorna erro 404
            }

            // verifica permissao: so o dono ou admin pode eliminar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Não tem permissão para eliminar esta coleção";
                return Forbid(); // retorna erro 403
            }

            // atribui a colecao a propriedade da pagina
            Colecao = colecao;

            // aviso se houver albuns na colecao
            if (colecao.AlbumColecoes != null && colecao.AlbumColecoes.Any())
            {
                TempData["WarningMessage"] = $"Esta coleção contém {colecao.AlbumColecoes.Count} álbum(ns). Ao eliminar, estas relações serão perdidas (os álbuns não são eliminados).";
            }

            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario de confirmacao e submetido via post
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da coleção não fornecido";
                return NotFound(); // retorna erro 404
            }

            // procura a colecao pelo id com os albuns associados
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes) // inclui a relacao com os albuns
                .FirstOrDefaultAsync(c => c.Id == id);

            // se a colecao nao existir, retorna erro
            if (colecao == null)
            {
                TempData["ErrorMessage"] = "Coleção não encontrada";
                return NotFound(); // retorna erro 404
            }

            // verifica permissao: so o dono ou admin pode eliminar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Não tem permissão para eliminar esta coleção";
                return Forbid(); // retorna erro 403
            }

            try
            {
                // guarda o nome da colecao e o numero de albuns antes de eliminar
                var nomeColecao = colecao.Nome;
                var numAlbuns = colecao.AlbumColecoes?.Count ?? 0;

                // remove as relacoes albumcolecao primeiro (para evitar conflitos de chave estrangeira)
                if (colecao.AlbumColecoes != null)
                    _context.AlbumColecoes.RemoveRange(colecao.AlbumColecoes);

                // remove a colecao
                _context.Colecoes.Remove(colecao);
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Coleção",
                    Acao = "Deletada",
                    Mensagem = $"Coleção '{nomeColecao}' foi removida!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Coleção \"{nomeColecao}\" eliminada com sucesso! ({numAlbuns} álbum(ns) removidos)";
                return RedirectToPage("./Index"); // redireciona para a lista de colecoes
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao eliminar a coleção: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}