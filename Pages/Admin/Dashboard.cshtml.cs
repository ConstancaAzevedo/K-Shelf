using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Security.Claims;

namespace K_Shelf.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashboardModel(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public int TotalUsers { get; set; }
        public int TotalArtistas { get; set; }
        public int TotalAlbuns { get; set; }
        public int TotalColecoes { get; set; }
        public int TotalPhotocards { get; set; }

        public List<RoleCount> UsersByRole { get; set; } = new();
        public List<ActivityItem> RecentActivities { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Estatísticas
            TotalUsers = await _context.Users.CountAsync();
            TotalArtistas = await _context.Artistas.CountAsync();
            TotalAlbuns = await _context.Albuns.CountAsync();
            TotalColecoes = await _context.Colecoes.CountAsync();
            TotalPhotocards = await _context.Photocards.CountAsync();

            // Utilizadores por Role
            var roles = await _roleManager.Roles.ToListAsync();
            UsersByRole = new List<RoleCount>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                UsersByRole.Add(new RoleCount
                {
                    RoleName = role.Name!,
                    Count = usersInRole.Count
                });
            }

            // Se não houver roles, adicionar contagem de utilizadores sem role
            var usersWithoutRole = await _context.Users
                .Where(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id))
                .CountAsync();

            if (usersWithoutRole > 0)
            {
                UsersByRole.Add(new RoleCount
                {
                    RoleName = "Sem Role",
                    Count = usersWithoutRole
                });
            }

            // Atividades Recentes 
            RecentActivities = new List<ActivityItem>
            {
                new ActivityItem
                {
                    Icon = "📀",
                    Description = $"Último álbum adicionado: {await GetLatestAlbumName()}",
                    TimeAgo = "Recentemente"
                },
                new ActivityItem
                {
                    Icon = "🎤",
                    Description = $"Último artista adicionado: {await GetLatestArtistName()}",
                    TimeAgo = "Recentemente"
                },
                new ActivityItem
                {
                    Icon = "🃏",
                    Description = $"Último photocard adicionado: {await GetLatestPhotocardName()}",
                    TimeAgo = "Recentemente"
                },
                new ActivityItem
                {
                    Icon = "📚",
                    Description = $"Última coleção criada: {await GetLatestColecaoName()}",
                    TimeAgo = "Recentemente"
                }                
             };
        }

        private async Task<string> GetLatestAlbumName()
        {
            var album = await _context.Albuns
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
            return album?.Titulo ?? "Nenhum";
        }

        private async Task<string> GetLatestArtistName()
        {
            var artista = await _context.Artistas
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
            return artista?.NomeExibicao ?? "Nenhum";
        }

        private async Task<string> GetLatestColecaoName()
        {
            var colecao = await _context.Colecoes
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
            return colecao?.Nome ?? "Nenhuma";
        }

        private async Task<string> GetLatestPhotocardName()
        {
            var photocard = await _context.Photocards
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();
            return photocard?.Versao ?? "Nenhum";
        }
    }

    public class RoleCount
    {
        public string RoleName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ActivityItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
    }
}