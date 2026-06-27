using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Admin.Users
{
    // restringe o acesso apenas a utilizadores com o role admin
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        // servicos do identity para gestao de utilizadores e roles
        private readonly UserManager<Utilizador> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        // contexto da base de dados para aceder as tabelas do sistema
        private readonly ApplicationDbContext _context;

        // construtor que recebe os servicos por injecao de dependencias
        public DetailsModel(
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            // inicializa os servicos injetados
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // propriedades para os dados do utilizador
        public Utilizador User { get; set; } = default!; // dados do utilizador
        public List<string> UserRoles { get; set; } = new(); // lista de roles do utilizador
        public List<Colecao> UserColecoes { get; set; } = new(); // lista de colecoes do utilizador

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(string id)
        {
            // verifica se o id foi fornecido
            if (string.IsNullOrEmpty(id))
            {
                // guarda mensagem de erro e redireciona para a lista
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage("./Index");
            }

            // procura o utilizador pelo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                // se o utilizador nao for encontrado, mostra erro
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage("./Index");
            }

            // atribui o utilizador a propriedade da pagina
            User = user;

            // obtem a lista de roles do utilizador
            var roles = await _userManager.GetRolesAsync(user);
            UserRoles = roles.ToList(); // converte para lista

            // obtem as colecoes do utilizador com os albuns associados
            UserColecoes = await _context.Colecoes
                .Where(c => c.UtilizadorId == user.Id) // filtra pelas colecoes do utilizador
                .Include(c => c.AlbumColecoes) // inclui a relacao com os albuns
                .OrderBy(c => c.Nome) // ordena alfabeticamente
                .ToListAsync();

            return Page(); // retorna a pagina
        }

        // handler para tornar um utilizador admin
        public async Task<IActionResult> OnPostMakeAdminAsync(string id)
        {
            // verifica se o id foi fornecido
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage(); // volta para a mesma pagina
            }

            // procura o utilizador pelo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage(); // volta para a mesma pagina
            }

            // cria o role admin se nao existir
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // remove o role user se existir (admin nao precisa de ser user)
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
            }

            // adiciona o role admin ao utilizador
            await _userManager.AddToRoleAsync(user, "Admin");

            // guarda mensagem de sucesso nos dados temporarios
            TempData["SuccessMessage"] = $"{user.Email} é agora Administrador!";
            return RedirectToPage(new { id }); // redireciona para os detalhes do utilizador
        }

        // handler para remover o role admin de um utilizador
        public async Task<IActionResult> OnPostRemoveAdminAsync(string id)
        {
            // verifica se o id foi fornecido
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage(); // volta para a mesma pagina
            }

            // procura o utilizador pelo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage(); // volta para a mesma pagina
            }

            // verifica se o utilizador e admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // remove o role admin
                await _userManager.RemoveFromRoleAsync(user, "Admin");

                // se o utilizador nao tiver o role user, adiciona
                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                // guarda mensagem de sucesso
                TempData["SuccessMessage"] = $"Role Admin removido de {user.Email}. Utilizador voltou a ser User.";
            }
            else
            {
                // se o utilizador nao for admin, mostra erro
                TempData["ErrorMessage"] = $"{user.Email} não é Administrador.";
            }

            return RedirectToPage(new { id }); // redireciona para os detalhes do utilizador
        }

        // handler para tornar um utilizador user
        public async Task<IActionResult> OnPostMakeUserAsync(string id)
        {
            // verifica se o id foi fornecido
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage(); // volta para a mesma pagina
            }

            // procura o utilizador pelo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage(); // volta para a mesma pagina
            }

            // cria o role user se nao existir
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // adiciona o role user ao utilizador se ele ainda nao o tiver
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
                // guarda mensagem de sucesso
                TempData["SuccessMessage"] = $"Role User atribuído a {user.Email}.";
            }
            else
            {
                // se o utilizador ja for user, mostra erro
                TempData["ErrorMessage"] = $"{user.Email} já tem o role User.";
            }

            return RedirectToPage(new { id }); // redireciona para os detalhes do utilizador
        }
    }
}