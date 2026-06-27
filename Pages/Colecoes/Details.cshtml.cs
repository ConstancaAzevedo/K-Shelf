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
    /// modelo de pagina para os detalhes de uma colecao especifica
    /// exige autenticacao de utilizador. permite ver albuns inseridos, adicionar novos ou remove-los
    /// </summary>
    [Authorize] // requer autenticacao para aceder a esta pagina
    public class DetailsModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// construtor do detailsmodel
        /// </summary>
        /// <param name="context">contexto da base de dados injetado</param>
        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>a colecao cujos detalhes estao a ser consultados</summary>
        public Colecao Colecao { get; set; } = default!;

        /// <summary>indica se o utilizador autenticado e o dono desta colecao</summary>
        public bool IsOwner { get; set; }

        /// <summary>lista de selecao (dropdown) com albuns do sistema disponiveis para adicionar</summary>
        public SelectList AlbunsDisponiveis { get; set; } = default!;

        /// <summary>propriedade ligada ao formulario para armazenar o id do album selecionado para adicionar</summary>
        [BindProperty]
        public int AlbumIdParaAdicionar { get; set; }

        /// <summary>
        /// carrega os dados da colecao, valida se o utilizador tem permissoes de acesso,
        /// e carrega a lista de albuns elegiveis para adicao
        /// </summary>
        /// <param name="id">id da colecao</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da coleção não fornecido";
                return NotFound(); // retorna erro 404
            }

            // carrega a colecao incluindo as relacoes muitos-para-muitos com albuns e respetivos criadores (grupos/solistas)
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes!) // inclui a relacao com os albuns
                    .ThenInclude(ac => ac.Album) // inclui os dados do album
                        .ThenInclude(a => a!.Grupo) // inclui o grupo do album
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album) // inclui os dados do album
                        .ThenInclude(a => a!.Solista) // inclui o solista do album
                .Include(c => c.Utilizador) // inclui os dados do utilizador
                .FirstOrDefaultAsync(c => c.Id == id);

            // se a colecao nao existir, retorna erro
            if (colecao == null)
            {
                TempData["ErrorMessage"] = "Coleção não encontrada";
                return NotFound(); // retorna erro 404
            }

            // regra de controlo de acesso: apenas o dono ou administrador pode ver os detalhes da colecao
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Não tem permissão para aceder a esta coleção";
                return Forbid(); // retorna erro 403
            }

            // atribui a colecao a propriedade da pagina
            Colecao = colecao;
            // define se o utilizador e o dono da colecao
            IsOwner = colecao.UtilizadorId == userId;

            // carrega apenas os albuns que ainda nao pertencem a esta colecao
            var albumIdsNaColecao = colecao.AlbumColecoes?.Select(ac => ac.AlbumId).ToList() ?? new List<int>();
            var albunsPossiveis = await _context.Albuns
                .Include(a => a.Grupo) // inclui o grupo do album
                .Include(a => a.Solista) // inclui o solista do album
                .Where(a => !albumIdsNaColecao.Contains(a.Id)) // filtra os albuns que ja estao na colecao
                .OrderBy(a => a.Titulo) // ordena por titulo
                .ToListAsync();

            // monta a lista do dropdown com "titulo do album — nome do artista/grupo"
            AlbunsDisponiveis = new SelectList(
                albunsPossiveis.Select(a => new {
                    a.Id,
                    Nome = $"{a.Titulo} — {a.Grupo?.Nome ?? a.Solista?.Nome ?? "Independente"}" // formato amigavel
                }),
                "Id", "Nome" // valor = id, texto = nome
            );

            return Page(); // retorna a pagina
        }

        /// <summary>
        /// endpoint post para associar um novo album a colecao do utilizador
        /// </summary>
        /// <param name="id">id da colecao</param>
        public async Task<IActionResult> OnPostAdicionarAlbumAsync(int id)
        {
            // procura a colecao pelo id com os albuns associados
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes) // inclui a relacao com os albuns
                .FirstOrDefaultAsync(c => c.Id == id);

            // se a colecao nao existir, retorna erro
            if (colecao == null)
            {
                TempData["ErrorMessage"] = "Coleção não encontrada";
                return NotFound(); // retorna erro 404
            }

            // garante que so o proprietario ou admin podem adicionar albuns
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Não tem permissão para adicionar álbuns a esta coleção";
                return Forbid(); // retorna erro 403
            }

            // verifica se o album existe
            var album = await _context.Albuns.FindAsync(AlbumIdParaAdicionar);
            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return RedirectToPage("./Details", new { id }); // redireciona para os detalhes
            }

            // evita duplicacoes de registo na tabela muitos-para-muitos
            var jaExiste = await _context.AlbumColecoes
                .AnyAsync(ac => ac.AlbumId == AlbumIdParaAdicionar && ac.ColecaoId == id);

            if (jaExiste)
            {
                TempData["WarningMessage"] = $"O álbum \"{album.Titulo}\" já está nesta coleção!";
                return RedirectToPage("./Details", new { id }); // redireciona para os detalhes
            }

            // adiciona o album a colecao se o id for valido
            if (AlbumIdParaAdicionar > 0)
            {
                try
                {
                    // cria a associacao muitos-para-muitos
                    _context.AlbumColecoes.Add(new AlbumColecao
                    {
                        AlbumId = AlbumIdParaAdicionar,
                        ColecaoId = id,
                        DataAdicao = DateTime.Now // define a data de adicao como agora
                    });
                    await _context.SaveChangesAsync(); // guarda na base de dados
                    TempData["SuccessMessage"] = $"Álbum \"{album.Titulo}\" adicionado à coleção com sucesso!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Erro ao adicionar álbum: {ex.Message}";
                }
            }

            return RedirectToPage("./Details", new { id }); // redireciona para os detalhes
        }

        /// <summary>
        /// endpoint post para remover a associacao de um album da colecao
        /// </summary>
        /// <param name="id">id da colecao</param>
        /// <param name="albumId">id do album a remover</param>
        public async Task<IActionResult> OnPostRemoverAlbumAsync(int id, int albumId)
        {
            // procura a colecao pelo id com os albuns associados
            var colecao = await _context.Colecoes
                            .Include(c => c.AlbumColecoes) // inclui a relacao com os albuns
                            .FirstOrDefaultAsync(c => c.Id == id);

            // se a colecao nao existir, retorna erro
            if (colecao == null)
            {
                TempData["ErrorMessage"] = "Coleção não encontrada";
                return NotFound(); // retorna erro 404
            }

            // garante que so o proprietario ou admin podem remover albuns
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (colecao.UtilizadorId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Não tem permissão para remover álbuns desta coleção";
                return Forbid(); // retorna erro 403
            }

            // verifica se o album existe
            var album = await _context.Albuns.FindAsync(albumId);
            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return RedirectToPage("./Details", new { id }); // redireciona para os detalhes
            }

            // procura a associacao entre o album e a colecao
            var albumColecao = await _context.AlbumColecoes
                  .FirstOrDefaultAsync(ac => ac.AlbumId == albumId && ac.ColecaoId == id);

            // se a associacao existir, remove
            if (albumColecao != null)
            {
                try
                {
                    _context.AlbumColecoes.Remove(albumColecao); // remove a associacao
                    await _context.SaveChangesAsync(); // guarda na base de dados
                    TempData["SuccessMessage"] = $"Álbum \"{album.Titulo}\" removido da coleção com sucesso";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Erro ao remover álbum: {ex.Message}";
                }
            }
            else
            {
                TempData["WarningMessage"] = $"O álbum \"{album.Titulo}\" não está nesta coleção";
            }

            return RedirectToPage("./Details", new { id }); // redireciona para os detalhes
        }
    }
}