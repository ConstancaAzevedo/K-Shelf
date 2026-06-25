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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Colecao Colecao { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var colecao = await _context.Colecoes.FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound();

            // Só o dono ou Admin pode editar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            Colecao = colecao;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var colecaoExistente = await _context.Colecoes.FindAsync(Colecao.Id);
            if (colecaoExistente == null)
                return NotFound();

            // Verificar que só o dono ou Admin pode editar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecaoExistente.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Validar se já existe outra coleção com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == colecaoExistente.UtilizadorId && 
                               c.Id != Colecao.Id && 
                               c.Nome.ToLower() == Colecao.Nome.ToLower());

            if (colecaoDuplicada)
            {
                ModelState.AddModelError("Colecao.Nome", $"Já existe uma coleção com o nome \"{Colecao.Nome}\"!");
                return Page();
            }

            // Atualizar apenas os campos editáveis
            colecaoExistente.Nome = Colecao.Nome;
            colecaoExistente.Descricao = Colecao.Descricao;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Colecoes.AnyAsync(c => c.Id == Colecao.Id))
                    return NotFound();
                throw;
            }

            TempData["SuccessMessage"] = $"Coleção \"{Colecao.Nome}\" atualizada com sucesso!";
            return RedirectToPage("./Details", new { id = Colecao.Id });
        }
    }
}
