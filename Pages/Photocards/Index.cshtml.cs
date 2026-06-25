using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Photocards
{
    /// <summary>
    /// Página de administração que lista todos os photocards registados no catálogo.
    /// Acesso restrito a utilizadores com o papel de Administrador.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor com injeção de dependência do contexto da base de dados.
        /// </summary>
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Lista completa de photocards para exibição na tabela administrativa.</summary>
        public IList<Photocard> Photocards { get; set; } = default!;

        /// <summary>
        /// Carrega todos os photocards com os dados do artista e álbum associados.
        /// </summary>
        public async Task OnGetAsync()
        {
            // Inclui dados relacionados (Artista e Álbum) para evitar N+1 queries
            Photocards = await _context.Photocards
                .Include(p => p.Artista)
                .Include(p => p.Album)
                .ToListAsync();
        }
    }
}
