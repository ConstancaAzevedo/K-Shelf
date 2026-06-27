using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Grupos
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Grupo> Grupos { get; set; } = new List<Grupo>();

        public async Task OnGetAsync()
        {
            Grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .ToListAsync();
        }
    }
}