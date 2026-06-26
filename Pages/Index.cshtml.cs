using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;

namespace K_Shelf.Pages
{
    /// <summary>
    /// Modelo de suporte para a página inicial (Dashboard) da aplicação.
    /// Carrega estatísticas gerais sobre a quantidade de artistas, álbuns e coleções registados.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Total de artistas na base de dados
        /// </summary>
        public int ArtistasCount { get; set; }

        /// <summary>
        /// Total de álbuns na base de dados
        /// </summary>
        public int AlbunsCount { get; set; }

        /// <summary>
        /// Total de coleções criadas 
        /// </summary>
        public int ColecoesCount { get; set; }

        /// <summary>
        /// Total de photocards na base de dados
        /// </summary>
        public int PhotocardsCount { get; set; }

        /// <summary>
        /// Método invocado na requisição GET da página
        /// Obtém os contadores estatísticos assincronamente
        /// </summary>
        public async Task OnGetAsync()
        {
            ArtistasCount = await _context.Artistas.CountAsync();
            AlbunsCount = await _context.Albuns.CountAsync();
            ColecoesCount = await _context.Colecoes.CountAsync();
            PhotocardsCount = await _context.Photocards.CountAsync();
        }
    }
}