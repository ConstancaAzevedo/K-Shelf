using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// api para gestao de albuns k-pop
    /// </summary>
    [Route("api/[controller]")] // define a rota base como api/albuns
    [ApiController] // indica que este controlador e uma api
    public class AlbunsController : ControllerBase
    {
        // contexto da base de dados
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public AlbunsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// obtem todos os albuns
        /// </summary>
        [HttpGet] // metodo http get
        [AllowAnonymous] // permite acesso sem autenticacao
        public async Task<ActionResult<IEnumerable<object>>> GetAlbuns()
        {
            // obtem todos os albuns com os dados relacionados
            var albuns = await _context.Albuns
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .Include(a => a.Musicas) // inclui as musicas do album
                .Select(a => new // projeta os dados para um objeto anonimo
                {
                    a.Id,
                    a.Titulo,
                    DataLancamento = a.DataLancamento.ToString("yyyy-MM-dd"), // formata a data
                    a.CapaUrl,
                    Tipo = a.Tipo.ToString(), // converte o enum para string
                    Edicao = a.Edicao.ToString(), // converte o enum para string
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null, // dados do grupo ou null
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null, // dados do solista ou null
                    TotalMusicas = a.Musicas != null ? a.Musicas.Count : 0 // contagem de musicas
                })
                .ToListAsync();

            return Ok(albuns); // retorna a lista com status 200
        }

        /// <summary>
        /// obtem um album por id, incluindo as musicas
        /// </summary>
        [HttpGet("{id}")] // metodo http get com parametro id na url
        [AllowAnonymous] // permite acesso sem autenticacao
        public async Task<ActionResult<object>> GetAlbum(int id)
        {
            // procura o album pelo id com os dados relacionados
            var album = await _context.Albuns
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .Include(a => a.Musicas) // inclui as musicas do album
                .Where(a => a.Id == id) // filtra pelo id
                .Select(a => new // projeta os dados para um objeto anonimo
                {
                    a.Id,
                    a.Titulo,
                    DataLancamento = a.DataLancamento.ToString("yyyy-MM-dd"), // formata a data
                    a.CapaUrl,
                    Tipo = a.Tipo.ToString(), // converte o enum para string
                    Edicao = a.Edicao.ToString(), // converte o enum para string
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null, // dados do grupo ou null
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null, // dados do solista ou null
                    Musicas = a.Musicas != null ? a.Musicas.Select(m => new // lista de musicas
                    {
                        m.Id,
                        m.TrackNumber,
                        m.Titulo,
                        m.Duracao,
                        m.IsTitleTrack,
                        m.IsSingle
                    }).OrderBy(m => m.TrackNumber).ToList() : null // ordena as musicas por track number
                })
                .FirstOrDefaultAsync();

            // se o album nao existir, retorna 404
            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });

            return Ok(album); // retorna o album com status 200
        }

        /// <summary>
        /// cria um novo album (apenas admin)
        /// </summary>
        [HttpPost] // metodo http post
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<ActionResult<Album>> PostAlbum(Album album)
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // adiciona o album ao contexto
            _context.Albuns.Add(album);
            await _context.SaveChangesAsync(); // guarda na base de dados

            // retorna o album criado com status 201 e a localizacao
            return CreatedAtAction(nameof(GetAlbum), new { id = album.Id }, album);
        }

        /// <summary>
        /// atualiza um album existente (apenas admin)
        /// </summary>
        [HttpPut("{id}")] // metodo http put com parametro id na url
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<IActionResult> PutAlbum(int id, Album album)
        {
            // verifica se o id da url corresponde ao id do objeto
            if (id != album.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // marca o objeto como modificado
            _context.Entry(album).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // guarda as alteracoes
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se o album ainda existe
                if (!await _context.Albuns.AnyAsync(a => a.Id == id))
                    return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });
                throw; // relanca a excecao se nao for um problema de concorrencia
            }

            return NoContent(); // retorna status 204 (sem conteudo)
        }

        /// <summary>
        /// elimina um album (apenas admin)
        /// </summary>
        [HttpDelete("{id}")] // metodo http delete com parametro id na url
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            // procura o album com as relacoes necessarias
            var album = await _context.Albuns
                .Include(a => a.Musicas) // inclui as musicas
                .Include(a => a.AlbumColecoes) // inclui as relacoes com colecoes
                .FirstOrDefaultAsync(a => a.Id == id);

            // se o album nao existir, retorna 404
            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });

            // remove as relacoes antes de eliminar o album
            if (album.Musicas != null)
                _context.Musicas.RemoveRange(album.Musicas); // remove todas as musicas

            if (album.AlbumColecoes != null)
                _context.AlbumColecoes.RemoveRange(album.AlbumColecoes); // remove todas as relacoes com colecoes

            _context.Albuns.Remove(album); // remove o album
            await _context.SaveChangesAsync(); // guarda as alteracoes

            return NoContent(); // retorna status 204 (sem conteudo)
        }

        /// <summary>
        /// adiciona uma musica existente a um album (apenas admin)
        /// </summary>
        [HttpPost("{id}/musicas/{musicaId}")] // rota: api/albuns/{id}/musicas/{musicaId}
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<IActionResult> AdicionarMusica(int id, int musicaId)
        {
            // procura o album com as musicas
            var album = await _context.Albuns
                .Include(a => a.Musicas)
                .FirstOrDefaultAsync(a => a.Id == id);

            // se o album nao existir, retorna 404
            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });

            // procura a musica pelo id
            var musica = await _context.Musicas.FindAsync(musicaId);
            // se a musica nao existir, retorna 404
            if (musica == null)
                return NotFound(new { mensagem = $"Música com ID {musicaId} não encontrada." });

            // verifica se a musica ja esta no album
            if (album.Musicas.Any(m => m.Id == musicaId))
                return BadRequest(new { mensagem = "Esta música já está associada a este álbum." });

            // adiciona a musica ao album
            album.Musicas.Add(musica);
            await _context.SaveChangesAsync(); // guarda as alteracoes

            return Ok(new { mensagem = "Música adicionada com sucesso!" }); // retorna sucesso
        }

        /// <summary>
        /// remove uma musica de um album (apenas admin)
        /// </summary>
        [HttpDelete("{id}/musicas/{musicaId}")] // rota: api/albuns/{id}/musicas/{musicaId}
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<IActionResult> RemoverMusica(int id, int musicaId)
        {
            // procura o album com as musicas
            var album = await _context.Albuns
                .Include(a => a.Musicas)
                .FirstOrDefaultAsync(a => a.Id == id);

            // se o album nao existir, retorna 404
            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {id} não encontrado." });

            // procura a musica no album
            var musica = album.Musicas.FirstOrDefault(m => m.Id == musicaId);
            // se a musica nao estiver no album, retorna 404
            if (musica == null)
                return NotFound(new { mensagem = $"Música com ID {musicaId} não encontrada neste álbum." });

            // remove a musica do album
            album.Musicas.Remove(musica);
            await _context.SaveChangesAsync(); // guarda as alteracoes

            return Ok(new { mensagem = "Música removida com sucesso!" }); // retorna sucesso
        }
    }
}