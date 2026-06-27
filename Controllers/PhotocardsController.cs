using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// api rest para consulta e gestao de photocards colecionaveis do catalogo k-shelf
    /// os endpoints de leitura sao publicos; os de escrita requerem o papel de administrador
    /// </summary>
    [Route("api/[controller]")] // define a rota base como api/photocards
    [ApiController] // indica que este controlador e uma api
    public class PhotocardsController : ControllerBase
    {
        // contexto da base de dados
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// construtor com injecao de dependencia do contexto da base de dados
        /// </summary>
        /// <param name="context">contexto da base de dados injetado</param>
        public PhotocardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// obtem todos os photocards do catalogo, incluindo o artista e album associados
        /// </summary>
        /// <returns>lista de todos os photocards disponiveis no catalogo</returns>
        /// <response code="200">retorna a lista de photocards com sucesso</response>
        [HttpGet] // metodo http get
        [AllowAnonymous] // permite acesso sem autenticacao
        [ProducesResponseType(StatusCodes.Status200OK)] // documenta a resposta 200
        public async Task<ActionResult<IEnumerable<object>>> GetPhotocards()
        {
            // obtem todos os photocards com os dados relacionados
            var photocards = await _context.Photocards
                .Include(p => p.Artista) // inclui o artista associado
                    .ThenInclude(a => a!.Grupo) // inclui o grupo do artista
                .Include(p => p.Album) // inclui o album associado
                .Select(p => new // projeta os dados para um objeto anonimo
                {
                    p.Id,
                    p.Versao,
                    p.ImagemUrl,
                    Artista = p.Artista != null ? new // dados do artista
                    {
                        p.Artista.Id,
                        p.Artista.NomeArtistico,
                        p.Artista.Nome,
                        Grupo = p.Artista.Grupo != null ? new { p.Artista.Grupo.Id, p.Artista.Grupo.Nome } : null // dados do grupo ou null
                    } : null,
                    Album = p.Album != null ? new // dados do album
                    {
                        p.Album.Id,
                        p.Album.Titulo,
                        p.Album.CapaUrl
                    } : null
                })
                .ToListAsync();

            return Ok(photocards); // retorna a lista com status 200
        }

        /// <summary>
        /// obtem os detalhes de um photocard especifico pelo seu id
        /// </summary>
        /// <param name="id">identificador unico do photocard</param>
        /// <returns>o photocard encontrado com todos os dados relacionados</returns>
        /// <response code="200">retorna o photocard com sucesso</response>
        /// <response code="404">o photocard com o id fornecido nao foi encontrado</response>
        [HttpGet("{id}")] // metodo http get com parametro id na url
        [AllowAnonymous] // permite acesso sem autenticacao
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetPhotocard(int id)
        {
            // procura o photocard pelo id com os dados relacionados
            var photocard = await _context.Photocards
                .Include(p => p.Artista) // inclui o artista associado
                    .ThenInclude(a => a!.Grupo) // inclui o grupo do artista
                .Include(p => p.Album) // inclui o album associado
                .Where(p => p.Id == id) // filtra pelo id
                .Select(p => new // projeta os dados para um objeto anonimo
                {
                    p.Id,
                    p.Versao,
                    p.ImagemUrl,
                    Artista = p.Artista != null ? new // dados do artista
                    {
                        p.Artista.Id,
                        p.Artista.NomeArtistico,
                        p.Artista.Nome,
                        Grupo = p.Artista.Grupo != null ? new { p.Artista.Grupo.Id, p.Artista.Grupo.Nome } : null // dados do grupo ou null
                    } : null,
                    Album = p.Album != null ? new // dados do album
                    {
                        p.Album.Id,
                        p.Album.Titulo,
                        p.Album.CapaUrl,
                        DataLancamento = p.Album.DataLancamento.ToString("yyyy-MM-dd") // formata a data de lancamento
                    } : null
                })
                .FirstOrDefaultAsync();

            // se o photocard nao existir, retorna 404
            if (photocard == null)
                return NotFound(new { mensagem = $"Photocard com ID {id} não encontrado." });

            return Ok(photocard); // retorna o photocard com status 200
        }

        /// <summary>
        /// cria e regista um novo photocard no catalogo (apenas admin)
        /// </summary>
        /// <param name="photocard">objeto json com os dados do photocard a criar</param>
        /// <returns>o photocard criado e a rota para aceder aos seus detalhes</returns>
        /// <response code="201">photocard criado com sucesso</response>
        /// <response code="400">os dados submetidos sao invalidos</response>
        [HttpPost] // metodo http post
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Photocard>> PostPhotocard(Photocard photocard)
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // valida que o artista existe na base de dados
            var artistaExiste = await _context.Artistas.AnyAsync(a => a.Id == photocard.ArtistaId);
            if (!artistaExiste)
                return BadRequest(new { mensagem = $"Artista com ID {photocard.ArtistaId} não encontrado." });

            // valida que o album existe, se for fornecido
            if (photocard.AlbumId.HasValue)
            {
                var albumExiste = await _context.Albuns.AnyAsync(a => a.Id == photocard.AlbumId);
                if (!albumExiste)
                    return BadRequest(new { mensagem = $"Álbum com ID {photocard.AlbumId} não encontrado." });
            }

            // adiciona o photocard ao contexto
            _context.Photocards.Add(photocard);
            await _context.SaveChangesAsync(); // guarda na base de dados

            // retorna o photocard criado com status 201 e a localizacao
            return CreatedAtAction(nameof(GetPhotocard), new { id = photocard.Id }, photocard);
        }

        /// <summary>
        /// atualiza os dados de um photocard existente no catalogo (apenas admin)
        /// </summary>
        /// <param name="id">id do photocard a atualizar (deve corresponder ao id no corpo)</param>
        /// <param name="photocard">dados atualizados do photocard</param>
        /// <returns>sem conteudo em caso de sucesso</returns>
        /// <response code="204">photocard atualizado com sucesso</response>
        /// <response code="400">incompatibilidade de ids ou dados invalidos</response>
        /// <response code="404">o photocard solicitado nao existe</response>
        [HttpPut("{id}")] // metodo http put com parametro id na url
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPhotocard(int id, Photocard photocard)
        {
            // verifica se o id da url corresponde ao id do objeto
            if (id != photocard.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // valida que o artista existe na base de dados
            var artistaExiste = await _context.Artistas.AnyAsync(a => a.Id == photocard.ArtistaId);
            if (!artistaExiste)
                return BadRequest(new { mensagem = $"Artista com ID {photocard.ArtistaId} não encontrado." });

            // marca o objeto como modificado
            _context.Entry(photocard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // guarda as alteracoes
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se o photocard ainda existe
                if (!await _context.Photocards.AnyAsync(p => p.Id == id))
                    return NotFound(new { mensagem = $"Photocard com ID {id} não encontrado." });
                throw; // relanca a excecao se nao for um problema de concorrencia
            }

            return NoContent(); // retorna status 204 (sem conteudo)
        }

        /// <summary>
        /// elimina definitivamente um photocard do catalogo (apenas admin)
        /// remove tambem todas as associacoes ao binder pessoal dos utilizadores
        /// </summary>
        /// <param name="id">id do photocard a eliminar</param>
        /// <returns>sem conteudo em caso de sucesso</returns>
        /// <response code="204">photocard eliminado com sucesso</response>
        /// <response code="404">photocard nao encontrado</response>
        [HttpDelete("{id}")] // metodo http delete com parametro id na url
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePhotocard(int id)
        {
            // procura o photocard com as relacoes
            var photocard = await _context.Photocards
                .Include(p => p.UtilizadorPhotocards) // inclui a relacao com os utilizadores
                .FirstOrDefaultAsync(p => p.Id == id);

            // se o photocard nao existir, retorna 404
            if (photocard == null)
                return NotFound(new { mensagem = $"Photocard com ID {id} não encontrado." });

            // remove primeiro as entradas do binder para evitar conflitos de chave estrangeira
            if (photocard.UtilizadorPhotocards != null)
                _context.UtilizadorPhotocards.RemoveRange(photocard.UtilizadorPhotocards); // remove todas as associacoes

            // remove o photocard
            _context.Photocards.Remove(photocard);
            await _context.SaveChangesAsync(); // guarda as alteracoes

            return NoContent(); // retorna status 204 (sem conteudo)
        }
    }
}