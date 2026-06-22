using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Security.Claims;

namespace K_Shelf.Pages.Colecoes
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Colecao Colecao { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound();

            // Só o dono ou Admin pode eliminar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            Colecao = colecao;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound();

            // Verificar permissão
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Remover relações AlbumColecao primeiro
            if (colecao.AlbumColecoes != null)
                _context.AlbumColecoes.RemoveRange(colecao.AlbumColecoes);

            _context.Colecoes.Remove(colecao);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Coleção \"{colecao.Nome}\" eliminada com sucesso.";
            return RedirectToPage("./Index");
        }
    }
}
