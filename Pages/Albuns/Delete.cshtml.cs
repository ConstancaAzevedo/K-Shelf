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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var album = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Include(a => a.Musicas)
                .Include(a => a.AlbumColecoes)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return NotFound();

            Album = album;
            ColecoesCont = album.AlbumColecoes?.Count ?? 0;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var album = await _context.Albuns
                .Include(a => a.AlbumColecoes)
                .Include(a => a.Musicas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return NotFound();

            // Remover relações primeiro (AlbumColecao e Musicas)
            if (album.AlbumColecoes != null)
                _context.AlbumColecoes.RemoveRange(album.AlbumColecoes);

            if (album.Musicas != null)
                _context.Musicas.RemoveRange(album.Musicas);

            _context.Albuns.Remove(album);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Álbum \"{album.Titulo}\" eliminado com sucesso.";
            return RedirectToPage("./Index");
        }
    }
}
