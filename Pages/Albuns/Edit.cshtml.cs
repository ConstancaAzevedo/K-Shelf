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
    // restringe o acesso apenas a utilizadores com o role admin
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        // construtor que recebe os servicos por injecao de dependencias
        public EditModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // propriedade que recebe os dados do album por binding
        [BindProperty]
        public Album Album { get; set; } = default!;

        // listas para os dropdowns
        public SelectList GruposSelectList { get; set; } = default!; // lista de grupos
        public SelectList SolistasSelectList { get; set; } = default!; // lista de solistas
        public SelectList AlbunsSelectList { get; set; } = default!; // lista de musicas

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do álbum não fornecido";
                return NotFound(); // retorna erro 404
            }

            // procura o album pelo id com os dados relacionados
            var album = await _context.Albuns
                .Include(a => a.Grupo) // inclui o grupo associado
                .Include(a => a.Solista) // inclui o solista associado
                .FirstOrDefaultAsync(a => a.Id == id);

            // se o album nao existir, retorna erro
            if (album == null)
            {
                TempData["ErrorMessage"] = "Álbum não encontrado";
                return NotFound(); // retorna erro 404
            }

            // atribui o album a propriedade da pagina
            Album = album;
            // carrega as listas para os dropdowns
            await CarregarSelectLists();
            await CarregarMusicasSelectList();

            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync()
        {
            // validacoes manuais

            // titulo obrigatorio
            if (string.IsNullOrWhiteSpace(Album.Titulo))
            {
                ModelState.AddModelError("Album.Titulo", "O título do álbum é obrigatório.");
            }

            // titulo com maximo de 200 caracteres
            if (!string.IsNullOrWhiteSpace(Album.Titulo) && Album.Titulo.Length > 200)
            {
                ModelState.AddModelError("Album.Titulo", "O título não pode exceder 200 caracteres.");
            }

            // verifica se ja existe outro album com o mesmo titulo
            if (!string.IsNullOrWhiteSpace(Album.Titulo))
            {
                var albumExistente = await _context.Albuns
                    .AnyAsync(a => a.Titulo.ToLower() == Album.Titulo.ToLower() && a.Id != Album.Id);

                if (albumExistente)
                {
                    ModelState.AddModelError("Album.Titulo", $"Já existe um álbum com o título \"{Album.Titulo}\"!");
                }
            }

            // validacao: data de lancamento nao pode ser no futuro
            if (Album.DataLancamento > DateTime.Now)
            {
                ModelState.AddModelError("Album.DataLancamento", "A data de lançamento não pode ser no futuro.");
            }

            // validacao: url da capa (se fornecida)
            if (!string.IsNullOrWhiteSpace(Album.CapaUrl))
            {
                if (!Uri.IsWellFormedUriString(Album.CapaUrl, UriKind.Absolute))
                {
                    ModelState.AddModelError("Album.CapaUrl", "O URL da capa não é válido.");
                }
            }

            // validacao: tem de ter pelo menos um artista associado
            if (!Album.GrupoId.HasValue && !Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum tem de estar associado a um Grupo ou a um Solista.");
            }

            // nao pode ter os dois ao mesmo tempo
            if (Album.GrupoId.HasValue && Album.SolistaId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "O álbum só pode estar associado a um Grupo OU a um Solista, não a ambos.");
            }

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
            {
                // recarrega as listas para os dropdowns
                await CarregarSelectLists();
                return Page(); // volta para a pagina com os erros
            }

            try
            {
                // procura o album original pelo id
                var albumOriginal = await _context.Albuns.FindAsync(Album.Id);
                // se o album nao existir, retorna erro
                if (albumOriginal == null)
                {
                    TempData["ErrorMessage"] = "Álbum não encontrado";
                    return NotFound(); // retorna erro 404
                }

                // atualiza os campos editaveis do album
                albumOriginal.Titulo = Album.Titulo; // atualiza o titulo
                albumOriginal.DataLancamento = Album.DataLancamento; // atualiza a data de lancamento
                albumOriginal.CapaUrl = Album.CapaUrl; // atualiza a url da capa
                albumOriginal.Tipo = Album.Tipo; // atualiza o tipo
                albumOriginal.Edicao = Album.Edicao; // atualiza a edicao
                albumOriginal.GrupoId = Album.GrupoId; // atualiza o grupo associado
                albumOriginal.SolistaId = Album.SolistaId; // atualiza o solista associado

                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Álbum",
                    Acao = "Editado",
                    Mensagem = $"✏️ Álbum '{Album.Titulo}' foi atualizado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Álbum \"{Album.Titulo}\" atualizado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de albuns
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se o album ainda existe
                if (!await _context.Albuns.AnyAsync(a => a.Id == Album.Id))
                {
                    TempData["ErrorMessage"] = "Álbum não encontrado";
                    return NotFound(); // retorna erro 404
                }
                throw; // relanca a excecao
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao atualizar álbum: {ex.Message}";
                // recarrega as listas para os dropdowns
                await CarregarSelectLists();
                return Page(); // volta para a pagina com o erro
            }
        }

        // metodo auxiliar que carrega as listas de grupos e solistas para os dropdowns
        private async Task CarregarSelectLists()
        {
            // obtem todos os grupos ordenados por nome
            var grupos = await _context.Grupos.OrderBy(g => g.Nome).ToListAsync();
            // obtem todos os solistas ordenados por nome
            var solistas = await _context.Solistas.OrderBy(s => s.Nome).ToListAsync();

            // cria os selectlists para os dropdowns (valor = id, texto = nome)
            // o ultimo parametro seleciona o valor atual
            GruposSelectList = new SelectList(grupos, "Id", "Nome", Album.GrupoId);
            SolistasSelectList = new SelectList(solistas, "Id", "Nome", Album.SolistaId);
        }

        // metodo auxiliar que carrega a lista de musicas para o dropdown
        private async Task CarregarMusicasSelectList()
        {
            // obtem todas as musicas ordenadas por titulo
            var musicas = await _context.Musicas
                .OrderBy(m => m.Titulo)
                .ToListAsync();

            // cria o selectlist para as musicas (valor = id, texto = titulo)
            AlbunsSelectList = new SelectList(musicas, "Id", "Titulo");
        }
    }
}