using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// API REST para gestão e manipulação de Coleções de álbuns de K-Pop.
    /// Disponibiliza operações CRUD e endpoints para associar/remover álbuns.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ColecoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor da classe ColecoesController.
        /// </summary>
        /// <param name="context">Contexto da base de dados injetado.</param>
        public ColecoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todas as coleções do utilizador autenticado.
        /// </summary>
        /// <returns>Uma lista de coleções contendo detalhes básicos e total de álbuns.</returns>
        /// <response code="200">Retorna a lista de coleções com sucesso.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> GetColecoes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var colecoes = await _context.Colecoes
                .Where(c => c.UtilizadorId == userId || isAdmin)
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
        /// Obtém os detalhes de uma coleção específica pelo seu identificador (ID).
        /// </summary>
        /// <param name="id">Identificador único (ID) da coleção.</param>
        /// <returns>A coleção solicitada contendo a lista de álbuns inseridos.</returns>
        /// <response code="200">Retorna a coleção encontrada com sucesso.</response>
        /// <response code="404">A coleção com o ID fornecido não foi encontrada.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetColecao(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var colecao = await _context.Colecoes
                .Where(c => c.Id == id && (c.UtilizadorId == userId || isAdmin))
                .Include(c => c.AlbumColecoes!)
                    .ThenInclude(ac => ac.Album)
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
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada ou sem permissão de acesso." });

            return Ok(colecao);
        }

        /// <summary>
        /// Cria e regista uma nova coleção no sistema.
        /// </summary>
        /// <param name="colecao">Objeto JSON com as propriedades da coleção a criar.</param>
        /// <returns>A coleção criada e a respetiva rota de detalhes.</returns>
        /// <response code="201">Coleção criada com sucesso.</response>
        /// <response code="400">Os dados submetidos são inválidos.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Colecao>> PostColecao(Colecao colecao)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            colecao.UtilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            colecao.DataCriacao = DateTime.Now;

            // Validar se já existe uma coleção com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == colecao.UtilizadorId && 
                               c.Nome.ToLower() == colecao.Nome.ToLower());

            if (colecaoDuplicada)
                return BadRequest(new { mensagem = $"Já existe uma coleção com o nome \"{colecao.Nome}\"!" });

            _context.Colecoes.Add(colecao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetColecao), new { id = colecao.Id }, colecao);
        }

        /// <summary>
        /// Atualiza as informações de uma coleção existente.
        /// </summary>
        /// <param name="id">ID da coleção a atualizar (deve corresponder ao ID no corpo).</param>
        /// <param name="colecao">Dados atualizados da coleção.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        /// <response code="204">Coleção atualizada com sucesso.</response>
        /// <response code="400">Incompatibilidade de IDs ou dados inválidos.</response>
        /// <response code="404">A coleção solicitada não existe.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutColecao(int id, Colecao colecao)
        {
            if (id != colecao.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var colecaoExistente = await _context.Colecoes.FindAsync(id);
            if (colecaoExistente == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Verificar permissão
            if (colecaoExistente.UtilizadorId != userId && !isAdmin)
                return Forbid();

            // Validar se já existe outra coleção com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == colecaoExistente.UtilizadorId && 
                               c.Id != id && 
                               c.Nome.ToLower() == colecao.Nome.ToLower());

            if (colecaoDuplicada)
                return BadRequest(new { mensagem = $"Já existe uma coleção com o nome \"{colecao.Nome}\"!" });

            colecaoExistente.Nome = colecao.Nome;
            colecaoExistente.Descricao = colecao.Descricao;

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
        /// Remove definitivamente uma coleção do sistema e desassocia todos os álbuns vinculados a ela.
        /// </summary>
        /// <param name="id">ID da coleção a remover.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        /// <response code="204">Coleção removida com sucesso.</response>
        /// <response code="404">Coleção não encontrada.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteColecao(int id)
        {
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Verificar permissão
            if (colecao.UtilizadorId != userId && !isAdmin)
                return Forbid();

            // Remove os vínculos na tabela muitos-para-muitos primeiro para evitar conflitos de FK
            if (colecao.AlbumColecoes != null)
                _context.AlbumColecoes.RemoveRange(colecao.AlbumColecoes);

            _context.Colecoes.Remove(colecao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Adiciona um álbum a uma coleção (cria associação Muitos-para-Muitos).
        /// </summary>
        /// <param name="id">ID da coleção de destino.</param>
        /// <param name="albumId">ID do álbum a associar.</param>
        /// <returns>Objeto JSON indicando o sucesso da operação.</returns>
        /// <response code="200">Álbum associado com sucesso.</response>
        /// <response code="400">O álbum já existe na coleção.</response>
        /// <response code="404">A coleção ou o álbum não existem.</response>
        [HttpPost("{id}/albuns/{albumId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAlbumToColecao(int id, int albumId)
        {
            var colecao = await _context.Colecoes.FindAsync(id);
            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Verificar permissão
            if (colecao.UtilizadorId != userId && !isAdmin)
                return Forbid();

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
        /// Remove um álbum associado a uma coleção (desassocia a relação muitos-para-muitos).
        /// </summary>
        /// <param name="id">ID da coleção.</param>
        /// <param name="albumId">ID do álbum a desassociar.</param>
        /// <returns>Mensagem de confirmação da remoção.</returns>
        /// <response code="200">Álbum removido da coleção com sucesso.</response>
        /// <response code="404">Associação entre o álbum e a coleção não encontrada.</response>
        [HttpDelete("{id}/albuns/{albumId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveAlbumFromColecao(int id, int albumId)
        {
            var colecao = await _context.Colecoes.FindAsync(id);
            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Verificar permissão
            if (colecao.UtilizadorId != userId && !isAdmin)
                return Forbid();

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
