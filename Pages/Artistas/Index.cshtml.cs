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
    // restringe o acesso apenas a utilizadores autenticados
    [Authorize]
    public class ArtistasIndexModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public ArtistasIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // lista de artistas que vai ser exibida na pagina
        public IList<Artista> Artistas { get; set; } = new List<Artista>();

        // termo de pesquisa para filtrar artistas
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        // filtro por tipo (todos, grupos, solistas)
        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; } = "all";

        // estatisticas da pagina
        public int TotalArtists { get; set; } // total de artistas
        public int TotalGroups { get; set; } // total de artistas em grupos
        public int TotalSolos { get; set; } // total de solistas

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // obtem o total de artistas na base de dados
            TotalArtists = await _context.Artistas.CountAsync();
            // inicializa as outras estatisticas a 0
            TotalGroups = 0;
            TotalSolos = 0;

            // query base sem includes para evitar erros de navegacao
            var query = _context.Artistas.AsQueryable();

            // aplica o filtro de pesquisa se existir
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a => a.Nome.Contains(SearchTerm) || // pesquisa por nome
                                         (a.NomeArtistico != null && a.NomeArtistico.Contains(SearchTerm)) || // pesquisa por nome artistico
                                         (a.Pais != null && a.Pais.Contains(SearchTerm)) || // pesquisa por pais
                                         (a.Posicao != null && a.Posicao.Contains(SearchTerm))); // pesquisa por posicao
            }

            // aplica o filtro por tipo se existir
            if (!string.IsNullOrEmpty(TypeFilter) && TypeFilter != "all")
            {
                if (TypeFilter == "groups")
                {
                    // filtra apenas artistas que pertencem a um grupo
                    query = query.Where(a => a.GrupoId.HasValue);
                }
                else if (TypeFilter == "solos")
                {
                    // filtra apenas artistas que sao solistas
                    query = query.Where(a => a.SolistaId.HasValue);
                }
            }

            // executa a query, ordena por nome artistico (ou nome) e guarda os resultados
            Artistas = await query.OrderBy(a => a.NomeArtistico ?? a.Nome).ToListAsync();
        }
    }
}