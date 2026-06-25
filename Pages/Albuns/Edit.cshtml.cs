using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Albuns
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
        public Album Album { get; set; } = default!;

        public SelectList GruposSelectList { get; set; } = default!;
        public SelectList SolistasSelectList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var album = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return NotFound();

            Album = album;
            await CarregarSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!Album.GrupoId.HasValue && !Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum tem de estar associado a um Grupo ou a um Solista.");
            }

            if (Album.GrupoId.HasValue && Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum só pode estar associado a um Grupo OU a um Solista, não a ambos.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarSelectLists();
                return Page();
            }

            _context.Attach(Album).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Albuns.AnyAsync(a => a.Id == Album.Id))
                    return NotFound();
                throw;
            }

            TempData["SuccessMessage"] = $"Álbum \"{Album.Titulo}\" atualizado com sucesso!";
            return RedirectToPage("./Index");
        }

        private async Task CarregarSelectLists()
        {
            var grupos = await _context.Grupos.OrderBy(g => g.Nome).ToListAsync();
            var solistas = await _context.Solistas.OrderBy(s => s.Nome).ToListAsync();

            GruposSelectList = new SelectList(grupos, "Id", "Nome", Album.GrupoId);
            SolistasSelectList = new SelectList(solistas, "Id", "Nome", Album.SolistaId);
        }
    }
}
