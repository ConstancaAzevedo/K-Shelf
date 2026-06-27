using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace K_Shelf.Pages.Grupos
{
    // restringe o acesso apenas a utilizadores com o role admin
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // propriedade que recebe os dados do grupo por binding
        [BindProperty]
        public Grupo Grupo { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public IActionResult OnGet()
        {
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync()
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return Page(); // se houver erros, volta para a pagina

            // adiciona o grupo ao contexto
            _context.Grupos.Add(Grupo);
            await _context.SaveChangesAsync(); // guarda na base de dados

            // guarda mensagem de sucesso nos dados temporarios
            TempData["SuccessMessage"] = $"Grupo \"{Grupo.Nome}\" criado com sucesso!";
            return RedirectToPage("./Index"); // redireciona para a lista de grupos
        }
    }
}