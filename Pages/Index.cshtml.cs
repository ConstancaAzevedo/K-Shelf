using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int ArtistasCount { get; set; }
        public int AlbunsCount { get; set; }
        public int ColecoesCount { get; set; }

        public async Task OnGetAsync()
        {
            ArtistasCount = await _context.Artistas.CountAsync();
            AlbunsCount = await _context.Albuns.CountAsync();
            ColecoesCount = await _context.Colecoes.CountAsync();
        }
    }
}