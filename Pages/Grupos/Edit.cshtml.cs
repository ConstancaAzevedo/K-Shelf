using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace K_Shelf.Pages.Grupos
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
        public Grupo Grupo { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do grupo não fornecido.";
                return NotFound();
            }

            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
            {
                TempData["ErrorMessage"] = "Grupo não encontrado.";
                return NotFound();
            }

            Grupo = grupo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var grupoOriginal = await _context.Grupos.FindAsync(Grupo.Id);
                if (grupoOriginal == null)
                {
                    TempData["ErrorMessage"] = "Grupo não encontrado.";
                    return NotFound();
                }

                grupoOriginal.Nome = Grupo.Nome;
                grupoOriginal.DataEstreia = Grupo.DataEstreia;
                grupoOriginal.Companhia = Grupo.Companhia;
                grupoOriginal.Fansigno = Grupo.Fansigno;
                grupoOriginal.ImagemUrl = Grupo.ImagemUrl;
                grupoOriginal.IsAtivo = Grupo.IsAtivo;

                await _context.SaveChangesAsync();

                // Notificação SignalR
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Grupo",
                    Acao = "Editado",
                    Mensagem = $"✏️ Grupo '{Grupo.Nome}' foi atualizado!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"✅ Grupo \"{Grupo.Nome}\" atualizado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Erro ao atualizar grupo: {ex.Message}";
                return Page();
            }
        }
    }
}