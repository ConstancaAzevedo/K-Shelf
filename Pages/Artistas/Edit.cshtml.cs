using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace K_Shelf.Pages.Artistas
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

        // propriedade que recebe os dados do artista por binding
        [BindProperty]
        public Artista Artista { get; set; } = new();

        // metodo executado quando a pagina e carregada via get
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // verifica se o id foi fornecido
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID do artista não fornecido";
                return NotFound(); // retorna erro 404
            }

            // procura o artista pelo id
            var artista = await _context.Artistas.FindAsync(id);
            // se o artista nao existir, retorna erro
            if (artista == null)
            {
                TempData["ErrorMessage"] = "Artista não encontrado";
                return NotFound(); // retorna erro 404
            }

            // atribui o artista a propriedade da pagina
            Artista = artista;
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

            // verifica se ja existe outro artista com o mesmo nome (excluindo o proprio)
            var artistaExistente = await _context.Artistas
                .AnyAsync(a => a.Nome.ToLower() == Artista.Nome.ToLower() && a.Id != Artista.Id);

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
                // procura o artista original pelo id
                var artistaOriginal = await _context.Artistas.FindAsync(Artista.Id);
                // se o artista nao existir, retorna erro
                if (artistaOriginal == null)
                {
                    TempData["ErrorMessage"] = "Artista não encontrado";
                    return NotFound(); // retorna erro 404
                }

                // atualiza os campos editaveis do artista
                artistaOriginal.Nome = Artista.Nome; // atualiza o nome
                artistaOriginal.NomeArtistico = Artista.NomeArtistico; // atualiza o nome artistico
                artistaOriginal.DataNascimento = Artista.DataNascimento; // atualiza a data de nascimento
                artistaOriginal.Posicao = Artista.Posicao; // atualiza a posicao
                artistaOriginal.Pais = Artista.Pais; // atualiza o pais
                artistaOriginal.ImagemUrl = Artista.ImagemUrl; // atualiza a url da imagem
                artistaOriginal.DataEntrada = Artista.DataEntrada; // atualiza a data de entrada
                artistaOriginal.DataSaida = Artista.DataSaida; // atualiza a data de saida
                artistaOriginal.IsAtivo = Artista.IsAtivo; // atualiza o status ativo/inativo

                await _context.SaveChangesAsync(); // guarda as alteracoes

                // notificacao signalr para todos os clientes
                await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                {
                    Tipo = "Artista",
                    Acao = "Editado",
                    Mensagem = $"✏️ Artista '{Artista.Nome}' foi atualizado!",
                    Data = DateTime.Now
                });

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Artista \"{Artista.NomeExibicao}\" atualizado com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de artistas
            }
            catch (DbUpdateConcurrencyException)
            {
                // verifica se o artista ainda existe
                if (!await _context.Artistas.AnyAsync(a => a.Id == Artista.Id))
                {
                    TempData["ErrorMessage"] = "Artista não encontrado.";
                    return NotFound(); // retorna erro 404
                }
                throw; // relanca a excecao
            }
            catch (Exception ex)
            {
                // guarda mensagem de erro nos dados temporarios
                TempData["ErrorMessage"] = $"Erro ao atualizar artista: {ex.Message}";
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}