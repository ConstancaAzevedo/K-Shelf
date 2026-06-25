using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// API para gestão de Álbuns K-Pop
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AlbunsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AlbunsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os álbuns
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetAlbuns()
        {
            var albuns = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Include(a => a.Musicas)
                .Select(a => new
                {
                    a.Id,
                    a.Titulo,
                    DataLancamento = a.DataLancamento.ToString("yyyy-MM-dd"),
                    a.CapaUrl,
                    Tipo = a.Tipo.ToString(),
                    Edicao = a.Edicao.ToString(),
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null,
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null,
                    TotalMusicas = a.Musicas != null ? a.Musicas.Count : 0
                })
                .ToListAsync();

            return Ok(albuns);
        }

        /// <summary>
        /// Obtém um álbum por ID, incluindo as músicas
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetAlbum(int id)
        {
            var album = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Include(a => a.Musicas)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Titulo,
                    DataLancamento = a.DataLancamento.ToString("yyyy-MM-dd"),
                    a.CapaUrl,
                    Tipo = a.Tipo.ToString(),
                    Edicao = a.Edicao.ToString(),
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null,
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null,
                    Musicas = a.Musicas != null ? a.Musicas.Select(m => new
                    {
                        m.Id,
                        m.TrackNumber,
                        m.Titulo,
                        m.Duracao,
                        m.IsTitleTrack,
                        m.IsSingle
                    }).OrderBy(m => m.TrackNumber).ToList() : null
                })
                .FirstOrDefaultAsync();

            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });

            return Ok(album);
        }

        /// <summary>
        /// Cria um novo álbum (apenas Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Album>> PostAlbum(Album album)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Albuns.Add(album);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlbum), new { id = album.Id }, album);
        }

        /// <summary>
        /// Atualiza um álbum existente (apenas Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAlbum(int id, Album album)
        {
            if (id != album.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(album).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Albuns.AnyAsync(a => a.Id == id))
                    return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina um álbum (apenas Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            var album = await _context.Albuns
                .Include(a => a.Musicas)
                .Include(a => a.AlbumColecoes)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });

            // Remove relações antes de eliminar
            if (album.Musicas != null)
                _context.Musicas.RemoveRange(album.Musicas);

            if (album.AlbumColecoes != null)
                _context.AlbumColecoes.RemoveRange(album.AlbumColecoes);

            _context.Albuns.Remove(album);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
