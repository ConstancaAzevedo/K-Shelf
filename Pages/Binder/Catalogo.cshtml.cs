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
    /// Página pública do Catálogo de Photocards. Permite pesquisar e filtrar photocards
    /// por grupo ou texto livre, e adicioná-los ao Binder pessoal (utilizadores autenticados).
    /// </summary>
    public class CatalogoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IHubContext<NotificacaoHub> _hubContext; 

        /// <summary>
        /// Construtor com injeção de dependência do contexto da BD e do gestor de utilizadores.
        /// </summary>
        public CatalogoModel(ApplicationDbContext context, UserManager<Utilizador> userManager, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        /// <summary>Lista de photocards filtrada para exibição no catálogo.</summary>
        public IList<Photocard> Photocards { get; set; } = default!;

        /// <summary>Lista de grupos para o filtro dropdown do catálogo.</summary>
        public SelectList GruposSelectList { get; set; } = default!;

        /// <summary>Texto de pesquisa introduzido pelo utilizador (versão, artista, grupo ou álbum).</summary>
        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        /// <summary>ID do grupo selecionado no filtro dropdown; null se nenhum filtro ativo.</summary>
        [BindProperty(SupportsGet = true)]
        public int? GrupoFilter { get; set; }

        /// <summary>ID do photocard que o utilizador pretende adicionar ao seu Binder.</summary>
        [BindProperty]
        public int AddPhotocardId { get; set; }

        /// <summary>Estado de posse escolhido pelo utilizador (Possui/Deseja/ParaTroca).</summary>
        [BindProperty]
        public EstadoPhotocard AddEstado { get; set; }

        /// <summary>Quantidade de cópias a adicionar ao Binder (mínimo 1).</summary>
        [BindProperty]
        public int AddQuantidade { get; set; } = 1;

        /// <summary>Notas pessoais opcionais do utilizador para o photocard.</summary>
        [BindProperty]
        public string? AddNotas { get; set; }

        /// <summary>
        /// Carrega e filtra os photocards do catálogo conforme a pesquisa e o grupo selecionado.
        /// </summary>
        public async Task OnGetAsync()
        {
            await CarregarGruposSelectList();

            var query = _context.Photocards
                .Include(p => p.Artista!)
                    .ThenInclude(a => a.Grupo!)
                .Include(p => p.Artista!)
                    .ThenInclude(a => a.Solista!)
                .Include(p => p.Album!)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(p => p.Versao.Contains(SearchQuery!) ||
                                         (p.Artista != null && p.Artista.NomeArtistico!.Contains(SearchQuery!)) ||
                                         (p.Artista != null && p.Artista.Grupo != null && p.Artista.Grupo.Nome.Contains(SearchQuery!)) ||
                                         (p.Album != null && p.Album.Titulo.Contains(SearchQuery!)));
            }

            if (GrupoFilter.HasValue)
            {
                query = query.Where(p => p.Artista != null && p.Artista.GrupoId == GrupoFilter.Value);
            }

            Photocards = await query.ToListAsync();
        }

        /// <summary>
        /// Processa o pedido de adição de um photocard ao Binder pessoal do utilizador autenticado.
        /// Se já existir com o mesmo estado, incrementa a quantidade; caso contrário, cria nova entrada.
        /// </summary>
        public async Task<IActionResult> OnPostAdicionarAsync()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Challenge();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Validar se o photocard existe
            var photocard = await _context.Photocards.FindAsync(AddPhotocardId);
            if (photocard == null)
            {
                TempData["ErrorMessage"] = "Photocard não encontrado.";
                return RedirectToPage();
            }

            // Verificar se já existe no Binder com o mesmo estado
            var binderEntry = await _context.UtilizadorPhotocards
                .FirstOrDefaultAsync(up => up.UtilizadorId == user.Id && up.PhotocardId == AddPhotocardId && up.Estado == AddEstado);

            if (binderEntry != null)
            {
                binderEntry.Quantidade += AddQuantidade;
                if (!string.IsNullOrWhiteSpace(AddNotas))
                {
                    binderEntry.Notas = string.IsNullOrEmpty(binderEntry.Notas) 
                        ? AddNotas 
                        : $"{binderEntry.Notas} | {AddNotas}";
                }
            }
            else
            {
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

            await _context.SaveChangesAsync();

            // notificação em tmepo real
            await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Binder",
                Acao = "Adicionado",
                Mensagem = $"Photocard '{photocard.Versao}' foi adicionado ao Binder de {user.Email}!",
                Data = DateTime.Now
            });

            TempData["SuccessMessage"] = $"Photocard adicionado ao teu Binder com sucesso!";
            return RedirectToPage();
        }

        /// <summary>
        /// Método auxiliar que carrega todos os grupos para o dropdown de filtro do catálogo.
        /// </summary>
        private async Task CarregarGruposSelectList()
        {
            var grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .ToListAsync();

            GruposSelectList = new SelectList(grupos, "Id", "Nome");
        }
    }
}