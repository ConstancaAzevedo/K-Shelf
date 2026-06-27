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
    public class CreateModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        // construtor que recebe os servicos por injecao de dependencias
        public CreateModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // propriedade que recebe os dados do album por binding
        [BindProperty]
        public Album Album { get; set; } = new Album();

        // listas para os dropdowns de grupos e solistas
        public SelectList GruposSelectList { get; set; } = default!;
        public SelectList SolistasSelectList { get; set; } = default!;

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync()
        {
            // carrega as listas para os dropdowns
            await CarregarSelectLists();
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

            // verifica se ja existe um album com o mesmo titulo
            if (!string.IsNullOrWhiteSpace(Album.Titulo))
            {
                var albumExistente = await _context.Albuns
                    .AnyAsync(a => a.Titulo.ToLower() == Album.Titulo.ToLower());

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
                // adiciona o album ao contexto
                _context.Albuns.Add(Album);
                await _context.SaveChangesAsync(); // guarda na base de dados

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Álbum",
                    Acao = "Criado",
                    Mensagem = $"Novo álbum '{Album.Titulo}' foi adicionado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Álbum \"{Album.Titulo}\" criado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de albuns
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao criar álbum: {ex.Message}";
                // recarrega as listas para os dropdowns
                await CarregarSelectLists();
                return Page(); // volta para a pagina com o erro
            }
        }

        // metodo auxiliar que carrega as listas de grupos e solistas para os dropdowns
        private async Task CarregarSelectLists()
        {
            // obtem todos os grupos ordenados por nome
            var grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .ToListAsync();

            // obtem todos os solistas ordenados por nome
            var solistas = await _context.Solistas
                .OrderBy(s => s.Nome)
                .ToListAsync();

            // cria os selectlists para os dropdowns (valor = id, texto = nome)
            GruposSelectList = new SelectList(grupos, "Id", "Nome");
            SolistasSelectList = new SelectList(solistas, "Id", "Nome");
        }
    }
}