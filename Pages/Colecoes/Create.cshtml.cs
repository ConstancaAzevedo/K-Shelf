using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Security.Claims;

namespace K_Shelf.Pages.Colecoes
{
    [Authorize] // Qualquer utilizador autenticado pode criar coleções
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Colecao Colecao { get; set; } = new Colecao();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Associar a coleção ao utilizador autenticado
            Colecao.UtilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Validar se já existe uma coleção com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == Colecao.UtilizadorId && 
                               c.Nome.ToLower() == Colecao.Nome.ToLower());

            if (colecaoDuplicada)
            {
                ModelState.AddModelError("Colecao.Nome", $"Já existe uma coleção com o nome \"{Colecao.Nome}\"!");
                return Page();
            }

            Colecao.DataCriacao = DateTime.Now;

            _context.Colecoes.Add(Colecao);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Coleção \"{Colecao.Nome}\" criada com sucesso!";
            return RedirectToPage("./Index");
        }
    }
}
