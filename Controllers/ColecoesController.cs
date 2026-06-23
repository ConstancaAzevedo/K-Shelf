using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// API para gestão de Coleções de álbuns
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ColecoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ColecoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todas as coleções
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetColecoes()
        {
            var colecoes = await _context.Colecoes
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Descricao,
                    DataCriacao = c.DataCriacao.ToString("yyyy-MM-dd"),
                    c.UtilizadorId,
                    TotalAlbuns = c.AlbumColecoes != null ? c.AlbumColecoes.Count : 0
                })
                .ToListAsync();

            return Ok(colecoes);
        }

        /// <summary>
        /// Obtém uma coleção por ID, incluindo os álbuns
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetColecao(int id)
        {
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Descricao,
                    DataCriacao = c.DataCriacao.ToString("yyyy-MM-dd"),
                    c.UtilizadorId,
                    Albuns = c.AlbumColecoes != null ? c.AlbumColecoes.Select(ac => new
                    {
                        ac.Album!.Id,
                        ac.Album.Titulo,
                        ac.Album.CapaUrl,
                        DataAdicao = ac.DataAdicao.ToString("yyyy-MM-dd")
                    }).ToList() : null
                })
                .FirstOrDefaultAsync();

            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            return Ok(colecao);
        }

        /// <summary>
        /// Cria uma nova coleção
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Colecao>> PostColecao(Colecao colecao)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            colecao.DataCriacao = DateTime.Now;
            _context.Colecoes.Add(colecao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetColecao), new { id = colecao.Id }, colecao);
        }

        /// <summary>
        /// Atualiza uma coleção existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutColecao(int id, Colecao colecao)
        {
            if (id != colecao.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(colecao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Colecoes.AnyAsync(c => c.Id == id))
                    return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina uma coleção
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColecao(int id)
        {
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            if (colecao.AlbumColecoes != null)
                _context.AlbumColecoes.RemoveRange(colecao.AlbumColecoes);

            _context.Colecoes.Remove(colecao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Adiciona um álbum a uma coleção
        /// </summary>
        [HttpPost("{id}/albuns/{albumId}")]
        public async Task<IActionResult> AddAlbumToColecao(int id, int albumId)
        {
            var colecao = await _context.Colecoes.FindAsync(id);
            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            var album = await _context.Albuns.FindAsync(albumId);
            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {albumId} não encontrado." });

            var jaExiste = await _context.AlbumColecoes
                .AnyAsync(ac => ac.ColecaoId == id && ac.AlbumId == albumId);

            if (jaExiste)
                return BadRequest(new { mensagem = "Este álbum já está na coleção." });

            _context.AlbumColecoes.Add(new AlbumColecao
            {
                ColecaoId = id,
                AlbumId = albumId,
                DataAdicao = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Álbum adicionado à coleção com sucesso." });
        }

        /// <summary>
        /// Remove um álbum de uma coleção
        /// </summary>
        [HttpDelete("{id}/albuns/{albumId}")]
        public async Task<IActionResult> RemoveAlbumFromColecao(int id, int albumId)
        {
            var albumColecao = await _context.AlbumColecoes
                .FirstOrDefaultAsync(ac => ac.ColecaoId == id && ac.AlbumId == albumId);

            if (albumColecao == null)
                return NotFound(new { mensagem = "Álbum não encontrado nesta coleção." });

            _context.AlbumColecoes.Remove(albumColecao);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Álbum removido da coleção com sucesso." });
        }
    }
}
