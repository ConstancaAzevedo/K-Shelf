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
    public class DetailsModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DetailsModel(
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public Utilizador User { get; set; } = default!;
        public List<string> UserRoles { get; set; } = new();
        public List<Colecao> UserColecoes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage("./Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage("./Index");
            }

            User = user;

            // Obter roles do utilizador
            var roles = await _userManager.GetRolesAsync(user);
            UserRoles = roles.ToList();

            // Obter coleções do utilizador
            UserColecoes = await _context.Colecoes
                .Where(c => c.UtilizadorId == user.Id)
                .Include(c => c.AlbumColecoes)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostMakeAdminAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage();
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Remover role User se existir
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            TempData["SuccessMessage"] = $"{user.Email} é agora Administrador!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRemoveAdminAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");

                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                TempData["SuccessMessage"] = $"Role Admin removido de {user.Email}. Utilizador voltou a ser User.";
            }
            else
            {
                TempData["ErrorMessage"] = $"{user.Email} não é Administrador.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostMakeUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID do utilizador não fornecido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToPage();
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
                TempData["SuccessMessage"] = $"Role User atribuído a {user.Email}.";
            }
            else
            {
                TempData["ErrorMessage"] = $"{user.Email} já tem o role User.";
            }

            return RedirectToPage(new { id });
        }
    }
}