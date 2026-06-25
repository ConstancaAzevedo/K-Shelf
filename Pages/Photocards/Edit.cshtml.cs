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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        public SelectList ArtistasSelectList { get; set; } = default!;
        public SelectList AlbunsSelectList { get; set; } = default!;

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

        private bool PhotocardExists(int id)
        {
            return _context.Photocards.Any(e => e.Id == id);
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
