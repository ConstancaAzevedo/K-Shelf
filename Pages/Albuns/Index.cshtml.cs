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
    // pagina de listagem de albuns (acesso publico)
    public class IndexModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // lista de albuns que vai ser exibida na pagina
        public IList<Album> Albuns { get; set; } = default!;

        // termo de pesquisa para filtrar albuns
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        // filtro por tipo de album
        [BindProperty(SupportsGet = true)]
        public Album.TipoAlbum? TypeFilter { get; set; }

        // filtro por edicao de album
        [BindProperty(SupportsGet = true)]
        public Album.EdicaoAlbum? EditionFilter { get; set; }

        // estatisticas da pagina
        public int TotalAlbuns { get; set; } // total de albuns
        public int StudioAlbunsCount { get; set; } // albuns de estúdio
        public int EpAlbunsCount { get; set; } // eps
        public int SingleAlbunsCount { get; set; } // singles

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // obtem as estatisticas
            TotalAlbuns = await _context.Albuns.CountAsync();
            StudioAlbunsCount = await _context.Albuns.CountAsync(a => a.Tipo == Album.TipoAlbum.Studio);
            EpAlbunsCount = await _context.Albuns.CountAsync(a => a.Tipo == Album.TipoAlbum.EP);
            SingleAlbunsCount = await _context.Albuns.CountAsync(a => a.Tipo == Album.TipoAlbum.Single);

            // query base com os dados relacionados
            var query = _context.Albuns
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .Include(a => a.Musicas) // inclui as musicas
                .AsQueryable();

            // aplica o filtro de pesquisa se existir
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a => a.Titulo.Contains(SearchTerm) || // pesquisa por titulo
                                         (a.Grupo != null && a.Grupo.Nome.Contains(SearchTerm)) || // pesquisa por grupo
                                         (a.Solista != null && a.Solista.Nome.Contains(SearchTerm))); // pesquisa por solista
            }

            // aplica o filtro por tipo se existir
            if (TypeFilter.HasValue)
            {
                query = query.Where(a => a.Tipo == TypeFilter.Value);
            }

            // aplica o filtro por edicao se existir
            if (EditionFilter.HasValue)
            {
                query = query.Where(a => a.Edicao == EditionFilter.Value);
            }

            // executa a query, ordena por data de lancamento (mais recente primeiro) e guarda os resultados
            Albuns = await query.OrderByDescending(a => a.DataLancamento).ToListAsync();
        }
    }
}