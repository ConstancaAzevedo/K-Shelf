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
    public class IndexModel : PageModel
    {
        // servicos do identity para gestao de utilizadores e roles
        private readonly UserManager<Utilizador> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // construtor que recebe os servicos por injecao de dependencias
        public IndexModel(
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // lista de utilizadores com os seus respetivos roles
        public List<UserWithRoles> Users { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            await LoadUsers(); // carrega a lista de utilizadores
        }

        // handler para tornar um utilizador admin
        public async Task<IActionResult> OnPostMakeAdminAsync(string id)
        {
            // verifica se o id foi fornecido
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "❌ ID do utilizador não fornecido.";
                return RedirectToPage(); // volta para a pagina atual
            }

            // procura o utilizador pelo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ Utilizador não encontrado.";
                return RedirectToPage(); // volta para a pagina atual
            }

            // verifica se o role admin existe, caso contrario cria
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
            TempData["SuccessMessage"] = $"✅ {user.Email} é agora Administrador! (Role User removido)";
            await LoadUsers(); // recarrega a lista de utilizadores
            return RedirectToPage(); // redireciona para a pagina atual
        }

        // handler para remover o role admin de um utilizador
        public async Task<IActionResult> OnPostRemoveAdminAsync(string id)
        {
            // verifica se o id foi fornecido
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "❌ ID do utilizador não fornecido.";
                return RedirectToPage(); // volta para a pagina atual
            }

            // procura o utilizador pelo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ Utilizador não encontrado.";
                return RedirectToPage(); // volta para a pagina atual
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
                TempData["SuccessMessage"] = $"✅ Role Admin removido de {user.Email}. Utilizador voltou a ser User.";
            }
            else
            {
                // se o utilizador nao for admin, mostra erro
                TempData["ErrorMessage"] = $"❌ {user.Email} não é Administrador.";
            }

            await LoadUsers(); // recarrega a lista de utilizadores
            return RedirectToPage(); // redireciona para a pagina atual
        }

        // handler para tornar um utilizador user
        public async Task<IActionResult> OnPostMakeUserAsync(string id)
        {
            // verifica se o id foi fornecido
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "❌ ID do utilizador não fornecido.";
                return RedirectToPage(); // volta para a pagina atual
            }

            // procura o utilizador pelo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ Utilizador não encontrado.";
                return RedirectToPage(); // volta para a pagina atual
            }

            // verifica se o role user existe, caso contrario cria
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // adiciona o role user ao utilizador se ele ainda nao o tiver
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
                // guarda mensagem de sucesso
                TempData["SuccessMessage"] = $"✅ Role User atribuído a {user.Email}.";
            }
            else
            {
                // se o utilizador ja for user, mostra erro
                TempData["ErrorMessage"] = $"❌ {user.Email} já tem o role User.";
            }

            await LoadUsers(); // recarrega a lista de utilizadores
            return RedirectToPage(); // redireciona para a pagina atual
        }

        // metodo privado que carrega a lista de utilizadores com os seus roles
        private async Task LoadUsers()
        {
            // obtem todos os utilizadores da base de dados
            var users = await _userManager.Users.ToListAsync();
            Users = new List<UserWithRoles>(); // inicializa a lista

            // percorre cada utilizador
            foreach (var user in users)
            {
                // obtem os roles do utilizador
                var roles = await _userManager.GetRolesAsync(user);
                // adiciona o utilizador a lista com os seus dados
                Users.Add(new UserWithRoles
                {
                    Id = user.Id, // identificador unico
                    Email = user.Email ?? "N/A", // email ou "N/A" se for nulo
                    UserName = user.UserName ?? "N/A", // nome de utilizador ou "N/A"
                    EmailConfirmed = user.EmailConfirmed, // indicador de email confirmado
                    Roles = roles.ToList() // lista de roles
                });
            }
        }
    }

    // classe auxiliar que representa um utilizador com os seus roles
    public class UserWithRoles
    {
        public string Id { get; set; } = string.Empty; // identificador do utilizador
        public string Email { get; set; } = string.Empty; // email do utilizador
        public string UserName { get; set; } = string.Empty; // nome de utilizador
        public bool EmailConfirmed { get; set; } // indica se o email foi confirmado
        public List<string> Roles { get; set; } = new(); // lista de roles do utilizador
    }
}