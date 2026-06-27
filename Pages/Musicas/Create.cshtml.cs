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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext;

        public CreateModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        
       }


        [BindProperty]
        public IFormFile? AudioFile { get; set; }

        [BindProperty]
        public Musica Musica { get; set; } = new();

        public SelectList AlbunsSelectList { get; set; } = default!;

        [BindProperty]
        public string DuracaoInput { get; set; } = string.Empty;

        public async Task OnGetAsync(int? albumId)
        {
            await CarregarAlbunsSelectList();
            if (albumId.HasValue)
            {
                Musica.AlbumId = albumId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // converter a duração de string para TimeSpan
            if (!string.IsNullOrWhiteSpace(DuracaoInput))
            {
                if (TimeSpan.TryParseExact(DuracaoInput, @"m\:ss", null, out var duracao))
                {
                    Musica.Duracao = duracao;
                }
                else
                {
                    ModelState.AddModelError("DuracaoInput", "Formato inválido. Use mm:ss (ex: 3:45)");
                }
            }

            if (AudioFile != null && AudioFile.Length > 0)
            {
                // Pasta onde guardar os áudios
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audios");
                Directory.CreateDirectory(uploadsFolder);

                // Nome único para o ficheiro
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(AudioFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AudioFile.CopyToAsync(stream);
                }

                // Guardar o caminho na base de dados
                Musica.PreviewAudioUrl = $"/audios/{fileName}";
            }

            // Validações
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
                _context.Musicas.Add(Musica);
                await _context.SaveChangesAsync();

                // Notificação
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Música",
                    Acao = "Criada",
                    Mensagem = $"Nova música '{Musica.Titulo}' foi adicionada!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Música \"{Musica.Titulo}\" criada com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar música: {ex.Message}";
                await CarregarAlbunsSelectList();
                return Page();
            }
        }

        private async Task CarregarAlbunsSelectList()
        {
            var albuns = await _context.Albuns
                .OrderBy(a => a.Titulo)
                .ToListAsync();

            AlbunsSelectList = new SelectList(albuns, "Id", "Titulo");
        }
    }
}