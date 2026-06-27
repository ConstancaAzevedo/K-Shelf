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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext; // NOVO

        public EditModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Album Album { get; set; } = default!;

        public SelectList GruposSelectList { get; set; } = default!;
        public SelectList SolistasSelectList { get; set; } = default!;
        public SelectList AlbunsSelectList { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do álbum não fornecido";
                return NotFound();
            }

            var album = await _context.Albuns
                .Include(a => a.Grupo)
                .Include(a => a.Solista)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return NotFound();
            }

            Album = album;
            await CarregarSelectLists();
            await CarregarMusicasSelectList();

            return Page();
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

            // Verificar se já existe outro álbum com o mesmo título
            if (!string.IsNullOrWhiteSpace(Album.Titulo))
            {
                var albumExistente = await _context.Albuns
                    .AnyAsync(a => a.Titulo.ToLower() == Album.Titulo.ToLower() && a.Id != Album.Id);

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
                var albumOriginal = await _context.Albuns.FindAsync(Album.Id);
                if (albumOriginal == null)
                {
                    TempData["ErrorMessage"] = "Álbum não encontrado";
                    return NotFound();
                }

                // Atualizar campos
                albumOriginal.Titulo = Album.Titulo;
                albumOriginal.DataLancamento = Album.DataLancamento;
                albumOriginal.CapaUrl = Album.CapaUrl;
                albumOriginal.Tipo = Album.Tipo;
                albumOriginal.Edicao = Album.Edicao;
                albumOriginal.GrupoId = Album.GrupoId;
                albumOriginal.SolistaId = Album.SolistaId;

                await _context.SaveChangesAsync();

                // notificação em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Álbum",
                    Acao = "Editado",
                    Mensagem = $"✏️ Álbum '{Album.Titulo}' foi atualizado!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Álbum \"{Album.Titulo}\" atualizado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Albuns.AnyAsync(a => a.Id == Album.Id))
                {
                    TempData["ErrorMessage"] = "Álbum não encontrado";
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao atualizar álbum: {ex.Message}";
                await CarregarSelectLists();
                return Page();
            }
        }

        private async Task CarregarSelectLists()
        {
            var grupos = await _context.Grupos.OrderBy(g => g.Nome).ToListAsync();
            var solistas = await _context.Solistas.OrderBy(s => s.Nome).ToListAsync();

            GruposSelectList = new SelectList(grupos, "Id", "Nome", Album.GrupoId);
            SolistasSelectList = new SelectList(solistas, "Id", "Nome", Album.SolistaId);
        }

        private async Task CarregarMusicasSelectList()
        {
            var musicas = await _context.Musicas
                .OrderBy(m => m.Titulo)
                .ToListAsync();

            AlbunsSelectList = new SelectList(musicas, "Id", "Titulo");
        }
    }
}