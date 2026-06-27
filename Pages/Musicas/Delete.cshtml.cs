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
        public Musica Musica { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da música não fornecido.";
                return NotFound();
            }

            var musica = await _context.Musicas
                .Include(m => m.Album)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (musica == null)
            {
                TempData["ErrorMessage"] = "Música não encontrada.";
                return NotFound();
            }

            Musica = musica;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da música não fornecido.";
                return NotFound();
            }

            var musica = await _context.Musicas.FindAsync(id);
            if (musica == null)
            {
                TempData["ErrorMessage"] = "Música não encontrada.";
                return NotFound();
            }

            try
            {
                var titulo = musica.Titulo;
                _context.Musicas.Remove(musica);
                await _context.SaveChangesAsync();

                // Notificação
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Música",
                    Acao = "Deletada",
                    Mensagem = $"Música '{titulo}' foi removida!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Música \"{titulo}\" eliminada com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao eliminar música: {ex.Message}";
                return Page();
            }
        }
    }
}