using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Albuns
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Album Album { get; set; } = default!;

        public int ColecoesCont { get; set; }
        public int MusicasCont { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do álbum não fornecido";
                return NotFound();
            }

            var album = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Include(a => a.Musicas)
                .Include(a => a.AlbumColecoes)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return NotFound();
            }

            Album = album;
            ColecoesCont = album.AlbumColecoes?.Count ?? 0;
            MusicasCont = album.Musicas?.Count ?? 0;

            if (ColecoesCont > 0)
            {
                TempData["WarningMessage"] = $"Este álbum está em {ColecoesCont} coleção(ões). Ao eliminar, será removido dessas coleções.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do álbum não fornecido";
                return NotFound();
            }

            var album = await _context.Albuns
                .Include(a => a.AlbumColecoes)
                .Include(a => a.Musicas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return NotFound();
            }

            try
            {
                var titulo = album.Titulo;
                var numColecoes = album.AlbumColecoes?.Count ?? 0;
                var numMusicas = album.Musicas?.Count ?? 0;

                // Remover relações primeiro
                if (album.AlbumColecoes != null && album.AlbumColecoes.Any())
                {
                    _context.AlbumColecoes.RemoveRange(album.AlbumColecoes);
                }

                if (album.Musicas != null && album.Musicas.Any())
                {
                    _context.Musicas.RemoveRange(album.Musicas);
                }

                _context.Albuns.Remove(album);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Álbum \"{titulo}\" eliminado com sucesso! ({numMusicas} música(s), {numColecoes} coleção(ões) removidas)";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao eliminar álbum: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }
    }
}
