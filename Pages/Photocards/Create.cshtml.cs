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
    /// pagina de administracao para criar e registar um novo photocard no catalogo
    /// acesso restrito a utilizadores com o papel de administrador
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        /// <summary>
        /// construtor com injecao de dependencia do contexto da base de dados
        /// </summary>
        public CreateModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>dados do photocard a criar, preenchidos a partir do formulario</summary>
        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        /// <summary>lista de artistas disponiveis para associar ao photocard</summary>
        public SelectList ArtistasSelectList { get; set; } = default!;

        /// <summary>lista de albuns disponiveis para associar ao photocard (opcional)</summary>
        public SelectList AlbunsSelectList { get; set; } = default!;

        /// <summary>
        /// inicializa o formulario de criacao carregando as listas de selecao
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // inicializa um novo photocard vazio
            Photocard = new Photocard();
            // carrega as listas para os dropdowns
            await CarregarSelectLists();
            return Page(); // retorna a pagina
        }

        /// <summary>
        /// processa o envio do formulario, valida os dados e regista o novo photocard
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

            // valida a url da imagem (opcional, senao usa placeholder padrao)
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

            try
            {
                // adiciona o photocard ao contexto
                _context.Photocards.Add(Photocard);
                await _context.SaveChangesAsync(); // guarda na base de dados

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Photocard",
                    Acao = "Criado",
                    Mensagem = $"Novo photocard '{Photocard.Versao}' foi adicionado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Photocard \"{Photocard.Versao}\" adicionado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de photocards
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao criar photocard: {ex.Message}";
                // recarrega as listas para os dropdowns
                await CarregarSelectLists();
                return Page(); // volta para a pagina com o erro
            }
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