using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Artistas
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Artista Artista { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null)
            {
                return NotFound();
            }

            Artista = artista;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Artista).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Artista '{Artista.NomeExibicao}' atualizado com sucesso!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Artistas.Any(a => a.Id == Artista.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToPage("./Index");
        }
    }
}