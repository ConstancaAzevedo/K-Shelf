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
    public class CreateModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        // construtor que recebe os servicos por injecao de dependencias
        public CreateModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // propriedade que recebe os dados do formulario por binding
        [BindProperty]
        public Solista Solista { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public IActionResult OnGet()
        {
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync()
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return Page(); // se houver erros, volta para a pagina

            try
            {
                // adiciona o solista ao contexto
                _context.Solistas.Add(Solista);
                await _context.SaveChangesAsync(); // guarda na base de dados

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Solista",
                    Acao = "Criado",
                    Mensagem = $"Novo solista '{Solista.Nome}' foi criado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Solista \"{Solista.Nome}\" criado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de solistas
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao criar solista: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}