using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserWithRoles> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadUsers();
        }

        public async Task<IActionResult> OnPostMakeAdminAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "❌ ID do utilizador não fornecido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ Utilizador não encontrado.";
                return RedirectToPage();
            }

            // Verificar se o role Admin existe
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Remover role User se existir
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
            }

            // Adicionar role Admin
            await _userManager.AddToRoleAsync(user, "Admin");

            TempData["SuccessMessage"] = $"✅ {user.Email} é agora Administrador! (Role User removido)";
            await LoadUsers();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAdminAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "❌ ID do utilizador não fornecido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ Utilizador não encontrado.";
                return RedirectToPage();
            }

            // Remover role Admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");

                // Adicionar role User se não tiver nenhum role
                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                TempData["SuccessMessage"] = $"✅ Role Admin removido de {user.Email}. Utilizador voltou a ser User.";
            }
            else
            {
                TempData["ErrorMessage"] = $"❌ {user.Email} não é Administrador.";
            }

            await LoadUsers();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMakeUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "❌ ID do utilizador não fornecido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ Utilizador não encontrado.";
                return RedirectToPage();
            }

            // Verificar se o role User existe
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Adicionar role User se não tiver
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
                TempData["SuccessMessage"] = $"✅ Role User atribuído a {user.Email}.";
            }
            else
            {
                TempData["ErrorMessage"] = $"❌ {user.Email} já tem o role User.";
            }

            await LoadUsers();
            return RedirectToPage();
        }

        private async Task LoadUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            Users = new List<UserWithRoles>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserWithRoles
                {
                    Id = user.Id,
                    Email = user.Email ?? "N/A",
                    UserName = user.UserName ?? "N/A",
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                });
            }
        }
    }

    public class UserWithRoles
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}