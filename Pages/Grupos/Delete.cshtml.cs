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
        public Grupo Grupo { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do grupo não fornecido.";
                return NotFound();
            }

            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
            {
                TempData["ErrorMessage"] = "Grupo não encontrado.";
                return NotFound();
            }

            Grupo = grupo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do grupo não fornecido.";
                return NotFound();
            }

            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
            {
                TempData["ErrorMessage"] = "Grupo não encontrado.";
                return NotFound();
            }

            try
            {
                var nome = grupo.Nome;
                _context.Grupos.Remove(grupo);
                await _context.SaveChangesAsync();

                // Notificação SignalR
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Grupo",
                    Acao = "Deletado",
                    Mensagem = $"🗑️ Grupo '{nome}' foi removido!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"✅ Grupo \"{nome}\" eliminado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Erro ao eliminar grupo: {ex.Message}";
                return Page();
            }
        }
    }
}