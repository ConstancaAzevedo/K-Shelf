using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// API REST para consulta e gestão de Photocards colecionáveis do catálogo K-Shelf.
    /// Os endpoints de leitura são públicos; os de escrita requerem o papel de Administrador.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PhotocardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor com injeção de dependência do contexto da base de dados.
        /// </summary>
        /// <param name="context">Contexto da base de dados injetado.</param>
        public PhotocardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os photocards do catálogo, incluindo o artista e álbum associados.
        /// </summary>
        /// <returns>Lista de todos os photocards disponíveis no catálogo.</returns>
        /// <response code="200">Retorna a lista de photocards com sucesso.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> GetPhotocards()
        {
            var photocards = await _context.Photocards
                .Include(p => p.Artista)
                    .ThenInclude(a => a!.Grupo)
                .Include(p => p.Album)
                .Select(p => new
                {
                    p.Id,
                    p.Versao,
                    p.ImagemUrl,
                    Artista = p.Artista != null ? new
                    {
                        p.Artista.Id,
                        p.Artista.NomeArtistico,
                        p.Artista.Nome,
                        Grupo = p.Artista.Grupo != null ? new { p.Artista.Grupo.Id, p.Artista.Grupo.Nome } : null
                    } : null,
                    Album = p.Album != null ? new
                    {
                        p.Album.Id,
                        p.Album.Titulo,
                        p.Album.CapaUrl
                    } : null
                })
                .ToListAsync();

            return Ok(photocards);
        }

        /// <summary>
        /// Obtém os detalhes de um photocard específico pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único do photocard.</param>
        /// <returns>O photocard encontrado com todos os dados relacionados.</returns>
        /// <response code="200">Retorna o photocard com sucesso.</response>
        /// <response code="404">O photocard com o ID fornecido não foi encontrado.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetPhotocard(int id)
        {
            var photocard = await _context.Photocards
                .Include(p => p.Artista)
                    .ThenInclude(a => a!.Grupo)
                .Include(p => p.Album)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Versao,
                    p.ImagemUrl,
                    Artista = p.Artista != null ? new
                    {
                        p.Artista.Id,
                        p.Artista.NomeArtistico,
                        p.Artista.Nome,
                        Grupo = p.Artista.Grupo != null ? new { p.Artista.Grupo.Id, p.Artista.Grupo.Nome } : null
                    } : null,
                    Album = p.Album != null ? new
                    {
                        p.Album.Id,
                        p.Album.Titulo,
                        p.Album.CapaUrl,
                        DataLancamento = p.Album.DataLancamento.ToString("yyyy-MM-dd")
                    } : null
                })
                .FirstOrDefaultAsync();

            if (photocard == null)
                return NotFound(new { mensagem = $"Photocard com ID {id} não encontrado." });

            return Ok(photocard);
        }

        /// <summary>
        /// Cria e regista um novo photocard no catálogo (apenas Admin).
        /// </summary>
        /// <param name="photocard">Objeto JSON com os dados do photocard a criar.</param>
        /// <returns>O photocard criado e a rota para aceder aos seus detalhes.</returns>
        /// <response code="201">Photocard criado com sucesso.</response>
        /// <response code="400">Os dados submetidos são inválidos.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Photocard>> PostPhotocard(Photocard photocard)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que o artista existe na base de dados
            var artistaExiste = await _context.Artistas.AnyAsync(a => a.Id == photocard.ArtistaId);
            if (!artistaExiste)
                return BadRequest(new { mensagem = $"Artista com ID {photocard.ArtistaId} não encontrado." });

            // Validar que o álbum existe, se for fornecido
            if (photocard.AlbumId.HasValue)
            {
                var albumExiste = await _context.Albuns.AnyAsync(a => a.Id == photocard.AlbumId);
                if (!albumExiste)
                    return BadRequest(new { mensagem = $"Álbum com ID {photocard.AlbumId} não encontrado." });
            }

            _context.Photocards.Add(photocard);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhotocard), new { id = photocard.Id }, photocard);
        }

        /// <summary>
        /// Atualiza os dados de um photocard existente no catálogo (apenas Admin).
        /// </summary>
        /// <param name="id">ID do photocard a atualizar (deve corresponder ao ID no corpo).</param>
        /// <param name="photocard">Dados atualizados do photocard.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        /// <response code="204">Photocard atualizado com sucesso.</response>
        /// <response code="400">Incompatibilidade de IDs ou dados inválidos.</response>
        /// <response code="404">O photocard solicitado não existe.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPhotocard(int id, Photocard photocard)
        {
            if (id != photocard.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que o artista existe
            var artistaExiste = await _context.Artistas.AnyAsync(a => a.Id == photocard.ArtistaId);
            if (!artistaExiste)
                return BadRequest(new { mensagem = $"Artista com ID {photocard.ArtistaId} não encontrado." });

            _context.Entry(photocard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Photocards.AnyAsync(p => p.Id == id))
                    return NotFound(new { mensagem = $"Photocard com ID {id} não encontrado." });
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina definitivamente um photocard do catálogo (apenas Admin).
        /// Remove também todas as associações ao Binder pessoal dos utilizadores.
        /// </summary>
        /// <param name="id">ID do photocard a eliminar.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        /// <response code="204">Photocard eliminado com sucesso.</response>
        /// <response code="404">Photocard não encontrado.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePhotocard(int id)
        {
            var photocard = await _context.Photocards
                .Include(p => p.UtilizadorPhotocards)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photocard == null)
                return NotFound(new { mensagem = $"Photocard com ID {id} não encontrado." });

            // Remove primeiro as entradas do Binder para evitar conflitos de chave estrangeira
            if (photocard.UtilizadorPhotocards != null)
                _context.UtilizadorPhotocards.RemoveRange(photocard.UtilizadorPhotocards);

            _context.Photocards.Remove(photocard);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
