using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Grupos
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

        // propriedade que recebe os dados do grupo por binding
        [BindProperty]
        public Grupo Grupo { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do grupo não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura o grupo pelo id
            var grupo = await _context.Grupos.FindAsync(id);
            // se o grupo nao existir, retorna erro
            if (grupo == null)
            {
                TempData["ErrorMessage"] = "Grupo não encontrado.";
                return NotFound(); // retorna erro 404
            }

            // atribui o grupo a propriedade da pagina
            Grupo = grupo;
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario de confirmacao e submetido via post
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do grupo não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura o grupo pelo id
            var grupo = await _context.Grupos.FindAsync(id);
            // se o grupo nao existir, retorna erro
            if (grupo == null)
            {
                TempData["ErrorMessage"] = "Grupo não encontrado.";
                return NotFound(); // retorna erro 404
            }

            try
            {
                // guarda o nome do grupo antes de o eliminar
                var nome = grupo.Nome;
                // remove o grupo do contexto
                _context.Grupos.Remove(grupo);
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Grupo",
                    Acao = "Deletado",
                    Mensagem = $"Grupo '{nome}' foi removido!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Grupo \"{nome}\" eliminado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de grupos
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao eliminar grupo: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}