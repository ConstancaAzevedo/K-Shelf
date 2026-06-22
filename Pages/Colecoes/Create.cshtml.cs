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
            Colecao.DataCriacao = DateTime.Now;

            _context.Colecoes.Add(Colecao);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Coleção \"{Colecao.Nome}\" criada com sucesso!";
            return RedirectToPage("./Index");
        }
    }
}
