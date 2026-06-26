using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Albuns
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext; // NOVO


        public CreateModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;

        }

        [BindProperty]
        public Album Album { get; set; } = new Album();

        public SelectList GruposSelectList { get; set; } = default!;
        public SelectList SolistasSelectList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await CarregarSelectLists();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            // Validações

            // Título obrigatório
            if (string.IsNullOrWhiteSpace(Album.Titulo))
            {
                ModelState.AddModelError("Album.Titulo", "O título do álbum é obrigatório.");
            }

            // Título com máximo de 200 caracteres
            if (!string.IsNullOrWhiteSpace(Album.Titulo) && Album.Titulo.Length > 200)
            {
                ModelState.AddModelError("Album.Titulo", "O título não pode exceder 200 caracteres.");
            }

            // Verificar se já existe um álbum com o mesmo título
            if (!string.IsNullOrWhiteSpace(Album.Titulo))
            {
                var albumExistente = await _context.Albuns
                    .AnyAsync(a => a.Titulo.ToLower() == Album.Titulo.ToLower());

                if (albumExistente)
                {
                    ModelState.AddModelError("Album.Titulo", $"Já existe um álbum com o título \"{Album.Titulo}\"!");
                }
            }

            // Validação: Data de Lançamento não pode ser no futuro
            if (Album.DataLancamento > DateTime.Now)
            {
                ModelState.AddModelError("Album.DataLancamento", "A data de lançamento não pode ser no futuro.");
            }

            // Validação: URL da capa (se fornecida)
            if (!string.IsNullOrWhiteSpace(Album.CapaUrl))
            {
                if (!Uri.IsWellFormedUriString(Album.CapaUrl, UriKind.Absolute))
                {
                    ModelState.AddModelError("Album.CapaUrl", "O URL da capa não é válido.");
                }
            }

            // Validação: tem de ter pelo menos um artista associado
            if (!Album.GrupoId.HasValue && !Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum tem de estar associado a um Grupo ou a um Solista.");
            }

            // Não pode ter os dois ao mesmo tempo
            if (Album.GrupoId.HasValue && Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum só pode estar associado a um Grupo OU a um Solista, não a ambos.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarSelectLists();
                return Page();
            }

            try
            {
                _context.Albuns.Add(Album);
                await _context.SaveChangesAsync();

                // notificação em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Álbum",
                    Acao = "Criado",
                    Mensagem = $"Novo álbum '{Album.Titulo}' foi adicionado!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Álbum \"{Album.Titulo}\" criado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar álbum: {ex.Message}";
                await CarregarSelectLists();
                return Page();
            }

        }

        private async Task CarregarSelectLists()
        {
            var grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .ToListAsync();

            var solistas = await _context.Solistas
                .OrderBy(s => s.Nome)
                .ToListAsync();

            GruposSelectList = new SelectList(grupos, "Id", "Nome");
            SolistasSelectList = new SelectList(solistas, "Id", "Nome");
        }
    }
}
