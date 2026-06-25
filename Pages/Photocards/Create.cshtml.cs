using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Photocards
{
    /// <summary>
    /// Página de administração para criar e registar um novo photocard no catálogo.
    /// Acesso restrito a utilizadores com o papel de Administrador.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor com injeção de dependência do contexto da base de dados.
        /// </summary>
        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Dados do photocard a criar, preenchidos a partir do formulário.</summary>
        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        /// <summary>Lista de artistas disponíveis para associar ao photocard.</summary>
        public SelectList ArtistasSelectList { get; set; } = default!;

        /// <summary>Lista de álbuns disponíveis para associar ao photocard (opcional).</summary>
        public SelectList AlbunsSelectList { get; set; } = default!;

        /// <summary>
        /// Inicializa o formulário de criação carregando as listas de seleção.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            Photocard = new Photocard();
            await CarregarSelectLists();
            return Page();
        }

        /// <summary>
        /// Processa o envio do formulário, valida os dados e regista o novo photocard.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Validações adicionais
            if (string.IsNullOrWhiteSpace(Photocard.Versao))
            {
                ModelState.AddModelError("Photocard.Versao", "A versão do photocard é obrigatória.");
            }

            if (Photocard.ArtistaId <= 0)
            {
                ModelState.AddModelError("Photocard.ArtistaId", "Selecione um artista.");
            }

            // Validar URL da imagem (opcional, senão usa placeholder padrão)
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

            try
            {
                _context.Photocards.Add(Photocard);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Photocard \"{Photocard.Versao}\" adicionado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar photocard: {ex.Message}";
                await CarregarSelectLists();
                return Page();
            }
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
