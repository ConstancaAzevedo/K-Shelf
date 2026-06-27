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
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da música não fornecido.";
                return NotFound(); // retorna erro 404
            }

            // procura a musica pelo id
            var musica = await _context.Musicas.FirstOrDefaultAsync(m => m.Id == id);
            // se a musica nao existir, retorna erro
            if (musica == null)
            {
                TempData["ErrorMessage"] = "Música não encontrada.";
                return NotFound(); // retorna erro 404
            }

            // atribui a musica a propriedade da pagina
            Musica = musica;

            // preenche o campo duracaoinput com o valor atual no formato mm:ss
            if (Musica.Duracao.HasValue)
            {
                DuracaoInput = Musica.Duracao.Value.ToString(@"m\:ss");
            }

            // carrega a lista de albuns para o dropdown
            await CarregarAlbunsSelectList();
            return Page(); // retorna a pagina
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

            // validacoes manuais (iguais ao create)
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
                // procura a musica original pelo id
                var musicaOriginal = await _context.Musicas.FindAsync(Musica.Id);
                // se a musica nao existir, retorna erro
                if (musicaOriginal == null)
                {
                    TempData["ErrorMessage"] = "Música não encontrada.";
                    return NotFound(); // retorna erro 404
                }

                // atualiza os campos editaveis da musica
                musicaOriginal.Titulo = Musica.Titulo; // atualiza o titulo
                musicaOriginal.Duracao = Musica.Duracao; // atualiza a duracao
                musicaOriginal.TrackNumber = Musica.TrackNumber; // atualiza o numero da faixa
                musicaOriginal.AlbumId = Musica.AlbumId; // atualiza o album
                musicaOriginal.IsTitleTrack = Musica.IsTitleTrack; // atualiza se e title track
                musicaOriginal.IsSingle = Musica.IsSingle; // atualiza se e single
                musicaOriginal.Compositores = Musica.Compositores; // atualiza os compositores
                musicaOriginal.Produtores = Musica.Produtores; // atualiza os produtores
                musicaOriginal.Letra = Musica.Letra; // atualiza a letra
                musicaOriginal.YoutubeUrl = Musica.YoutubeUrl; // atualiza o youtube url
                musicaOriginal.PreviewAudioUrl = Musica.PreviewAudioUrl; // atualiza o preview audio

                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Música",
                    Acao = "Editada",
                    Mensagem = $"✏️ Música '{Musica.Titulo}' foi atualizada!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Música \"{Musica.Titulo}\" atualizada com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de musicas
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao atualizar música: {ex.Message}";
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
            // o ultimo parametro seleciona o album atual
            AlbunsSelectList = new SelectList(albuns, "Id", "Titulo", Musica.AlbumId);
        }
    }
}