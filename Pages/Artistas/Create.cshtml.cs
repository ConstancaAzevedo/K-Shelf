using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Artistas
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

        // propriedade que recebe os dados do artista por binding
        [BindProperty]
        public Artista Artista { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public IActionResult OnGet()
        {
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync()
        {
            // validacoes manuais

            // nome obrigatorio
            if (string.IsNullOrWhiteSpace(Artista.Nome))
            {
                ModelState.AddModelError("Artista.Nome", "O nome do artista é obrigatório.");
            }

            // nome com minimo de 2 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Nome) && Artista.Nome.Length < 2)
            {
                ModelState.AddModelError("Artista.Nome", "O nome do artista deve ter pelo menos 2 caracteres.");
            }

            // nome com maximo de 100 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Nome) && Artista.Nome.Length > 100)
            {
                ModelState.AddModelError("Artista.Nome", "O nome do artista não pode exceder 100 caracteres.");
            }

            // nome artistico - maximo 100 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.NomeArtistico) && Artista.NomeArtistico.Length > 100)
            {
                ModelState.AddModelError("Artista.NomeArtistico", "O nome artístico não pode exceder 100 caracteres.");
            }

            // posicao - maximo 50 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Posicao) && Artista.Posicao.Length > 50)
            {
                ModelState.AddModelError("Artista.Posicao", "A posição não pode exceder 50 caracteres.");
            }

            // nacionalidade - maximo 50 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Pais) && Artista.Pais.Length > 50)
            {
                ModelState.AddModelError("Artista.Pais", "A nacionalidade não pode exceder 50 caracteres.");
            }

            // verifica se ja existe um artista com o mesmo nome
            var artistaExistente = await _context.Artistas
                .AnyAsync(a => a.Nome.ToLower() == Artista.Nome.ToLower());

            if (artistaExistente)
            {
                ModelState.AddModelError("Artista.Nome", $"Já existe um artista com o nome \"{Artista.Nome}\"!");
            }

            // validacao: data de saida deve ser posterior a data de entrada
            if (Artista.DataEntrada.HasValue && Artista.DataSaida.HasValue)
            {
                if (Artista.DataSaida <= Artista.DataEntrada)
                {
                    ModelState.AddModelError("Artista.DataSaida", "A data de saída deve ser posterior à data de entrada.");
                }
            }

            // validacao: data de nascimento nao pode ser no futuro
            if (Artista.DataNascimento > DateTime.Now)
            {
                ModelState.AddModelError("Artista.DataNascimento", "A data de nascimento não pode ser no futuro.");
            }

            // validacao: data de entrada nao pode ser no futuro
            if (Artista.DataEntrada.HasValue && Artista.DataEntrada > DateTime.Now)
            {
                ModelState.AddModelError("Artista.DataEntrada", "A data de entrada não pode ser no futuro.");
            }

            // verifica se o modelo e valido
            if (!ModelState.IsValid)
            {
                return Page(); // volta para a pagina com os erros
            }

            try
            {
                // adiciona o artista ao contexto
                _context.Artistas.Add(Artista);
                await _context.SaveChangesAsync(); // guarda na base de dados

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Artista",
                    Acao = "Criado",
                    Mensagem = $"Novo artista '{Artista.Nome}' foi adicionado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Artista \"{Artista.NomeExibicao}\" criado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de artistas
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao criar artista: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}