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
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        public SelectList ArtistasSelectList { get; set; } = default!;
        public SelectList AlbunsSelectList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            Photocard = new Photocard();
            await CarregarSelectLists();
            return Page();
        }

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
