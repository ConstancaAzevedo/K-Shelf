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
        public Artista Artista { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            // Validações

            // Nome obrigatório
            if (string.IsNullOrWhiteSpace(Artista.Nome))
            {
                ModelState.AddModelError("Artista.Nome", "O nome do artista é obrigatório.");
            }

            // Nome com mínimo de 2 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Nome) && Artista.Nome.Length < 2)
            {
                ModelState.AddModelError("Artista.Nome", "O nome do artista deve ter pelo menos 2 caracteres.");
            }

            // Nome com máximo de 100 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Nome) && Artista.Nome.Length > 100)
            {
                ModelState.AddModelError("Artista.Nome", "O nome do artista não pode exceder 100 caracteres.");
            }

            // Nome Artístico - máximo 100 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.NomeArtistico) && Artista.NomeArtistico.Length > 100)
            {
                ModelState.AddModelError("Artista.NomeArtistico", "O nome artístico não pode exceder 100 caracteres.");
            }

            // Posição - máximo 50 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Posicao) && Artista.Posicao.Length > 50)
            {
                ModelState.AddModelError("Artista.Posicao", "A posição não pode exceder 50 caracteres.");
            }

            // Nacionalidade - máximo 50 caracteres
            if (!string.IsNullOrWhiteSpace(Artista.Pais) && Artista.Pais.Length > 50)
            {
                ModelState.AddModelError("Artista.Nacionalidade", "A nacionalidade não pode exceder 50 caracteres.");
            }

            // Verificar se já existe um artista com o mesmo nome
            var artistaExistente = await _context.Artistas
                .AnyAsync(a => a.Nome.ToLower() == Artista.Nome.ToLower());

            if (artistaExistente)
            {
                ModelState.AddModelError("Artista.Nome", $"Já existe um artista com o nome \"{Artista.Nome}\"!");
            }

            // Validação: Data de Saída deve ser posterior à Data de Entrada
            if (Artista.DataEntrada.HasValue && Artista.DataSaida.HasValue)
            {
                if (Artista.DataSaida <= Artista.DataEntrada)
                {
                    ModelState.AddModelError("Artista.DataSaida", "A data de saída deve ser posterior à data de entrada.");
                }
            }

            // Validação: Data de Nascimento não pode ser no futuro
            if (Artista.DataNascimento > DateTime.Now)
            {
                ModelState.AddModelError("Artista.DataNascimento", "A data de nascimento não pode ser no futuro.");
            }

            // 1Validação: Data de Entrada não pode ser no futuro
            if (Artista.DataEntrada.HasValue && Artista.DataEntrada > DateTime.Now)
            {
                ModelState.AddModelError("Artista.DataEntrada", "A data de entrada não pode ser no futuro.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _context.Artistas.Add(Artista);
                await _context.SaveChangesAsync();

                // notificação em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Artista",
                    Acao = "Criado",
                    Mensagem = $"Novo artista '{Artista.Nome}' foi adicionado!",
                    Data = DateTime.Now
                });

                TempData["SuccessMessage"] = $"Artista \"{Artista.NomeExibicao}\" criado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar artista: {ex.Message}";
                return Page();
            }
        }
    }
}