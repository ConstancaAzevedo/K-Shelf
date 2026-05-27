using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Albuns
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Album> Albuns { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public Album.TipoAlbum? TypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public Album.EdicaoAlbum? EditionFilter { get; set; }

        public int TotalAlbuns { get; set; }
        public int StudioAlbunsCount { get; set; }
        public int EpAlbunsCount { get; set; }
        public int SingleAlbunsCount { get; set; }

        public async Task OnGetAsync()
        {
            // Estatísticas
            TotalAlbuns = await _context.Albuns.CountAsync();
            StudioAlbunsCount = await _context.Albuns.CountAsync(a => a.Tipo == Album.TipoAlbum.Studio);
            EpAlbunsCount = await _context.Albuns.CountAsync(a => a.Tipo == Album.TipoAlbum.EP);
            SingleAlbunsCount = await _context.Albuns.CountAsync(a => a.Tipo == Album.TipoAlbum.Single);

            var query = _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Include(a => a.Musicas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a => a.Titulo.Contains(SearchTerm) ||
                                         (a.Grupo != null && a.Grupo.Nome.Contains(SearchTerm)) ||
                                         (a.Solista != null && a.Solista.Nome.Contains(SearchTerm)));
            }

            if (TypeFilter.HasValue)
            {
                query = query.Where(a => a.Tipo == TypeFilter.Value);
            }

            if (EditionFilter.HasValue)
            {
                query = query.Where(a => a.Edicao == EditionFilter.Value);
            }

            Albuns = await query.OrderByDescending(a => a.DataLancamento).ToListAsync();
        }
    }
}
