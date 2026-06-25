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
    public class CatalogoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public CatalogoModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Photocard> Photocards { get; set; } = default!;
        public SelectList GruposSelectList { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GrupoFilter { get; set; }

        // Propriedades para adicionar ao Binder
        [BindProperty]
        public int AddPhotocardId { get; set; }

        [BindProperty]
        public EstadoPhotocard AddEstado { get; set; }

        [BindProperty]
        public int AddQuantidade { get; set; } = 1;

        [BindProperty]
        public string? AddNotas { get; set; }

        public async Task OnGetAsync()
        {
            await CarregarGruposSelectList();

            var query = _context.Photocards
                .Include(p => p.Artista)
                    .ThenInclude(a => a.Grupo)
                .Include(p => p.Artista)
                    .ThenInclude(a => a.Solista)
                .Include(p => p.Album)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(p => p.Versao.Contains(SearchQuery) ||
                                         (p.Artista != null && p.Artista.NomeArtistico.Contains(SearchQuery)) ||
                                         (p.Artista != null && p.Artista.Grupo != null && p.Artista.Grupo.Nome.Contains(SearchQuery)) ||
                                         (p.Album != null && p.Album.Titulo.Contains(SearchQuery)));
            }

            if (GrupoFilter.HasValue)
            {
                query = query.Where(p => p.Artista != null && p.Artista.GrupoId == GrupoFilter.Value);
            }

            Photocards = await query.ToListAsync();
        }

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
            TempData["SuccessMessage"] = $"Photocard adicionado ao teu Binder com sucesso!";
            return RedirectToPage();
        }



        private async Task CarregarGruposSelectList()
        {
            var grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .ToListAsync();

            GruposSelectList = new SelectList(grupos, "Id", "Nome");
        }
    }
}
