using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// API para gestão de Artistas K-Pop
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArtistasController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os artistas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetArtistas()
        {
            var artistas = await _context.Artistas
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Select(a => new
                {
                    a.Id,
                    a.Nome,
                    a.NomeArtistico,
                    a.Posicao,
                    a.Nacionalidade,
                    a.ImagemUrl,
                    a.IsAtivo,
                    DataNascimento = a.DataNascimento.ToString("yyyy-MM-dd"),
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null,
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null
                })
                .ToListAsync();

            return Ok(artistas);
        }

        /// <summary>
        /// Obtém um artista por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetArtista(int id)
        {
            var artista = await _context.Artistas
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Nome,
                    a.NomeArtistico,
                    a.Posicao,
                    a.Nacionalidade,
                    a.ImagemUrl,
                    a.IsAtivo,
                    DataNascimento = a.DataNascimento.ToString("yyyy-MM-dd"),
                    DataEntrada = a.DataEntrada.HasValue ? a.DataEntrada.Value.ToString("yyyy-MM-dd") : null,
                    DataSaida = a.DataSaida.HasValue ? a.DataSaida.Value.ToString("yyyy-MM-dd") : null,
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null,
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null
                })
                .FirstOrDefaultAsync();

            if (artista == null)
                return NotFound(new { mensagem = $"Artista com ID {id} não encontrado." });

            return Ok(artista);
        }

        /// <summary>
        /// Cria um novo artista
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Artista>> PostArtista(Artista artista)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArtista), new { id = artista.Id }, artista);
        }

        /// <summary>
        /// Atualiza um artista existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArtista(int id, Artista artista)
        {
            if (id != artista.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(artista).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Artistas.AnyAsync(a => a.Id == id))
                    return NotFound(new { mensagem = $"Artista com ID {id} não encontrado." });
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina um artista
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtista(int id)
        {
            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null)
                return NotFound(new { mensagem = $"Artista com ID {id} não encontrado." });

            _context.Artistas.Remove(artista);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
