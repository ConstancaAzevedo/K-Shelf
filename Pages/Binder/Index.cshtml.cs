using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Binder
{
    /// <summary>
    /// pagina do binder pessoal de photocards do utilizador autenticado
    /// apresenta os photocards em grelha 3x3 com efeito de rotacao 3d flip,
    /// permitindo filtrar por estado (colecao, wishlist, para troca) e editar ou remover entradas
    /// </summary>
    [Authorize] // apenas utilizadores autenticados podem aceder
    public class IndexModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // gestor de utilizadores do identity
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// construtor com injecao de dependencia do contexto da bd e do gestor de utilizadores
        /// </summary>
        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>lista de entradas do binder do utilizador atual apos filtragem</summary>
        public IList<UtilizadorPhotocard> BinderCards { get; set; } = default!;

        /// <summary>filtro de estado ativo (possui/deseja/paratroca); null mostra todos</summary>
        [BindProperty(SupportsGet = true)]
        public EstadoPhotocard? FilterEstado { get; set; }

        /// <summary>texto de pesquisa para filtrar por versao, artista, grupo ou album</summary>
        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        /// <summary>id do grupo selecionado no filtro dropdown; null se nenhum filtro ativo</summary>
        [BindProperty(SupportsGet = true)]
        public int? GrupoFilter { get; set; }

        /// <summary>lista de grupos para o dropdown de filtro do binder</summary>
        public SelectList GruposSelectList { get; set; } = default!;

        /// <summary>total de photocards na colecao pessoal do utilizador</summary>
        public int TotalPossuiCount { get; set; }

        /// <summary>total de photocards na wishlist do utilizador</summary>
        public int TotalDesejaCount { get; set; }

        /// <summary>total de photocards marcados para troca pelo utilizador</summary>
        public int TotalParaTrocaCount { get; set; }

        /// <summary>
        /// carrega o binder do utilizador autenticado com as estatisticas e os filtros aplicados
        /// </summary>
        public async Task OnGetAsync()
        {
            // obtem o utilizador autenticado
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return; // se nao existir, sai do metodo

            // carrega a lista de grupos para o dropdown
            var grupos = await _context.Grupos.OrderBy(g => g.Nome).ToListAsync();
            GruposSelectList = new SelectList(grupos, "Id", "Nome");

            // carrega as estatisticas do utilizador
            TotalPossuiCount = await _context.UtilizadorPhotocards
                .CountAsync(up => up.UtilizadorId == user.Id && up.Estado == EstadoPhotocard.Possui);
            TotalDesejaCount = await _context.UtilizadorPhotocards
                .CountAsync(up => up.UtilizadorId == user.Id && up.Estado == EstadoPhotocard.Deseja);
            TotalParaTrocaCount = await _context.UtilizadorPhotocards
                .CountAsync(up => up.UtilizadorId == user.Id && up.Estado == EstadoPhotocard.ParaTroca);

            // query base para obter os photocards do utilizador
            var query = _context.UtilizadorPhotocards
                .Include(up => up.Photocard!) // inclui o photocard
                .ThenInclude(p => p.Artista!) // inclui o artista
                    .ThenInclude(a => a.Grupo!) // inclui o grupo do artista
                .Include(up => up.Photocard!)
                .ThenInclude(p => p.Artista!)
                    .ThenInclude(a => a.Solista!) // inclui o solista do artista
                .Include(up => up.Photocard!)
                .ThenInclude(p => p.Album!) // inclui o album
                .Where(up => up.UtilizadorId == user.Id) // filtra pelo utilizador atual
                .AsQueryable();

            // aplica o filtro por estado se existir
            if (FilterEstado.HasValue)
            {
                query = query.Where(up => up.Estado == FilterEstado.Value);
            }

            // aplica o filtro de pesquisa se existir
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(up => (up.Photocard != null && up.Photocard.Versao.Contains(SearchQuery!)) || // pesquisa por versao
                                         (up.Photocard != null && up.Photocard.Artista != null && up.Photocard.Artista.NomeArtistico!.Contains(SearchQuery!)) || // pesquisa por nome artistico
                                         (up.Photocard != null && up.Photocard.Artista != null && up.Photocard.Artista.Grupo != null && up.Photocard.Artista.Grupo.Nome.Contains(SearchQuery!)) || // pesquisa por grupo
                                         (up.Photocard != null && up.Photocard.Album != null && up.Photocard.Album.Titulo.Contains(SearchQuery!))); // pesquisa por album
            }

            // aplica o filtro por grupo se existir
            if (GrupoFilter.HasValue)
            {
                query = query.Where(up => up.Photocard != null && up.Photocard.Artista != null && up.Photocard.Artista.GrupoId == GrupoFilter.Value);
            }

            // executa a query e guarda os resultados
            BinderCards = await query.ToListAsync();
        }

        /// <summary>
        /// atualiza a quantidade e as notas de um photocard no binder do utilizador
        /// </summary>
        /// <param name="id">id da entrada do binder a editar</param>
        /// <param name="editQuantidade">nova quantidade de copias</param>
        /// <param name="editNotas">notas pessoais atualizadas</param>
        public async Task<IActionResult> OnPostEditarAsync(int id, int editQuantidade, string? editNotas)
        {
            // obtem o utilizador autenticado
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // redireciona para o login

            // procura a entrada do binder pelo id e utilizador
            var entry = await _context.UtilizadorPhotocards
                .FirstOrDefaultAsync(up => up.Id == id && up.UtilizadorId == user.Id);

            // se nao existir, mostra erro
            if (entry == null)
            {
                TempData["ErrorMessage"] = "Registo não encontrado no teu Binder.";
                return RedirectToPage();
            }

            // atualiza a quantidade (entre 1 e 100)
            entry.Quantidade = Math.Clamp(editQuantidade, 1, 100);
            // atualiza as notas
            entry.Notas = editNotas;

            // guarda as alteracoes
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Registo atualizado com sucesso!";
            return RedirectToPage(); // redireciona para a pagina atual
        }

        /// <summary>
        /// remove um photocard do binder pessoal do utilizador autenticado
        /// </summary>
        /// <param name="id">id da entrada do binder a remover</param>
        public async Task<IActionResult> OnPostRemoverAsync(int id)
        {
            // obtem o utilizador autenticado
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // redireciona para o login

            // procura a entrada do binder pelo id e utilizador
            var entry = await _context.UtilizadorPhotocards
                .FirstOrDefaultAsync(up => up.Id == id && up.UtilizadorId == user.Id);

            // se existir, remove
            if (entry != null)
            {
                _context.UtilizadorPhotocards.Remove(entry); // remove a entrada
                await _context.SaveChangesAsync(); // guarda as alteracoes
                TempData["SuccessMessage"] = "Photocard removido do teu Binder.";
            }

            return RedirectToPage(); // redireciona para a pagina atual
        }
    }
}