using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Solistas
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext;

        public EditModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Solista Solista { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do solista não fornecido.";
                return NotFound();
            }

            var solista = await _context.Solistas.FindAsync(id);
            if (solista == null)
            {
                TempData["ErrorMessage"] = "Solista não encontrado.";
                return NotFound();
            }

            Solista = solista;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var solistaOriginal = await _context.Solistas.FindAsync(Solista.Id);
                if (solistaOriginal == null)
                {
                    TempData["ErrorMessage"] = "Solista não encontrado.";
                    return NotFound();
                }

                solistaOriginal.Nome = Solista.Nome;
                solistaOriginal.DataEstreia = Solista.DataEstreia;
                solistaOriginal.Companhia = Solista.Companhia;
                solistaOriginal.ImagemUrl = Solista.ImagemUrl;
                solistaOriginal.IsAtivo = Solista.IsAtivo;

                await _context.SaveChangesAsync();

                // Notificação SignalR
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Solista",
                    Acao = "Editado",
                    Mensagem = $"✏️ Solista '{Solista.Nome}' foi atualizado!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"✅ Solista \"{Solista.Nome}\" atualizado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Erro ao atualizar solista: {ex.Message}";
                return Page();
            }
        }
    }
}