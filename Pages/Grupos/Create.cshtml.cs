using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Pages.Grupos
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
        public Grupo Grupo { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Grupos.Add(Grupo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Grupo \"{Grupo.Nome}\" criado com sucesso!";
            return RedirectToPage("./Index");
        }
    }
}