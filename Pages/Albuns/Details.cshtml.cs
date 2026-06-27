using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Albuns
{
    // pagina de detalhes do album (acesso publico)
    public class DetailsModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // propriedade que armazena os dados do album
        public Album Album { get; set; } = default!;

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
                return NotFound(); // retorna erro 404

            // procura o album pelo id com todos os dados relacionados
            var album = await _context.Albuns
                .Include(a => a.Grupo) // inclui o grupo associado
                    .ThenInclude(g => g!.Artistas) // inclui os artistas do grupo
                .Include(a => a.Solista) // inclui o solista associado
                .Include(a => a.Musicas!.OrderBy(m => m.TrackNumber)) // inclui as musicas ordenadas por numero de faixa
                .Include(a => a.AlbumColecoes!) // inclui as relacoes com colecoes
                    .ThenInclude(ac => ac.Colecao) // inclui os dados da colecao
                .FirstOrDefaultAsync(a => a.Id == id);

            // se o album nao existir, retorna erro
            if (album == null)
                return NotFound(); // retorna erro 404

            // atribui o album a propriedade da pagina
            Album = album;
            return Page(); // retorna a pagina
        }
    }
}