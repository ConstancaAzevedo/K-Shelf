using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Albuns
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Album Album { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var album = await _context.Albuns
                .Include(a => a.Grupo)
                    .ThenInclude(g => g!.Artistas)
                .Include(a => a.Solista)
                .Include(a => a.Musicas!.OrderBy(m => m.TrackNumber))
                .Include(a => a.AlbumColecoes!)
                    .ThenInclude(ac => ac.Colecao)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return NotFound();

            Album = album;
            return Page();
        }
    }
}
