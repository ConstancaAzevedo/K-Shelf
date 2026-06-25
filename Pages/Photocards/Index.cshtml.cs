using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Photocards
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Photocard> Photocards { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Photocards = await _context.Photocards
                .Include(p => p.Artista)
                .Include(p => p.Album)
                .ToListAsync();
        }
    }
}
