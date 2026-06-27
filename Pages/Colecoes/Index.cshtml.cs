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
    // requer autenticacao para aceder a esta pagina
    [Authorize]
    public class IndexModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // indica se o utilizador esta autenticado
        public bool IsUserAuthenticated { get; set; }
        // lista de colecoes do utilizador
        public IList<Colecao> Colecoes { get; set; } = new List<Colecao>();
        // lista de colecoes mock para utilizadores nao autenticados (demo)
        public IList<Colecao> MockColecoes { get; set; } = new List<Colecao>();

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // verifica se o utilizador esta autenticado
            IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (IsUserAuthenticated)
            {
                // obtem o id do utilizador autenticado
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // verifica se o utilizador e admin
                var isAdmin = User.IsInRole("Admin");

                if (userId != null)
                {
                    // admin ve todas as colecoes, user ve apenas as suas
                    if (isAdmin)
                    {
                        // admin: obtem todas as colecoes
                        Colecoes = await _context.Colecoes
                            .Include(c => c.AlbumColecoes!) // inclui a relacao com os albuns
                                .ThenInclude(ac => ac.Album) // inclui os dados do album
                            .OrderByDescending(c => c.DataCriacao) // ordena por data de criacao (mais recente primeiro)
                            .ToListAsync();
                    }
                    else
                    {
                        // user: obtem apenas as suas colecoes
                        Colecoes = await _context.Colecoes
                            .Where(c => c.UtilizadorId == userId) // filtra pelo id do utilizador
                            .Include(c => c.AlbumColecoes!) // inclui a relacao com os albuns
                                .ThenInclude(ac => ac.Album) // inclui os dados do album
                            .OrderByDescending(c => c.DataCriacao) // ordena por data de criacao (mais recente primeiro)
                            .ToListAsync();
                    }
                }
            }

            // gera dados mockados para o modo convidado/demonstracao
            // estas colecoes sao usadas para mostrar exemplos a utilizadores nao autenticados
            if (!Colecoes.Any())
            {
                // obtem os primeiros 3 albuns da base de dados
                var albuns = await _context.Albuns.Take(3).ToListAsync();

                // cria a primeira colecao mock
                var mock1 = new Colecao
                {
                    Id = -1, // id negativo para distinguir dos reais
                    Nome = "Favoritos de Todos os Tempos",
                    Descricao = "Uma coleção contendo os meus lançamentos K-Pop favoritos da vida inteira.",
                    DataCriacao = DateTime.Now.AddDays(-12), // data simulada
                    AlbumColecoes = new List<AlbumColecao>()
                };

                // cria a segunda colecao mock
                var mock2 = new Colecao
                {
                    Id = -2, // id negativo para distinguir dos reais
                    Nome = "Álbuns Físicos Compilados",
                    Descricao = "Aqui organizo apenas os meus álbuns físicos com photobooks, photocards e posters.",
                    DataCriacao = DateTime.Now.AddDays(-5), // data simulada
                    AlbumColecoes = new List<AlbumColecao>()
                };

                // adiciona os albuns a primeira colecao mock
                if (albuns.Any())
                {
                    foreach (var alb in albuns)
                    {
                        mock1.AlbumColecoes.Add(new AlbumColecao { Album = alb, AlbumId = alb.Id });
                    }

                    // adiciona os primeiros 2 albuns a segunda colecao mock
                    if (albuns.Count > 1)
                    {
                        mock2.AlbumColecoes.Add(new AlbumColecao { Album = albuns[0], AlbumId = albuns[0].Id });
                        mock2.AlbumColecoes.Add(new AlbumColecao { Album = albuns[1], AlbumId = albuns[1].Id });
                    }
                }

                // adiciona as colecoes mock a lista
                MockColecoes.Add(mock1);
                MockColecoes.Add(mock2);
            }
        }
    }
}