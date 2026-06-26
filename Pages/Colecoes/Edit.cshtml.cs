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
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext; 

        public EditModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Colecao Colecao { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var colecao = await _context.Colecoes.FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound();

            // Só o dono ou Admin pode editar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            Colecao = colecao;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            // validações manuais 

            // Nome obrigatório
            if (string.IsNullOrWhiteSpace(Colecao.Nome))
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção é obrigatório.");
            }

            // Nome com mínimo de 3 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length < 3)
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção deve ter pelo menos 3 caracteres.");
            }

            // Nome com máximo de 100 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length > 100)
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção não pode exceder 100 caracteres.");
            }

            // Descrição com máximo de 500 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Descricao) && Colecao.Descricao.Length > 500)
            {
                ModelState.AddModelError("Colecao.Descricao", "A descrição não pode exceder 500 caracteres.");
            }


            if (!ModelState.IsValid)
                return Page();

            var colecaoExistente = await _context.Colecoes.FindAsync(Colecao.Id);
            if (colecaoExistente == null)
                return NotFound();

            // Verificar que só o dono ou Admin pode editar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecaoExistente.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Validar se já existe outra coleção com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == colecaoExistente.UtilizadorId && 
                               c.Id != Colecao.Id && 
                               c.Nome.ToLower() == Colecao.Nome.ToLower());

            if (colecaoDuplicada)
            {
                ModelState.AddModelError("Colecao.Nome", $"Já existe uma coleção com o nome \"{Colecao.Nome}\"!");
                return Page();
            }

            // Atualizar apenas os campos editáveis
            colecaoExistente.Nome = Colecao.Nome;
            colecaoExistente.Descricao = Colecao.Descricao;

            try
            {
                await _context.SaveChangesAsync();

                // notificação em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Coleção",
                    Acao = "Editada",
                    Mensagem = $"Coleção '{Colecao.Nome}' foi atualizada!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Coleção \"{Colecao.Nome}\" atualizada com sucesso!";
                return RedirectToPage("./Details", new { id = Colecao.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Colecoes.AnyAsync(c => c.Id == Colecao.Id))
                    return NotFound();
                throw;
            }
        }
    }
}
