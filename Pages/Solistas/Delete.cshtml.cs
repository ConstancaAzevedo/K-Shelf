using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Solistas
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

        // propriedade que recebe os dados do solista por binding
        [BindProperty]
        public Solista Solista { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do solista não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura o solista pelo id
            var solista = await _context.Solistas.FindAsync(id);
            // se o solista nao existir, retorna erro
            if (solista == null)
            {
                TempData["ErrorMessage"] = "Solista não encontrado.";
                return NotFound(); // retorna erro 404
            }

            // atribui o solista a propriedade da pagina
            Solista = solista;
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario de confirmacao e submetido via post
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do solista não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura o solista pelo id
            var solista = await _context.Solistas.FindAsync(id);
            // se o solista nao existir, retorna erro
            if (solista == null)
            {
                TempData["ErrorMessage"] = "Solista não encontrado.";
                return NotFound(); // retorna erro 404
            }

            try
            {
                // guarda o nome do solista antes de o eliminar
                var nome = solista.Nome;
                // remove o solista do contexto
                _context.Solistas.Remove(solista);
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Solista",
                    Acao = "Deletado",
                    Mensagem = $"Solista '{nome}' foi removido!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Solista \"{nome}\" eliminado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de solistas
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao eliminar solista: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}