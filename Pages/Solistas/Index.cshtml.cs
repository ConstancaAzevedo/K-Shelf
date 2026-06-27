using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Solistas
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

        // lista de solistas que vai ser exibida na pagina
        public IList<Solista> Solistas { get; set; } = new List<Solista>();

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // obtem todos os solistas da base de dados ordenados por nome
            Solistas = await _context.Solistas
                .OrderBy(s => s.Nome) // ordena alfabeticamente pelo nome
                .ToListAsync(); // converte para lista
        }
    }
}