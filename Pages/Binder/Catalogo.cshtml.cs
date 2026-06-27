using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Binder
{
    /// <summary>
    /// pagina publica do catalogo de photocards. permite pesquisar e filtrar photocards
    /// por grupo ou texto livre, e adiciona-los ao binder pessoal (utilizadores autenticados)
    /// </summary>
    public class CatalogoModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // gestor de utilizadores do identity
        private readonly UserManager<Utilizador> _userManager;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        /// <summary>
        /// construtor com injecao de dependencia do contexto da bd e do gestor de utilizadores
        /// </summary>
        public CatalogoModel(ApplicationDbContext context, UserManager<Utilizador> userManager, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        /// <summary>lista de photocards filtrada para exibicao no catalogo</summary>
        public IList<Photocard> Photocards { get; set; } = default!;

        /// <summary>lista de grupos para o filtro dropdown do catalogo</summary>
        public SelectList GruposSelectList { get; set; } = default!;

        /// <summary>texto de pesquisa introduzido pelo utilizador (versao, artista, grupo ou album)</summary>
        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        /// <summary>id do grupo selecionado no filtro dropdown; null se nenhum filtro ativo</summary>
        [BindProperty(SupportsGet = true)]
        public int? GrupoFilter { get; set; }

        /// <summary>id do photocard que o utilizador pretende adicionar ao seu binder</summary>
        [BindProperty]
        public int AddPhotocardId { get; set; }

        /// <summary>estado de posse escolhido pelo utilizador (possui/deseja/paratroca)</summary>
        [BindProperty]
        public EstadoPhotocard AddEstado { get; set; }

        /// <summary>quantidade de copias a adicionar ao binder (minimo 1)</summary>
        [BindProperty]
        public int AddQuantidade { get; set; } = 1;

        /// <summary>notas pessoais opcionais do utilizador para o photocard</summary>
        [BindProperty]
        public string? AddNotas { get; set; }

        /// <summary>
        /// carrega e filtra os photocards do catalogo conforme a pesquisa e o grupo selecionado
        /// </summary>
        public async Task OnGetAsync()
        {
            // carrega a lista de grupos para o dropdown
            await CarregarGruposSelectList();

            // query base com os dados relacionados
            var query = _context.Photocards
                .Include(p => p.Artista!) // inclui o artista
                    .ThenInclude(a => a.Grupo!) // inclui o grupo do artista
                .Include(p => p.Artista!)
                    .ThenInclude(a => a.Solista!) // inclui o solista do artista
                .Include(p => p.Album!) // inclui o album
                .AsQueryable();

            // aplica o filtro de pesquisa se existir
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(p => p.Versao.Contains(SearchQuery!) || // pesquisa por versao
                                         (p.Artista != null && p.Artista.NomeArtistico!.Contains(SearchQuery!)) || // pesquisa por nome artistico
                                         (p.Artista != null && p.Artista.Grupo != null && p.Artista.Grupo.Nome.Contains(SearchQuery!)) || // pesquisa por grupo
                                         (p.Album != null && p.Album.Titulo.Contains(SearchQuery!))); // pesquisa por album
            }

            // aplica o filtro por grupo se existir
            if (GrupoFilter.HasValue)
            {
                query = query.Where(p => p.Artista != null && p.Artista.GrupoId == GrupoFilter.Value);
            }

            // executa a query e guarda os resultados
            Photocards = await query.ToListAsync();
        }

        /// <summary>
        /// processa o pedido de adicao de um photocard ao binder pessoal do utilizador autenticado
        /// se ja existir com o mesmo estado, incrementa a quantidade; caso contrario, cria nova entrada
        /// </summary>
        public async Task<IActionResult> OnPostAdicionarAsync()
        {
            // verifica se o utilizador esta autenticado
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Challenge(); // redireciona para o login
            }

            // obtem o utilizador autenticado
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // redireciona para o login
            }

            // valida se o photocard existe
            var photocard = await _context.Photocards.FindAsync(AddPhotocardId);
            if (photocard == null)
            {
                TempData["ErrorMessage"] = "Photocard não encontrado.";
                return RedirectToPage(); // volta para a pagina com erro
            }

            // verifica se ja existe no binder com o mesmo estado
            var binderEntry = await _context.UtilizadorPhotocards
                .FirstOrDefaultAsync(up => up.UtilizadorId == user.Id && up.PhotocardId == AddPhotocardId && up.Estado == AddEstado);

            if (binderEntry != null)
            {
                // se ja existir, incrementa a quantidade
                binderEntry.Quantidade += AddQuantidade;
                // adiciona as notas se existirem
                if (!string.IsNullOrWhiteSpace(AddNotas))
                {
                    binderEntry.Notas = string.IsNullOrEmpty(binderEntry.Notas)
                        ? AddNotas
                        : $"{binderEntry.Notas} | {AddNotas}"; // concatena as notas
                }
            }
            else
            {
                // cria uma nova entrada no binder
                var newEntry = new UtilizadorPhotocard
                {
                    UtilizadorId = user.Id,
                    PhotocardId = AddPhotocardId,
                    Estado = AddEstado,
                    Quantidade = AddQuantidade,
                    Notas = AddNotas
                };
                _context.UtilizadorPhotocards.Add(newEntry);
            }

            // guarda as alteracoes na base de dados
            await _context.SaveChangesAsync();

            // notificacao signalr para todos os clientes
            await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Binder",
                Acao = "Adicionado",
                Mensagem = $"Photocard '{photocard.Versao}' foi adicionado ao Binder de {user.Email}!",
                Data = DateTime.Now
            });

            // mensagem de sucesso
            TempData["SuccessMessage"] = $"Photocard adicionado ao teu Binder com sucesso!";
            return RedirectToPage(); // redireciona para a pagina atual
        }

        /// <summary>
        /// metodo auxiliar que carrega todos os grupos para o dropdown de filtro do catalogo
        /// </summary>
        private async Task CarregarGruposSelectList()
        {
            // obtem todos os grupos ordenados por nome
            var grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .ToListAsync();

            // cria o selectlist para os grupos (valor = id, texto = nome)
            GruposSelectList = new SelectList(grupos, "Id", "Nome");
        }
    }
}