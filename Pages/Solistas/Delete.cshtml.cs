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
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext;

        public DeleteModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Solista Solista { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do solista não fornecido.";
                return NotFound();
            }

            var solista = await _context.Solistas.FindAsync(id);
            if (solista == null)
            {
                TempData["ErrorMessage"] = "Solista não encontrado.";
                return NotFound();
            }

            Solista = solista;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do solista não fornecido.";
                return NotFound();
            }

            var solista = await _context.Solistas.FindAsync(id);
            if (solista == null)
            {
                TempData["ErrorMessage"] = "Solista não encontrado.";
                return NotFound();
            }

            try
            {
                var nome = solista.Nome;
                _context.Solistas.Remove(solista);
                await _context.SaveChangesAsync();

                // Notificação SignalR
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Solista",
                    Acao = "Deletado",
                    Mensagem = $"🗑️ Solista '{nome}' foi removido!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"✅ Solista \"{nome}\" eliminado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Erro ao eliminar solista: {ex.Message}";
                return Page();
            }
        }
    }
}