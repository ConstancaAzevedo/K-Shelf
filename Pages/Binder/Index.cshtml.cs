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
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<UtilizadorPhotocard> BinderCards { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public EstadoPhotocard? FilterEstado { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GrupoFilter { get; set; }

        public SelectList GruposSelectList { get; set; } = default!;

        public int TotalPossuiCount { get; set; }
        public int TotalDesejaCount { get; set; }
        public int TotalParaTrocaCount { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var grupos = await _context.Grupos.OrderBy(g => g.Nome).ToListAsync();
            GruposSelectList = new SelectList(grupos, "Id", "Nome");

            // Carregar estatísticas
            TotalPossuiCount = await _context.UtilizadorPhotocards
                .CountAsync(up => up.UtilizadorId == user.Id && up.Estado == EstadoPhotocard.Possui);
            TotalDesejaCount = await _context.UtilizadorPhotocards
                .CountAsync(up => up.UtilizadorId == user.Id && up.Estado == EstadoPhotocard.Deseja);
            TotalParaTrocaCount = await _context.UtilizadorPhotocards
                .CountAsync(up => up.UtilizadorId == user.Id && up.Estado == EstadoPhotocard.ParaTroca);

            var query = _context.UtilizadorPhotocards
                .Include(up => up.Photocard)
                .ThenInclude(p => p.Artista)
                    .ThenInclude(a => a.Grupo)
                .Include(up => up.Photocard)
                .ThenInclude(p => p.Artista)
                    .ThenInclude(a => a.Solista)
                .Include(up => up.Photocard)
                .ThenInclude(p => p.Album)
                .Where(up => up.UtilizadorId == user.Id)
                .AsQueryable();

            if (FilterEstado.HasValue)
            {
                query = query.Where(up => up.Estado == FilterEstado.Value);
            }

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(up => (up.Photocard != null && up.Photocard.Versao.Contains(SearchQuery)) ||
                                         (up.Photocard != null && up.Photocard.Artista != null && up.Photocard.Artista.NomeArtistico.Contains(SearchQuery)) ||
                                         (up.Photocard != null && up.Photocard.Artista != null && up.Photocard.Artista.Grupo != null && up.Photocard.Artista.Grupo.Nome.Contains(SearchQuery)) ||
                                         (up.Photocard != null && up.Photocard.Album != null && up.Photocard.Album.Titulo.Contains(SearchQuery)));
            }

            if (GrupoFilter.HasValue)
            {
                query = query.Where(up => up.Photocard != null && up.Photocard.Artista != null && up.Photocard.Artista.GrupoId == GrupoFilter.Value);
            }

            BinderCards = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostEditarAsync(int id, int editQuantidade, string? editNotas)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var entry = await _context.UtilizadorPhotocards
                .FirstOrDefaultAsync(up => up.Id == id && up.UtilizadorId == user.Id);

            if (entry == null)
            {
                TempData["ErrorMessage"] = "Registo não encontrado no teu Binder.";
                return RedirectToPage();
            }

            entry.Quantidade = Math.Clamp(editQuantidade, 1, 100);
            entry.Notas = editNotas;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Registo atualizado com sucesso!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoverAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var entry = await _context.UtilizadorPhotocards
                .FirstOrDefaultAsync(up => up.Id == id && up.UtilizadorId == user.Id);

            if (entry != null)
            {
                _context.UtilizadorPhotocards.Remove(entry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Photocard removido do teu Binder.";
            }

            return RedirectToPage();
        }
    }
}
