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

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync()
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return Page(); // se houver erros, volta para a pagina

            try
            {
                // procura o grupo original pelo id
                var grupoOriginal = await _context.Grupos.FindAsync(Grupo.Id);
                // se o grupo nao existir, retorna erro
                if (grupoOriginal == null)
                {
                    TempData["ErrorMessage"] = "Grupo não encontrado.";
                    return NotFound(); // retorna erro 404
                }

                // atualiza os campos editaveis do grupo
                grupoOriginal.Nome = Grupo.Nome; // atualiza o nome
                grupoOriginal.DataEstreia = Grupo.DataEstreia; // atualiza a data de estreia
                grupoOriginal.Companhia = Grupo.Companhia; // atualiza a companhia
                grupoOriginal.Fansigno = Grupo.Fansigno; // atualiza o fansigno
                grupoOriginal.ImagemUrl = Grupo.ImagemUrl; // atualiza a url da imagem
                grupoOriginal.IsAtivo = Grupo.IsAtivo; // atualiza o status ativo/inativo

                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Grupo",
                    Acao = "Editado",
                    Mensagem = $"Grupo '{Grupo.Nome}' foi atualizado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Grupo \"{Grupo.Nome}\" atualizado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de grupos
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao atualizar grupo: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}