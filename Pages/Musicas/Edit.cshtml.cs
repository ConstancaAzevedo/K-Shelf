using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Musicas
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
        public Musica Musica { get; set; } = new();

        public SelectList AlbunsSelectList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da música não fornecido.";
                return NotFound();
            }

            var musica = await _context.Musicas.FirstOrDefaultAsync(m => m.Id == id);
            if (musica == null)
            {
                TempData["ErrorMessage"] = "Música não encontrada.";
                return NotFound();
            }

            Musica = musica;
            await CarregarAlbunsSelectList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validações (iguais ao Create)
            if (string.IsNullOrWhiteSpace(Musica.Titulo))
            {
                ModelState.AddModelError("Musica.Titulo", "O título da música é obrigatório.");
            }

            if (Musica.TrackNumber <= 0)
            {
                ModelState.AddModelError("Musica.TrackNumber", "O número da faixa deve ser maior que 0.");
            }

            if (Musica.AlbumId <= 0)
            {
                ModelState.AddModelError("Musica.AlbumId", "Selecione um álbum.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarAlbunsSelectList();
                return Page();
            }

            try
            {
                var musicaOriginal = await _context.Musicas.FindAsync(Musica.Id);
                if (musicaOriginal == null)
                {
                    TempData["ErrorMessage"] = "Música não encontrada.";
                    return NotFound();
                }

                // Atualizar campos
                musicaOriginal.Titulo = Musica.Titulo;
                musicaOriginal.Duracao = Musica.Duracao;
                musicaOriginal.TrackNumber = Musica.TrackNumber;
                musicaOriginal.AlbumId = Musica.AlbumId;
                musicaOriginal.IsTitleTrack = Musica.IsTitleTrack;
                musicaOriginal.IsSingle = Musica.IsSingle;
                musicaOriginal.Compositores = Musica.Compositores;
                musicaOriginal.Produtores = Musica.Produtores;
                musicaOriginal.Letra = Musica.Letra;
                musicaOriginal.YoutubeUrl = Musica.YoutubeUrl;
                musicaOriginal.PreviewAudioUrl = Musica.PreviewAudioUrl;

                await _context.SaveChangesAsync();

                // Notificação
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Música",
                    Acao = "Editada",
                    Mensagem = $"✏️ Música '{Musica.Titulo}' foi atualizada!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Música \"{Musica.Titulo}\" atualizada com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao atualizar música: {ex.Message}";
                await CarregarAlbunsSelectList();
                return Page();
            }
        }

        private async Task CarregarAlbunsSelectList()
        {
            var albuns = await _context.Albuns
                .OrderBy(a => a.Titulo)
                .ToListAsync();

            AlbunsSelectList = new SelectList(albuns, "Id", "Titulo", Musica.AlbumId);
        }
    }
}