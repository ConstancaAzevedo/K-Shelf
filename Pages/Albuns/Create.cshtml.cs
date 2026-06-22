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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Album Album { get; set; } = new Album();

        public SelectList GruposSelectList { get; set; } = default!;
        public SelectList SolistasSelectList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await CarregarSelectLists();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validação: tem de ter pelo menos um artista associado
            if (!Album.GrupoId.HasValue && !Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum tem de estar associado a um Grupo ou a um Solista.");
            }

            // Não pode ter os dois ao mesmo tempo
            if (Album.GrupoId.HasValue && Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum só pode estar associado a um Grupo OU a um Solista, não a ambos.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarSelectLists();
                return Page();
            }

            _context.Albuns.Add(Album);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Álbum \"{Album.Titulo}\" criado com sucesso!";
            return RedirectToPage("./Index");
        }

        private async Task CarregarSelectLists()
        {
            var grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .ToListAsync();

            var solistas = await _context.Solistas
                .OrderBy(s => s.Nome)
                .ToListAsync();

            GruposSelectList = new SelectList(grupos, "Id", "Nome");
            SolistasSelectList = new SelectList(solistas, "Id", "Nome");
        }
    }
}
