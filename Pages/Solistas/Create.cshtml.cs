using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Pages.Solistas
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
        public Solista Solista { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Solistas.Add(Solista);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Solista \"{Solista.Nome}\" criado com sucesso!";
            return RedirectToPage("/Albuns/Create");
        }
    }
}