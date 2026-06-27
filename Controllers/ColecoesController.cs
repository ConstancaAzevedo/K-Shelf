using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// api rest para gestao e manipulacao de colecoes de albuns de k-pop
    /// disponibiliza operacoes crud e endpoints para associar/remover albuns
    /// </summary>
    [Route("api/[controller]")] // define a rota base como api/colecoes
    [ApiController] // indica que este controlador e uma api
    [Authorize] // exige autenticacao para todos os endpoints
    public class ColecoesController : ControllerBase
    {
        // contexto da base de dados
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// construtor da classe colecoescontroller
        /// </summary>
        /// <param name="context">contexto da base de dados injetado</param>
        public ColecoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// obtem todas as colecoes do utilizador autenticado
        /// </summary>
        /// <returns>uma lista de colecoes contendo detalhes basicos e total de albuns</returns>
        /// <response code="200">retorna a lista de colecoes com sucesso</response>
        [HttpGet] // metodo http get
        [ProducesResponseType(StatusCodes.Status200OK)] // documenta a resposta 200
        public async Task<ActionResult<IEnumerable<object>>> GetColecoes()
        {
            // obtem o id do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // verifica se o utilizador e admin
            var isAdmin = User.IsInRole("Admin");

            // obtem as colecoes do utilizador ou todas se for admin
            var colecoes = await _context.Colecoes
                .Where(c => c.UtilizadorId == userId || isAdmin) // filtra por utilizador ou admin
                .Include(c => c.AlbumColecoes!) // inclui a relacao com os albuns
                    .ThenInclude(ac => ac.Album) // inclui os dados do album
                .Select(c => new // projeta os dados para um objeto anonimo
                {
                    c.Id,
                    c.Nome,
                    c.Descricao,
                    DataCriacao = c.DataCriacao.ToString("yyyy-MM-dd"), // formata a data
                    c.UtilizadorId,
                    TotalAlbuns = c.AlbumColecoes != null ? c.AlbumColecoes.Count : 0 // conta os albuns
                })
                .ToListAsync();

            return Ok(colecoes); // retorna a lista com status 200
        }

        /// <summary>
        /// obtem os detalhes de uma colecao especifica pelo seu identificador (id)
        /// </summary>
        /// <param name="id">identificador unico (id) da colecao</param>
        /// <returns>a colecao solicitada contendo a lista de albuns inseridos</returns>
        /// <response code="200">retorna a colecao encontrada com sucesso</response>
        /// <response code="404">a colecao com o id fornecido nao foi encontrada</response>
        [HttpGet("{id}")] // metodo http get com parametro id na url
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetColecao(int id)
        {
            // obtem o id do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // verifica se o utilizador e admin
            var isAdmin = User.IsInRole("Admin");

            // procura a colecao pelo id
            var colecao = await _context.Colecoes
                .Where(c => c.Id == id && (c.UtilizadorId == userId || isAdmin)) // filtra por id e permissao
                .Include(c => c.AlbumColecoes!) // inclui a relacao com os albuns
                    .ThenInclude(ac => ac.Album) // inclui os dados do album
                .Select(c => new // projeta os dados para um objeto anonimo
                {
                    c.Id,
                    c.Nome,
                    c.Descricao,
                    DataCriacao = c.DataCriacao.ToString("yyyy-MM-dd"), // formata a data
                    c.UtilizadorId,
                    Albuns = c.AlbumColecoes != null ? c.AlbumColecoes.Select(ac => new // lista de albuns
                    {
                        ac.Album!.Id,
                        ac.Album.Titulo,
                        ac.Album.CapaUrl,
                        DataAdicao = ac.DataAdicao.ToString("yyyy-MM-dd") // formata a data de adicao
                    }).ToList() : null
                })
                .FirstOrDefaultAsync();

            // se a colecao nao existir, retorna 404
            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada ou sem permissão de acesso." });

            return Ok(colecao); // retorna a colecao com status 200
        }

        /// <summary>
        /// cria e regista uma nova colecao no sistema
        /// </summary>
        /// <param name="colecao">objeto json com as propriedades da colecao a criar</param>
        /// <returns>a colecao criada e a respetiva rota de detalhes</returns>
        /// <response code="201">colecao criada com sucesso</response>
        /// <response code="400">os dados submetidos sao invalidos</response>
        [HttpPost] // metodo http post
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Colecao>> PostColecao(Colecao colecao)
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // associa a colecao ao utilizador autenticado
            colecao.UtilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            // define a data de criacao como agora
            colecao.DataCriacao = DateTime.Now;

            // valida se ja existe uma colecao com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == colecao.UtilizadorId &&
                               c.Nome.ToLower() == colecao.Nome.ToLower());

            // se existir, retorna erro
            if (colecaoDuplicada)
                return BadRequest(new { mensagem = $"Já existe uma coleção com o nome \"{colecao.Nome}\"!" });

            // adiciona a colecao ao contexto
            _context.Colecoes.Add(colecao);
            await _context.SaveChangesAsync(); // guarda na base de dados

            // retorna a colecao criada com status 201 e a localizacao
            return CreatedAtAction(nameof(GetColecao), new { id = colecao.Id }, colecao);
        }

        /// <summary>
        /// atualiza as informacoes de uma colecao existente
        /// </summary>
        /// <param name="id">id da colecao a atualizar (deve corresponder ao id no corpo)</param>
        /// <param name="colecao">dados atualizados da colecao</param>
        /// <returns>sem conteudo em caso de sucesso</returns>
        /// <response code="204">colecao atualizada com sucesso</response>
        /// <response code="400">incompatibilidade de ids ou dados invalidos</response>
        /// <response code="404">a colecao solicitada nao existe</response>
        [HttpPut("{id}")] // metodo http put com parametro id na url
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutColecao(int id, Colecao colecao)
        {
            // verifica se o id da url corresponde ao id do objeto
            if (id != colecao.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // procura a colecao existente pelo id
            var colecaoExistente = await _context.Colecoes.FindAsync(id);
            // se nao existir, retorna 404
            if (colecaoExistente == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            // obtem o id do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // verifica se o utilizador e admin
            var isAdmin = User.IsInRole("Admin");

            // verifica permissao (dono ou admin)
            if (colecaoExistente.UtilizadorId != userId && !isAdmin)
                return Forbid(); // retorna 403

            // valida se ja existe outra colecao com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == colecaoExistente.UtilizadorId &&
                               c.Id != id &&
                               c.Nome.ToLower() == colecao.Nome.ToLower());

            // se existir, retorna erro
            if (colecaoDuplicada)
                return BadRequest(new { mensagem = $"Já existe uma coleção com o nome \"{colecao.Nome}\"!" });

            // atualiza os campos editaveis
            colecaoExistente.Nome = colecao.Nome;
            colecaoExistente.Descricao = colecao.Descricao;

            try
            {
                await _context.SaveChangesAsync(); // guarda as alteracoes
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se a colecao ainda existe
                if (!await _context.Colecoes.AnyAsync(c => c.Id == id))
                    return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });
                throw; // relanca a excecao se nao for um problema de concorrencia
            }

            return NoContent(); // retorna status 204 (sem conteudo)
        }

        /// <summary>
        /// remove definitivamente uma colecao do sistema e desassocia todos os albuns vinculados a ela
        /// </summary>
        /// <param name="id">id da colecao a remover</param>
        /// <returns>sem conteudo em caso de sucesso</returns>
        /// <response code="204">colecao removida com sucesso</response>
        /// <response code="404">colecao nao encontrada</response>
        [HttpDelete("{id}")] // metodo http delete com parametro id na url
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteColecao(int id)
        {
            // procura a colecao com as relacoes
            var colecao = await _context.Colecoes
                .Include(c => c.AlbumColecoes) // inclui a relacao com os albuns
                .FirstOrDefaultAsync(c => c.Id == id);

            // se a colecao nao existir, retorna 404
            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            // obtem o id do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // verifica se o utilizador e admin
            var isAdmin = User.IsInRole("Admin");

            // verifica permissao (dono ou admin)
            if (colecao.UtilizadorId != userId && !isAdmin)
                return Forbid(); // retorna 403

            // remove os vinculos na tabela muitos-para-muitos primeiro para evitar conflitos de fk
            if (colecao.AlbumColecoes != null)
                _context.AlbumColecoes.RemoveRange(colecao.AlbumColecoes);

            // remove a colecao
            _context.Colecoes.Remove(colecao);
            await _context.SaveChangesAsync(); // guarda as alteracoes

            return NoContent(); // retorna status 204 (sem conteudo)
        }

        /// <summary>
        /// adiciona um album a uma colecao (cria associacao muitos-para-muitos)
        /// </summary>
        /// <param name="id">id da colecao de destino</param>
        /// <param name="albumId">id do album a associar</param>
        /// <returns>objeto json indicando o sucesso da operacao</returns>
        /// <response code="200">album associado com sucesso</response>
        /// <response code="400">o album ja existe na colecao</response>
        /// <response code="404">a colecao ou o album nao existem</response>
        [HttpPost("{id}/albuns/{albumId}")] // rota: api/colecoes/{id}/albuns/{albumId}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAlbumToColecao(int id, int albumId)
        {
            // procura a colecao pelo id
            var colecao = await _context.Colecoes.FindAsync(id);
            // se a colecao nao existir, retorna 404
            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            // obtem o id do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // verifica se o utilizador e admin
            var isAdmin = User.IsInRole("Admin");

            // verifica permissao (dono ou admin)
            if (colecao.UtilizadorId != userId && !isAdmin)
                return Forbid(); // retorna 403

            // procura o album pelo id
            var album = await _context.Albuns.FindAsync(albumId);
            // se o album nao existir, retorna 404
            if (album == null)
                return NotFound(new { mensagem = $"Álbum com ID {albumId} não encontrado." });

            // verifica se o album ja esta na colecao
            var jaExiste = await _context.AlbumColecoes
                .AnyAsync(ac => ac.ColecaoId == id && ac.AlbumId == albumId);

            // se ja existir, retorna erro
            if (jaExiste)
                return BadRequest(new { mensagem = "Este álbum já está na coleção." });

            // cria a associacao
            _context.AlbumColecoes.Add(new AlbumColecao
            {
                ColecaoId = id,
                AlbumId = albumId,
                DataAdicao = DateTime.Now // define a data de adicao como agora
            });

            await _context.SaveChangesAsync(); // guarda as alteracoes
            return Ok(new { mensagem = "Álbum adicionado à coleção com sucesso." }); // retorna sucesso
        }

        /// <summary>
        /// remove um album associado a uma colecao (desassocia a relacao muitos-para-muitos)
        /// </summary>
        /// <param name="id">id da colecao</param>
        /// <param name="albumId">id do album a desassociar</param>
        /// <returns>mensagem de confirmacao da remocao</returns>
        /// <response code="200">album removido da colecao com sucesso</response>
        /// <response code="404">associacao entre o album e a colecao nao encontrada</response>
        [HttpDelete("{id}/albuns/{albumId}")] // rota: api/colecoes/{id}/albuns/{albumId}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveAlbumFromColecao(int id, int albumId)
        {
            // procura a colecao pelo id
            var colecao = await _context.Colecoes.FindAsync(id);
            // se a colecao nao existir, retorna 404
            if (colecao == null)
                return NotFound(new { mensagem = $"Coleção com ID {id} não encontrada." });

            // obtem o id do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // verifica se o utilizador e admin
            var isAdmin = User.IsInRole("Admin");

            // verifica permissao (dono ou admin)
            if (colecao.UtilizadorId != userId && !isAdmin)
                return Forbid(); // retorna 403

            // procura a associacao entre o album e a colecao
            var albumColecao = await _context.AlbumColecoes
                .FirstOrDefaultAsync(ac => ac.ColecaoId == id && ac.AlbumId == albumId);

            // se a associacao nao existir, retorna 404
            if (albumColecao == null)
                return NotFound(new { mensagem = "Álbum não encontrado nesta coleção." });

            // remove a associacao
            _context.AlbumColecoes.Remove(albumColecao);
            await _context.SaveChangesAsync(); // guarda as alteracoes

            return Ok(new { mensagem = "Álbum removido da coleção com sucesso." }); // retorna sucesso
        }
    }
}