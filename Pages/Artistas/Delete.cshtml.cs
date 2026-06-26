using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Artistas
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext; // NOVO


        public DeleteModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;

        }

        [BindProperty]
        public Artista? Artista { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do artista não fornecido.";
                return NotFound();
            }

            Artista = await _context.Artistas
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Include(a => a.Albuns)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Artista == null)
            {
                TempData["ErrorMessage"] = "Artista não encontrado.";
                return NotFound();
            }

            if (Artista.Albuns != null && Artista.Albuns.Any())
            {
                TempData["WarningMessage"] = $"Este artista tem {Artista.Albuns.Count} álbum(ns) associados. Ao eliminar, estes álbuns também serão removidos.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do artista não fornecido.";
                return NotFound();
            }

            var artista = await _context.Artistas
                .Include(a => a.Albuns)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (artista == null)
            {
                TempData["ErrorMessage"] = "Artista não encontrado.";
                return NotFound();
            }

            try
            {
                var nome = artista.NomeExibicao;
                var numAlbuns = artista.Albuns?.Count ?? 0;

                // Remover álbuns associados
                if (artista.Albuns != null && artista.Albuns.Any())
                {
                    _context.Albuns.RemoveRange(artista.Albuns);
                }

                _context.Artistas.Remove(artista);
                await _context.SaveChangesAsync();

                // notificação em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Artista",
                    Acao = "Deletado",
                    Mensagem = $"Artista '{nome}' foi removido!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Artista \"{nome}\" eliminado com sucesso! ({numAlbuns} álbum(ns) removidos)";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao eliminar artista: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }
    }
}