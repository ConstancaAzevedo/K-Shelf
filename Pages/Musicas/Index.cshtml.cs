using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Musicas
{
    // restringe o acesso apenas a utilizadores com o role admin
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // lista de musicas que vai ser exibida na pagina
        public IList<Musica> Musicas { get; set; } = new List<Musica>();

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // obtem todas as musicas da base de dados com os dados do album
            Musicas = await _context.Musicas
                .Include(m => m.Album) // inclui o album associado
                .OrderBy(m => m.AlbumId) // ordena por album id
                .ThenBy(m => m.TrackNumber) // depois ordena por numero de faixa
                .ToListAsync(); // converte para lista
        }
    }
}