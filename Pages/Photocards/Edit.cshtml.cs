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
    /// Página de administração para editar os dados de um photocard existente no catálogo.
    /// Acesso restrito a utilizadores com o papel de Administrador.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext; // NOVO

        /// <summary>
        /// Construtor com injeção de dependência do contexto da base de dados.
        /// </summary>
        public EditModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>Dados do photocard a editar, carregados da BD e vinculados ao formulário.</summary>
        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        /// <summary>Lista de artistas disponíveis para o dropdown de seleção.</summary>
        public SelectList ArtistasSelectList { get; set; } = default!;

        /// <summary>Lista de álbuns disponíveis para o dropdown de seleção (opcional).</summary>
        public SelectList AlbunsSelectList { get; set; } = default!;

        /// <summary>
        /// Carrega o photocard para edição e inicializa as listas de seleção.
        /// </summary>
        /// <param name="id">ID do photocard a editar.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index");
            }

            var photocard = await _context.Photocards.FirstOrDefaultAsync(p => p.Id == id);
            if (photocard == null)
            {
                TempData["ErrorMessage"] = "Photocard não encontrado.";
                return RedirectToPage("./Index");
            }

            Photocard = photocard;
            await CarregarSelectLists();
            return Page();
        }

        /// <summary>
        /// Processa o envio do formulário de edição, valida e persiste as alterações na BD.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Validações
            if (string.IsNullOrWhiteSpace(Photocard.Versao))
            {
                ModelState.AddModelError("Photocard.Versao", "A versão do photocard é obrigatória.");
            }

            if (Photocard.ArtistaId <= 0)
            {
                ModelState.AddModelError("Photocard.ArtistaId", "Selecione um artista.");
            }

            // Validar URL da imagem
            if (!string.IsNullOrWhiteSpace(Photocard.ImagemUrl))
            {
                if (!Uri.IsWellFormedUriString(Photocard.ImagemUrl, UriKind.Absolute) && !Photocard.ImagemUrl.StartsWith("/"))
                {
                    ModelState.AddModelError("Photocard.ImagemUrl", "O URL da imagem não é válido.");
                }
            }
            else
            {
                Photocard.ImagemUrl = "/imagens/photocards/default.png";
            }

            if (!ModelState.IsValid)
            {
                await CarregarSelectLists();
                return Page();
            }

            _context.Attach(Photocard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // notificação em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Photocard",
                    Acao = "Editado",
                    Mensagem = $"Photocard '{Photocard.Versao}' foi atualizado!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Photocard \"{Photocard.Versao}\" editado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhotocardExists(Photocard.Id))
                {
                    TempData["ErrorMessage"] = "O photocard que tentou editar já não existe na base de dados.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao editar photocard: {ex.Message}";
                await CarregarSelectLists();
                return Page();
            }
        }

        /// <summary>
        /// Verifica se um photocard com o ID especificado ainda existe na base de dados.
        /// </summary>
        /// <param name="id">ID do photocard a verificar.</param>
        private bool PhotocardExists(int id)
        {
            return _context.Photocards.Any(e => e.Id == id);
        }

        /// <summary>
        /// Método auxiliar que carrega as listas de artistas e álbuns para os dropdowns do formulário.
        /// </summary>
        private async Task CarregarSelectLists()
        {
            var artistas = await _context.Artistas
                .OrderBy(a => a.NomeArtistico)
                .ToListAsync();

            ArtistasSelectList = new SelectList(artistas, "Id", "NomeArtistico");

            var albuns = await _context.Albuns
                .OrderBy(a => a.Titulo)
                .ToListAsync();

            AlbunsSelectList = new SelectList(albuns, "Id", "Titulo");
        }
    }
}