using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Security.Claims;

namespace K_Shelf.Pages.Colecoes
{
    /// <summary>
    /// Modelo de Página para os detalhes de uma Coleção específica.
    /// Exige autenticação de utilizador. Permite ver álbuns inseridos, adicionar novos ou removê-los.
    /// </summary>
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor do DetailsModel.
        /// </summary>
        /// <param name="context">Contexto da base de dados injetado.</param>
        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>A coleção cujos detalhes estão a ser consultados.</summary>
        public Colecao Colecao { get; set; } = default!;

        /// <summary>Indica se o utilizador autenticado é o dono desta coleção.</summary>
        public bool IsOwner { get; set; }

        /// <summary>Lista de seleção (dropdown) com álbuns do sistema disponíveis para adicionar.</summary>
        public SelectList AlbunsDisponiveis { get; set; } = default!;

        /// <summary>Propriedade ligada ao formulário para armazenar o ID do álbum selecionado para adicionar.</summary>
        [BindProperty]
        public int AlbumIdParaAdicionar { get; set; }

        /// <summary>
        /// Carrega os dados da coleção, valida se o utilizador tem permissões de acesso,
        /// e carrega a lista de álbuns elegíveis para adição.
        /// </summary>
        /// <param name="id">ID da coleção.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            // Carrega a coleção incluindo as relações muitos-para-muitos com Álbuns e respetivos criadores (Grupos/Solistas)
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                        .ThenInclude(a => a!.Grupo)
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                        .ThenInclude(a => a!.Solista)
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound();

            // Regra de Controlo de Acesso: Apenas o dono ou um Administrador pode ver os detalhes da coleção
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            Colecao = colecao;
            IsOwner = colecao.UtilizadorId == userId;

            // Carrega apenas os álbuns que ainda NÃO pertencem a esta coleção
            var albumIdsNaColecao = colecao.AlbumColecoes?.Select(ac => ac.AlbumId).ToList() ?? new List<int>();
            var albunsPossiveis = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Where(a => !albumIdsNaColecao.Contains(a.Id))
                .OrderBy(a => a.Titulo)
                .ToListAsync();

            // Monta a lista do dropdown com "Título do Álbum — Nome do Artista/Grupo"
            AlbunsDisponiveis = new SelectList(
                albunsPossiveis.Select(a => new {
                    a.Id,
                    Nome = $"{a.Titulo} — {a.Grupo?.Nome ?? a.Solista?.Nome ?? "Independente"}"
                }),
                "Id", "Nome"
            );

            return Page();
        }

        /// <summary>
        /// Endpoint POST para associar um novo álbum à coleção do utilizador.
        /// </summary>
        /// <param name="id">ID da coleção.</param>
        public async Task<IActionResult> OnPostAdicionarAlbumAsync(int id)
        {
            var colecao = await _context.Colecoes.FindAsync(id);
            if (colecao == null)
                return NotFound();

            // Garante que só o proprietário ou Admin podem adicionar álbuns
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Evita duplicações de registo na tabela Muitos-para-Muitos
            var jaExiste = await _context.AlbumColecoes
                .AnyAsync(ac => ac.AlbumId == AlbumIdParaAdicionar && ac.ColecaoId == id);

            if (!jaExiste && AlbumIdParaAdicionar > 0)
            {
                _context.AlbumColecoes.Add(new AlbumColecao
                {
                    AlbumId = AlbumIdParaAdicionar,
                    ColecaoId = id,
                    DataAdicao = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Álbum adicionado à coleção!";
            }

            return RedirectToPage("./Details", new { id });
        }

        /// <summary>
        /// Endpoint POST para remover a associação de um álbum da coleção.
        /// </summary>
        /// <param name="id">ID da coleção.</param>
        /// <param name="albumId">ID do álbum a remover.</param>
        public async Task<IActionResult> OnPostRemoverAlbumAsync(int id, int albumId)
        {
            var colecao = await _context.Colecoes.FindAsync(id);
            if (colecao == null)
                return NotFound();

            // Garante que só o proprietário ou Admin podem remover álbuns
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var albumColecao = await _context.AlbumColecoes
                .FirstOrDefaultAsync(ac => ac.AlbumId == albumId && ac.ColecaoId == id);

            if (albumColecao != null)
            {
                _context.AlbumColecoes.Remove(albumColecao);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Álbum removido da coleção.";
            }

            return RedirectToPage("./Details", new { id });
        }
    }
}
