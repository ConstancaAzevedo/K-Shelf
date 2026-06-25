using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace K_Shelf.Pages.Artistas
{
    [Authorize]
    public class ArtistasIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ArtistasIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Artista> Artistas { get; set; } = new List<Artista>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; } = "all";

        public int TotalArtists { get; set; }
        public int TotalGroups { get; set; }
        public int TotalSolos { get; set; }

        public async Task OnGetAsync()
        {
            TotalArtists = await _context.Artistas.CountAsync();
            TotalGroups = 0;
            TotalSolos = 0;

            // Remover os Includes que causam erro
            var query = _context.Artistas.AsQueryable();

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