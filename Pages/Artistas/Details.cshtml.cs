using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Artistas
{
    // restringe o acesso apenas a utilizadores autenticados
    [Authorize]
    public class DetailsModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // propriedade que armazena os dados do artista
        public Artista? Artista { get; set; }

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                return NotFound(); // retorna erro 404
            }

            // procura o artista pelo id com os dados relacionados
            Artista = await _context.Artistas
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .FirstOrDefaultAsync(m => m.Id == id);

            // se o artista nao existir, retorna erro
            if (Artista == null)
            {
                return NotFound(); // retorna erro 404
            }

            return Page(); // retorna a pagina
        }
    }
}