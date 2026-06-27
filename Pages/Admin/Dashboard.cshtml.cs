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
    // restringe o acesso apenas a utilizadores com o role admin
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas do sistema
        private readonly ApplicationDbContext _context;
        // gestor de utilizadores do identity para operacoes relacionadas com usuarios
        private readonly UserManager<Utilizador> _userManager;
        // gestor de roles do identity para operacoes relacionadas com cargos
        private readonly RoleManager<IdentityRole> _roleManager;

        // construtor que recebe os servicos necessarios por injecao de dependencias
        public DashboardModel(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // propriedades que armazenam as estatisticas do sistema para mostrar no dashboard
        public int TotalUsers { get; set; } // numero total de utilizadores registados
        public int TotalArtistas { get; set; } // numero total de artistas
        public int TotalAlbuns { get; set; } // numero total de albuns
        public int TotalColecoes { get; set; } // numero total de colecoes
        public int TotalPhotocards { get; set; } // numero total de photocards

        // lista com a contagem de utilizadores agrupados por cada role existente
        public List<RoleCount> UsersByRole { get; set; } = new();
        // lista com as atividades mais recentes realizadas no sistema
        public List<ActivityItem> RecentActivities { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // obtem as estatisticas contando os registos em cada tabela
            TotalUsers = await _context.Users.CountAsync();
            TotalArtistas = await _context.Artistas.CountAsync();
            TotalAlbuns = await _context.Albuns.CountAsync();
            TotalColecoes = await _context.Colecoes.CountAsync();
            TotalPhotocards = await _context.Photocards.CountAsync();

            // obtem a lista de todos os roles existentes no sistema
            var roles = await _roleManager.Roles.ToListAsync();
            UsersByRole = new List<RoleCount>();

            // percorre cada role para contar quantos utilizadores o possuem
            foreach (var role in roles)
            {
                // obtem a lista de utilizadores que pertencem a este role
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                // adiciona a contagem a lista de resultados
                UsersByRole.Add(new RoleCount
                {
                    RoleName = role.Name!,
                    Count = usersInRole.Count
                });
            }

            // conta os utilizadores que nao possuem nenhum role atribuido
            var usersWithoutRole = await _context.Users
                .Where(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id))
                .CountAsync();

            // se existirem utilizadores sem role, adiciona essa informacao a lista
            if (usersWithoutRole > 0)
            {
                UsersByRole.Add(new RoleCount
                {
                    RoleName = "Sem Role",
                    Count = usersWithoutRole
                });
            }

            // constroi a lista de atividades recentes com os ultimos itens adicionados
            RecentActivities = new List<ActivityItem>
            {
                // ultimo album adicionado ao sistema
                new ActivityItem
                {
                    Icon = "📀",
                    Description = $"Último álbum adicionado: {await GetLatestAlbumName()}",
                    TimeAgo = "Recentemente"
                },
                // ultimo artista adicionado ao sistema
                new ActivityItem
                {
                    Icon = "🎤",
                    Description = $"Último artista adicionado: {await GetLatestArtistName()}",
                    TimeAgo = "Recentemente"
                },
                // ultimo photocard adicionado ao sistema
                new ActivityItem
                {
                    Icon = "🃏",
                    Description = $"Último photocard adicionado: {await GetLatestPhotocardName()}",
                    TimeAgo = "Recentemente"
                },
                // ultima colecao criada por um utilizador
                new ActivityItem
                {
                    Icon = "📚",
                    Description = $"Última coleção criada: {await GetLatestColecaoName()}",
                    TimeAgo = "Recentemente"
                }
             };
        }

        // obtem o titulo do ultimo album adicionado a base de dados
        private async Task<string> GetLatestAlbumName()
        {
            // ordena os albuns por id decrescente e obtem o primeiro
            var album = await _context.Albuns
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
            // retorna o titulo ou "nenhum" se nao houver registos
            return album?.Titulo ?? "Nenhum";
        }

        // obtem o nome do ultimo artista adicionado a base de dados
        private async Task<string> GetLatestArtistName()
        {
            // ordena os artistas por id decrescente e obtem o primeiro
            var artista = await _context.Artistas
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
            // retorna o nome de exibicao ou "nenhum" se nao houver registos
            return artista?.NomeExibicao ?? "Nenhum";
        }

        // obtem o nome da ultima colecao adicionada a base de dados
        private async Task<string> GetLatestColecaoName()
        {
            // ordena as colecoes por id decrescente e obtem a primeira
            var colecao = await _context.Colecoes
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
            // retorna o nome da colecao ou "nenhuma" se nao houver registos
            return colecao?.Nome ?? "Nenhuma";
        }

        // obtem a versao do ultimo photocard adicionado a base de dados
        private async Task<string> GetLatestPhotocardName()
        {
            // ordena os photocards por id decrescente e obtem o primeiro
            var photocard = await _context.Photocards
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();
            // retorna a versao do photocard ou "nenhum" se nao houver registos
            return photocard?.Versao ?? "Nenhum";
        }
    }

    // classe auxiliar que guarda a contagem de utilizadores para um role especifico
    public class RoleCount
    {
        public string RoleName { get; set; } = string.Empty; // nome do role
        public int Count { get; set; } // numero de utilizadores com esse role
    }

    // classe auxiliar que guarda a informacao de uma atividade para mostrar no dashboard
    public class ActivityItem
    {
        public string Icon { get; set; } = string.Empty; // icone representativo da atividade
        public string Description { get; set; } = string.Empty; // descricao textual da atividade
        public string TimeAgo { get; set; } = string.Empty; // tempo decorrido desde a atividade
    }
}