using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Models;

namespace K_Shelf.Pages.Admin.Cargos
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Utilizador> _userManager;

        public IndexModel(
            RoleManager<IdentityRole> roleManager,
            UserManager<Utilizador> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [BindProperty]
        public string NewRoleName { get; set; } = string.Empty;

        public List<RoleWithUserCount> Roles { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadRoles();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Validação
            if (string.IsNullOrWhiteSpace(NewRoleName))
            {
                TempData["ErrorMessage"] = "O nome do cargo é obrigatório.";
                await LoadRoles();
                return Page();
            }

            if (NewRoleName.Length < 3 || NewRoleName.Length > 50)
            {
                TempData["ErrorMessage"] = "O nome do cargo deve ter entre 3 e 50 caracteres.";
                await LoadRoles();
                return Page();
            }

            if (await _roleManager.RoleExistsAsync(NewRoleName))
            {
                TempData["ErrorMessage"] = $"Já existe um cargo com o nome \"{NewRoleName}\".";
                await LoadRoles();
                return Page();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(NewRoleName));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Cargo \"{NewRoleName}\" criado com sucesso!";
                NewRoleName = string.Empty;
                await LoadRoles();
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = $"Erro ao criar cargo: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            await LoadRoles();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do cargo não fornecido.";
                return RedirectToPage();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Cargo não encontrado.";
                return RedirectToPage();
            }

            // Não permitir eliminar cargos padrão
            if (role.Name == "Admin" || role.Name == "User")
            {
                TempData["ErrorMessage"] = $"O cargo \"{role.Name}\" é padrão do sistema e não pode ser eliminado.";
                return RedirectToPage();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Cargo \"{role.Name}\" eliminado com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Erro ao eliminar cargo: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToPage();
        }

        private async Task LoadRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            Roles = new List<RoleWithUserCount>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                Roles.Add(new RoleWithUserCount
                {
                    Id = role.Id,
                    Name = role.Name!,
                    UserCount = usersInRole.Count
                });
            }
        }
    }

    public class RoleWithUserCount
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}