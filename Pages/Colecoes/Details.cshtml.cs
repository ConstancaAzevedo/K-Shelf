using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Security.Claims;

namespace K_Shelf.Pages.Colecoes
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Colecao Colecao { get; set; } = default!;
        public bool IsOwner { get; set; }

        // Para adicionar álbuns à coleção
        public SelectList AlbunsDisponiveis { get; set; } = default!;

        [BindProperty]
        public int AlbumIdParaAdicionar { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                        .ThenInclude(a => a!.Grupo)
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                        .ThenInclude(a => a!.Solista)
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound();

            // Só o dono ou Admin pode ver detalhes
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            Colecao = colecao;
            IsOwner = colecao.UtilizadorId == userId;

            // Carregar álbuns que ainda não estão na coleção
            var albumIdsNaColecao = colecao.AlbumColecoes?.Select(ac => ac.AlbumId).ToList() ?? new List<int>();
            var albunsPossiveis = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Where(a => !albumIdsNaColecao.Contains(a.Id))
                .OrderBy(a => a.Titulo)
                .ToListAsync();

            AlbunsDisponiveis = new SelectList(
                albunsPossiveis.Select(a => new {
                    a.Id,
                    Nome = $"{a.Titulo} — {a.Grupo?.Nome ?? a.Solista?.Nome ?? "Independente"}"
                }),
                "Id", "Nome"
            );

            return Page();
        }

        // Adicionar álbum à coleção
        public async Task<IActionResult> OnPostAdicionarAlbumAsync(int id)
        {
            var colecao = await _context.Colecoes.FindAsync(id);
            if (colecao == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Verificar se o álbum já está na coleção
            var jaExiste = await _context.AlbumColecoes
                .AnyAsync(ac => ac.AlbumId == AlbumIdParaAdicionar && ac.ColecaoId == id);

            if (!jaExiste && AlbumIdParaAdicionar > 0)
            {
                _context.AlbumColecoes.Add(new AlbumColecao
                {
                    AlbumId = AlbumIdParaAdicionar,
                    ColecaoId = id,
                    DataAdicao = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Álbum adicionado à coleção!";
            }

            return RedirectToPage("./Details", new { id });
        }

        // Remover álbum da coleção
        public async Task<IActionResult> OnPostRemoverAlbumAsync(int id, int albumId)
        {
            var colecao = await _context.Colecoes.FindAsync(id);
            if (colecao == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var albumColecao = await _context.AlbumColecoes
                .FirstOrDefaultAsync(ac => ac.AlbumId == albumId && ac.ColecaoId == id);

            if (albumColecao != null)
            {
                _context.AlbumColecoes.Remove(albumColecao);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Álbum removido da coleção.";
            }

            return RedirectToPage("./Details", new { id });
        }
    }
}
