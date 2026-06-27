using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Musicas
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

        // propriedade que recebe os dados da musica por binding
        [BindProperty]
        public Musica Musica { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da música não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura a musica pelo id com o album associado
            var musica = await _context.Musicas
                .Include(m => m.Album) // inclui o album associado
                .FirstOrDefaultAsync(m => m.Id == id);

            // se a musica nao existir, retorna erro
            if (musica == null)
            {
                TempData["ErrorMessage"] = "Música não encontrada.";
                return NotFound(); // retorna erro 404
            }

            // atribui a musica a propriedade da pagina
            Musica = musica;
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario de confirmacao e submetido via post
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da música não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura a musica pelo id
            var musica = await _context.Musicas.FindAsync(id);
            // se a musica nao existir, retorna erro
            if (musica == null)
            {
                TempData["ErrorMessage"] = "Música não encontrada.";
                return NotFound(); // retorna erro 404
            }

            try
            {
                // guarda o titulo da musica antes de a eliminar
                var titulo = musica.Titulo;
                // remove a musica do contexto
                _context.Musicas.Remove(musica);
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Música",
                    Acao = "Deletada",
                    Mensagem = $"Música '{titulo}' foi removida!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Música \"{titulo}\" eliminada com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de musicas
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao eliminar música: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}