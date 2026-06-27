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
    public class EditModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        // construtor que recebe os servicos por injecao de dependencias
        public EditModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
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

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync()
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return Page(); // se houver erros, volta para a pagina

            try
            {
                // procura o solista original pelo id
                var solistaOriginal = await _context.Solistas.FindAsync(Solista.Id);
                // se o solista nao existir, retorna erro
                if (solistaOriginal == null)
                {
                    TempData["ErrorMessage"] = "Solista não encontrado.";
                    return NotFound(); // retorna erro 404
                }

                // atualiza os campos editaveis do solista
                solistaOriginal.Nome = Solista.Nome; // atualiza o nome
                solistaOriginal.DataEstreia = Solista.DataEstreia; // atualiza a data de estreia
                solistaOriginal.Companhia = Solista.Companhia; // atualiza a companhia
                solistaOriginal.ImagemUrl = Solista.ImagemUrl; // atualiza a url da imagem
                solistaOriginal.IsAtivo = Solista.IsAtivo; // atualiza o status ativo/inativo

                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Solista",
                    Acao = "Editado",
                    Mensagem = $"Solista '{Solista.Nome}' foi atualizado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Solista \"{Solista.Nome}\" atualizado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de solistas
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao atualizar solista: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}