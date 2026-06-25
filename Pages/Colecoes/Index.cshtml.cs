using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Colecoes
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool IsUserAuthenticated { get; set; }
        public IList<Colecao> Colecoes { get; set; } = new List<Colecao>();
        public IList<Colecao> MockColecoes { get; set; } = new List<Colecao>();

        public async Task OnGetAsync()
        {
            IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (IsUserAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                if (userId != null)
                {
                    // Admin vê todas as coleções, User vê apenas as suas
                    if (isAdmin)
                    {
                        Colecoes = await _context.Colecoes
                            .Include(c => c.AlbumColecoes!)
                                .ThenInclude(ac => ac.Album)
                            .OrderByDescending(c => c.DataCriacao)
                            .ToListAsync();
                    }
                    else
                    {
                        Colecoes = await _context.Colecoes
                            .Where(c => c.UtilizadorId == userId)
                            .Include(c => c.AlbumColecoes!)
                                .ThenInclude(ac => ac.Album)
                            .OrderByDescending(c => c.DataCriacao)
                            .ToListAsync();
                    }
                }
            }

            // Gerar dados mockados para o modo convidado/demonstração
            if (!Colecoes.Any())
            {
                var albuns = await _context.Albuns.Take(3).ToListAsync();

                var mock1 = new Colecao
                {
                    Id = -1,
                    Nome = "Favoritos de Todos os Tempos 📀",
                    Descricao = "Uma coleção contendo os meus lançamentos K-Pop favoritos da vida inteira.",
                    DataCriacao = DateTime.Now.AddDays(-12),
                    AlbumColecoes = new List<AlbumColecao>()
                };

                var mock2 = new Colecao
                {
                    Id = -2,
                    Nome = "Álbuns Físicos Compilados 💿",
                    Descricao = "Aqui organizo apenas os meus álbuns físicos com photobooks, photocards e posters.",
                    DataCriacao = DateTime.Now.AddDays(-5),
                    AlbumColecoes = new List<AlbumColecao>()
                };

                if (albuns.Any())
                {
                    foreach (var alb in albuns)
                    {
                        mock1.AlbumColecoes.Add(new AlbumColecao { Album = alb, AlbumId = alb.Id });
                    }

                    if (albuns.Count > 1)
                    {
                        mock2.AlbumColecoes.Add(new AlbumColecao { Album = albuns[0], AlbumId = albuns[0].Id });
                        mock2.AlbumColecoes.Add(new AlbumColecao { Album = albuns[1], AlbumId = albuns[1].Id });
                    }
                }

                MockColecoes.Add(mock1);
                MockColecoes.Add(mock2);
            }
        }
    }
}
