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
    /// pagina de administracao que lista todos os photocards registados no catalogo
    /// acesso restrito a utilizadores com o papel de administrador
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// construtor com injecao de dependencia do contexto da base de dados
        /// </summary>
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>lista completa de photocards para exibicao na tabela administrativa</summary>
        public IList<Photocard> Photocards { get; set; } = default!;

        /// <summary>
        /// carrega todos os photocards com os dados do artista e album associados
        /// </summary>
        public async Task OnGetAsync()
        {
            // obtem todos os photocards da base de dados com os dados relacionados
            // inclui dados relacionados (artista e album) para evitar n+1 queries
            Photocards = await _context.Photocards
                .Include(p => p.Artista) // inclui o artista associado
                .Include(p => p.Album) // inclui o album associado
                .ToListAsync(); // converte para lista
        }
    }
}