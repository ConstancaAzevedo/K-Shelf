using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Artistas
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Artista? Artista { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Artista = await _context.Artistas
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Artista == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}