using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Artistas
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artista = await _context.Artistas.FindAsync(id);
            if (artista != null)
            {
                var nome = artista.NomeExibicao;
                _context.Artistas.Remove(artista);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Artista '{nome}' eliminado com sucesso!";
            }

            return RedirectToPage("./Index");
        }
    }
}