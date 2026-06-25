using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using System;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Photocards
{
    /// <summary>
    /// Página de confirmação e execução da eliminação de um photocard do catálogo.
    /// Acesso restrito a utilizadores com o papel de Administrador.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor com injeção de dependência do contexto da base de dados.
        /// </summary>
        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>O photocard a ser apresentado para confirmação de eliminação.</summary>
        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        /// <summary>
        /// Carrega os detalhes do photocard a eliminar para confirmação do administrador.
        /// </summary>
        /// <param name="id">ID do photocard a eliminar.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Validação do parâmetro obrigatório
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index");
            }

            // Carrega o photocard com os dados relacionados para exibir ao utilizador
            var photocard = await _context.Photocards
                .Include(p => p.Artista)
                .Include(p => p.Album)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photocard == null)
            {
                TempData["ErrorMessage"] = "Photocard não encontrado.";
                return RedirectToPage("./Index");
            }

            Photocard = photocard;
            return Page();
        }

        /// <summary>
        /// Executa a eliminação definitiva do photocard da base de dados após confirmação.
        /// </summary>
        /// <param name="id">ID do photocard a eliminar.</param>
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index");
            }

            var photocard = await _context.Photocards.FindAsync(id);

            if (photocard != null)
            {
                try
                {
                    // Remove o photocard e persiste a alteração na base de dados
                    _context.Photocards.Remove(photocard);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Photocard \"{photocard.Versao}\" eliminado com sucesso!";
                }
                catch (Exception ex)
                {
                    // Captura erros de integridade referencial ou de acesso à BD
                    TempData["ErrorMessage"] = $"Erro ao eliminar photocard: {ex.Message}";
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
