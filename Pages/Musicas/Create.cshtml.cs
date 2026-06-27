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

        // propriedade para receber o ficheiro de audio do formulario
        [BindProperty]
        public IFormFile? AudioFile { get; set; }

        // dados da musica recebidos do formulario
        [BindProperty]
        public Musica Musica { get; set; } = new();

        // lista de albuns para o dropdown
        public SelectList AlbunsSelectList { get; set; } = default!;

        // campo auxiliar para receber a duracao como string no formato mm:ss
        [BindProperty]
        public string DuracaoInput { get; set; } = string.Empty;

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync(int? albumId)
        {
            // carrega a lista de albuns para o dropdown
            await CarregarAlbunsSelectList();
            // se for fornecido um album id, preenche automaticamente
            if (albumId.HasValue)
            {
                Musica.AlbumId = albumId.Value;
            }
        }

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync()
        {
            // converte a duracao de string para timespan
            if (!string.IsNullOrWhiteSpace(DuracaoInput))
            {
                // tenta fazer parse no formato mm:ss
                if (TimeSpan.TryParseExact(DuracaoInput, @"m\:ss", null, out var duracao))
                {
                    Musica.Duracao = duracao; // atribui o valor convertido
                }
                else
                {
                    // se o formato for invalido, adiciona erro ao modelo
                    ModelState.AddModelError("DuracaoInput", "Formato inválido. Use mm:ss (ex: 3:45)");
                }
            }

            // processa o upload do ficheiro de audio se existir
            if (AudioFile != null && AudioFile.Length > 0)
            {
                // define a pasta onde guardar os audios
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audios");
                // cria a pasta se nao existir
                Directory.CreateDirectory(uploadsFolder);

                // gera um nome unico para o ficheiro
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(AudioFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // guarda o ficheiro no sistema de ficheiros
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AudioFile.CopyToAsync(stream);
                }

                // guarda o caminho do ficheiro na base de dados
                Musica.PreviewAudioUrl = $"/audios/{fileName}";
            }

            // validacoes manuais
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

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
            {
                // recarrega a lista de albuns para o dropdown
                await CarregarAlbunsSelectList();
                return Page(); // volta para a pagina com os erros
            }

            try
            {
                // adiciona a musica ao contexto
                _context.Musicas.Add(Musica);
                await _context.SaveChangesAsync(); // guarda na base de dados

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Música",
                    Acao = "Criada",
                    Mensagem = $"Nova música '{Musica.Titulo}' foi adicionada!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Música \"{Musica.Titulo}\" criada com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de musicas
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao criar música: {ex.Message}";
                // recarrega a lista de albuns para o dropdown
                await CarregarAlbunsSelectList();
                return Page(); // volta para a pagina com o erro
            }
        }

        // metodo auxiliar que carrega a lista de albuns para o dropdown
        private async Task CarregarAlbunsSelectList()
        {
            // obtem todos os albuns ordenados por titulo
            var albuns = await _context.Albuns
                .OrderBy(a => a.Titulo)
                .ToListAsync();

            // cria o selectlist para os albuns (valor = id, texto = titulo)
            AlbunsSelectList = new SelectList(albuns, "Id", "Titulo");
        }
    }
}