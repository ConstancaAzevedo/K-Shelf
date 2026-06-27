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

        // propriedade que recebe os dados da colecao por binding
        [BindProperty]
        public Colecao Colecao { get; set; } = default!;

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
                return NotFound(); // retorna erro 404

            // procura a colecao pelo id
            var colecao = await _context.Colecoes.FirstOrDefaultAsync(c => c.Id == id);

            // se a colecao nao existir, retorna erro
            if (colecao == null)
                return NotFound(); // retorna erro 404

            // verifica permissao: so o dono ou admin pode editar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid(); // retorna erro 403

            // atribui a colecao a propriedade da pagina
            Colecao = colecao;
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario e submetido via post
        // com validacao do token antiforgery
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // remove a validacao do utilizadorid porque nao pode ser alterado
            ModelState.Remove("Colecao.UtilizadorId");

            // validacoes manuais

            // nome obrigatorio
            if (string.IsNullOrWhiteSpace(Colecao.Nome))
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção é obrigatório.");
            }

            // nome com minimo de 3 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length < 3)
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção deve ter pelo menos 3 caracteres.");
            }

            // nome com maximo de 100 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length > 100)
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção não pode exceder 100 caracteres.");
            }

            // descricao com maximo de 500 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Descricao) && Colecao.Descricao.Length > 500)
            {
                ModelState.AddModelError("Colecao.Descricao", "A descrição não pode exceder 500 caracteres.");
            }

            // se houver erros de validacao volta a pagina
            if (!ModelState.IsValid)
                return Page();

            // procura a colecao original pelo id
            var colecaoExistente = await _context.Colecoes.FindAsync(Colecao.Id);
            // se a colecao nao existir, retorna erro
            if (colecaoExistente == null)
                return NotFound(); // retorna erro 404

            // verifica permissao: so o dono ou admin pode editar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecaoExistente.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid(); // retorna erro 403

            // valida se ja existe outra colecao com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == colecaoExistente.UtilizadorId &&
                               c.Id != Colecao.Id && // exclui a propria colecao
                               c.Nome.ToLower() == Colecao.Nome.ToLower());

            // se existir, mostra erro
            if (colecaoDuplicada)
            {
                ModelState.AddModelError("Colecao.Nome", $"Já existe uma coleção com o nome \"{Colecao.Nome}\"!");
                return Page();
            }

            // atualiza apenas os campos editaveis
            colecaoExistente.Nome = Colecao.Nome; // atualiza o nome
            colecaoExistente.Descricao = Colecao.Descricao; // atualiza a descricao

            try
            {
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Coleção",
                    Acao = "Editada",
                    Mensagem = $"Coleção '{Colecao.Nome}' foi atualizada!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Coleção \"{Colecao.Nome}\" atualizada com sucesso!";
                return RedirectToPage("./Details", new { id = Colecao.Id }); // redireciona para os detalhes
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se a colecao ainda existe
                if (!await _context.Colecoes.AnyAsync(c => c.Id == Colecao.Id))
                    return NotFound(); // retorna erro 404
                throw; // relanca a excecao
            }
        }
    }
}