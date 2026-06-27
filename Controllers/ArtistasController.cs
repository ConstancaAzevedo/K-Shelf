using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Controllers
{
    /// <summary>
    /// api para gestao de artistas k-pop
    /// </summary>
    [Route("api/[controller]")] // define a rota base como api/artistas
    [ApiController] // indica que este controlador e uma api
    public class ArtistasController : ControllerBase
    {
        // contexto da base de dados
        private readonly ApplicationDbContext _context;

        // construtor que recebe o contexto por injecao de dependencias
        public ArtistasController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// obtem todos os artistas
        /// </summary>
        [HttpGet] // metodo http get
        [AllowAnonymous] // permite acesso sem autenticacao
        public async Task<ActionResult<IEnumerable<object>>> GetArtistas()
        {
            // obtem todos os artistas com os dados relacionados
            var artistas = await _context.Artistas
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .Select(a => new // projeta os dados para um objeto anonimo
                {
                    a.Id,
                    a.Nome,
                    a.NomeArtistico,
                    a.Posicao,
                    a.Pais,
                    a.ImagemUrl,
                    a.IsAtivo,
                    DataNascimento = a.DataNascimento.ToString("yyyy-MM-dd"), // formata a data
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null, // dados do grupo ou null
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null // dados do solista ou null
                })
                .ToListAsync();

            return Ok(artistas); // retorna a lista com status 200
        }

        /// <summary>
        /// obtem um artista por id
        /// </summary>
        [HttpGet("{id}")] // metodo http get com parametro id na url
        [AllowAnonymous] // permite acesso sem autenticacao
        public async Task<ActionResult<object>> GetArtista(int id)
        {
            // procura o artista pelo id com os dados relacionados
            var artista = await _context.Artistas
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .Where(a => a.Id == id) // filtra pelo id
                .Select(a => new // projeta os dados para um objeto anonimo
                {
                    a.Id,
                    a.Nome,
                    a.NomeArtistico,
                    a.Posicao,
                    a.Pais,
                    a.ImagemUrl,
                    a.IsAtivo,
                    DataNascimento = a.DataNascimento.ToString("yyyy-MM-dd"), // formata a data de nascimento
                    DataEntrada = a.DataEntrada.HasValue ? a.DataEntrada.Value.ToString("yyyy-MM-dd") : null, // formata a data de entrada ou null
                    DataSaida = a.DataSaida.HasValue ? a.DataSaida.Value.ToString("yyyy-MM-dd") : null, // formata a data de saida ou null
                    Grupo = a.Grupo != null ? new { a.Grupo.Id, a.Grupo.Nome } : null, // dados do grupo ou null
                    Solista = a.Solista != null ? new { a.Solista.Id, a.Solista.Nome } : null // dados do solista ou null
                })
                .FirstOrDefaultAsync();

            // se o artista nao existir, retorna 404
            if (artista == null)
                return NotFound(new { mensagem = $"Artista com ID {id} não encontrado." });

            return Ok(artista); // retorna o artista com status 200
        }

        /// <summary>
        /// cria um novo artista (apenas admin)
        /// </summary>
        [HttpPost] // metodo http post
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<ActionResult<Artista>> PostArtista(Artista artista)
        {
            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // adiciona o artista ao contexto
            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync(); // guarda na base de dados

            // retorna o artista criado com status 201 e a localizacao
            return CreatedAtAction(nameof(GetArtista), new { id = artista.Id }, artista);
        }

        /// <summary>
        /// atualiza um artista existente (apenas admin)
        /// </summary>
        [HttpPut("{id}")] // metodo http put com parametro id na url
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<IActionResult> PutArtista(int id, Artista artista)
        {
            // verifica se o id da url corresponde ao id do objeto
            if (id != artista.Id)
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do corpo do pedido." });

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // retorna os erros de validacao

            // marca o objeto como modificado
            _context.Entry(artista).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // guarda as alteracoes
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se o artista ainda existe
                if (!await _context.Artistas.AnyAsync(a => a.Id == id))
                    return NotFound(new { mensagem = $"Artista com ID {id} não encontrado." });
                throw; // relanca a excecao se nao for um problema de concorrencia
            }

            return NoContent(); // retorna status 204 (sem conteudo)
        }

        /// <summary>
        /// elimina um artista (apenas admin)
        /// </summary>
        [HttpDelete("{id}")] // metodo http delete com parametro id na url
        [Authorize(Roles = "Admin")] // apenas admins podem aceder
        public async Task<IActionResult> DeleteArtista(int id)
        {
            // procura o artista pelo id
            var artista = await _context.Artistas.FindAsync(id);
            // se o artista nao existir, retorna 404
            if (artista == null)
                return NotFound(new { mensagem = $"Artista com ID {id} não encontrado." });

            // remove o artista do contexto
            _context.Artistas.Remove(artista);
            await _context.SaveChangesAsync(); // guarda as alteracoes

            return NoContent(); // retorna status 204 (sem conteudo)
        }
    }
}