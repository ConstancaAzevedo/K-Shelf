using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Artistas
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Artista> Artistas { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; } = "all"; // "all", "groups", "solos"

        public int TotalArtists { get; set; }
        public int TotalGroups { get; set; }
        public int TotalSolos { get; set; }

        public async Task OnGetAsync()
        {
            // Estatísticas
            TotalArtists = await _context.Artistas.CountAsync();
            TotalGroups = await _context.Grupos.CountAsync();
            TotalSolos = await _context.Solistas.CountAsync();

            var query = _context.Artistas
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a => a.Nome.Contains(SearchTerm) || 
                                         (a.NomeArtistico != null && a.NomeArtistico.Contains(SearchTerm)) ||
                                         (a.Nacionalidade != null && a.Nacionalidade.Contains(SearchTerm)) ||
                                         (a.Posicao != null && a.Posicao.Contains(SearchTerm)));
            }

            if (!string.IsNullOrEmpty(TypeFilter) && TypeFilter != "all")
            {
                if (TypeFilter == "groups")
                {
                    query = query.Where(a => a.GrupoId.HasValue);
                }
                else if (TypeFilter == "solos")
                {
                    query = query.Where(a => a.SolistaId.HasValue);
                }
            }

            Artistas = await query.OrderBy(a => a.NomeArtistico ?? a.Nome).ToListAsync();
        }
    }
}
