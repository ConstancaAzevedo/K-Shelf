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
        public Colecao Colecao { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da coleção não fornecido";
                return NotFound();
            }

            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
            {
                TempData["ErrorMessage"] = "Coleção não encontrada";
                return NotFound();
            }

            // Só o dono ou Admin pode eliminar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Não tem permissão para eliminar esta coleção";
                return Forbid();
            }

            Colecao = colecao;

            // Aviso se houver álbuns na coleção
            if (colecao.AlbumColecoes != null && colecao.AlbumColecoes.Any())
            {
                TempData["WarningMessage"] = $"Esta coleção contém {colecao.AlbumColecoes.Count} álbum(ns). Ao eliminar, estas relações serão perdidas (os álbuns não são eliminados).";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da coleção não fornecido";
                return NotFound();
            }

            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
            {
                TempData["ErrorMessage"] = "Coleção não encontrada";
                return NotFound();
            };

            // Verificar permissão
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Não tem permissão para eliminar esta coleção";
                return Forbid();
            }

            try
            {
                var nomeColecao = colecao.Nome;
                var numAlbuns = colecao.AlbumColecoes?.Count ?? 0;

                // Remover relações AlbumColecao primeiro
                if (colecao.AlbumColecoes != null)
                    _context.AlbumColecoes.RemoveRange(colecao.AlbumColecoes);

                _context.Colecoes.Remove(colecao);
                await _context.SaveChangesAsync();

                // notificação em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Coleção",
                    Acao = "Deletada",
                    Mensagem = $"🗑️ Coleção '{nomeColecao}' foi removida!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Coleção \"{nomeColecao}\" eliminada com sucesso! ({numAlbuns} álbum(ns) removidos)";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao eliminar a coleção: {ex.Message}";
                return Page();
            }
        }
    }
}