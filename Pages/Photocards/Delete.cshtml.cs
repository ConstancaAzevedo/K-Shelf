using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Photocards
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
        public Photocard Photocard { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index");
            }

            var photocard = await _context.Photocards
                .Include(p => p.Artista)
                .Include(p => p.Album)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photocard == null)
            {
                TempData["ErrorMessage"] = "Photocard não encontrado.";
                return RedirectToPage("./Index");
            }

            Photocard = photocard;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index");
            }

            var photocard = await _context.Photocards.FindAsync(id);

            if (photocard != null)
            {
                try
                {
                    _context.Photocards.Remove(photocard);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Photocard \"{photocard.Versao}\" eliminado com sucesso!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Erro ao eliminar photocard: {ex.Message}";
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
