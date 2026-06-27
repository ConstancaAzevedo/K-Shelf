using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Grupos
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

        // lista de grupos que vai ser exibida na pagina
        public IList<Grupo> Grupos { get; set; } = new List<Grupo>();

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // obtem todos os grupos da base de dados ordenados por nome
            Grupos = await _context.Grupos
                .OrderBy(g => g.Nome) // ordena alfabeticamente pelo nome
                .ToListAsync(); // converte para lista
        }
    }
}