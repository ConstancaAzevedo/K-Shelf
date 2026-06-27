using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Solistas
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Solista> Solistas { get; set; } = new List<Solista>();

        public async Task OnGetAsync()
        {
            Solistas = await _context.Solistas
                .OrderBy(s => s.Nome)
                .ToListAsync();
        }
    }
}