using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Photocards
{
    /// <summary>
    /// pagina de administracao para editar os dados de um photocard existente no catalogo
    /// acesso restrito a utilizadores com o papel de administrador
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        /// <summary>
        /// construtor com injecao de dependencia do contexto da base de dados
        /// </summary>
        public EditModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>dados do photocard a editar, carregados da bd e vinculados ao formulario</summary>
        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        /// <summary>lista de artistas disponiveis para o dropdown de selecao</summary>
        public SelectList ArtistasSelectList { get; set; } = default!;

        /// <summary>lista de albuns disponiveis para o dropdown de selecao (opcional)</summary>
        public SelectList AlbunsSelectList { get; set; } = default!;

        /// <summary>
        /// carrega o photocard para edicao e inicializa as listas de selecao
        /// </summary>
        /// <param name="id">id do photocard a editar</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // valida se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index"); // redireciona para a lista
            }

            // procura o photocard pelo id
            var photocard = await _context.Photocards.FirstOrDefaultAsync(p => p.Id == id);
            // se o photocard nao existir, redireciona para a lista com erro
            if (photocard == null)
            {
                TempData["ErrorMessage"] = "Photocard não encontrado.";
                return RedirectToPage("./Index");
            }

            // atribui o photocard a propriedade da pagina
            Photocard = photocard;
            // carrega as listas para os dropdowns
            await CarregarSelectLists();
            return Page(); // retorna a pagina
        }

        /// <summary>
        /// processa o envio do formulario de edicao, valida e persiste as alteracoes na bd
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // validacoes adicionais
            if (string.IsNullOrWhiteSpace(Photocard.Versao))
            {
                ModelState.AddModelError("Photocard.Versao", "A versão do photocard é obrigatória.");
            }

            if (Photocard.ArtistaId <= 0)
            {
                ModelState.AddModelError("Photocard.ArtistaId", "Selecione um artista.");
            }

            // valida a url da imagem
            if (!string.IsNullOrWhiteSpace(Photocard.ImagemUrl))
            {
                // verifica se a url e valida (absoluta ou local)
                if (!Uri.IsWellFormedUriString(Photocard.ImagemUrl, UriKind.Absolute) && !Photocard.ImagemUrl.StartsWith("/"))
                {
                    ModelState.AddModelError("Photocard.ImagemUrl", "O URL da imagem não é válido.");
                }
            }
            else
            {
                // se nao for fornecida imagem, usa o placeholder padrao
                Photocard.ImagemUrl = "/imagens/photocards/default.png";
            }

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
            {
                // recarrega as listas para os dropdowns
                await CarregarSelectLists();
                return Page(); // volta para a pagina com os erros
            }

            // marca o photocard como modificado
            _context.Attach(Photocard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Photocard",
                    Acao = "Editado",
                    Mensagem = $"Photocard '{Photocard.Versao}' foi atualizado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Photocard \"{Photocard.Versao}\" editado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se o photocard ainda existe
                if (!PhotocardExists(Photocard.Id))
                {
                    // se nao existir, mostra erro
                    TempData["ErrorMessage"] = "O photocard que tentou editar já não existe na base de dados.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    throw; // relanca a excecao
                }
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao editar photocard: {ex.Message}";
                // recarrega as listas para os dropdowns
                await CarregarSelectLists();
                return Page(); // volta para a pagina com o erro
            }
        }

        /// <summary>
        /// verifica se um photocard com o id especificado ainda existe na base de dados
        /// </summary>
        /// <param name="id">id do photocard a verificar</param>
        private bool PhotocardExists(int id)
        {
            // verifica se existe algum photocard com o id fornecido
            return _context.Photocards.Any(e => e.Id == id);
        }

        /// <summary>
        /// metodo auxiliar que carrega as listas de artistas e albuns para os dropdowns do formulario
        /// </summary>
        private async Task CarregarSelectLists()
        {
            // obtem a lista de artistas ordenada por nome artistico
            var artistas = await _context.Artistas
                .OrderBy(a => a.NomeArtistico)
                .ToListAsync();

            // cria o selectlist para os artistas (valor = id, texto = nome artistico)
            ArtistasSelectList = new SelectList(artistas, "Id", "NomeArtistico");

            // obtem a lista de albuns ordenada por titulo
            var albuns = await _context.Albuns
                .OrderBy(a => a.Titulo)
                .ToListAsync();

            // cria o selectlist para os albuns (valor = id, texto = titulo)
            AlbunsSelectList = new SelectList(albuns, "Id", "Titulo");
        }
    }
}