using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace K_Shelf.Pages.Colecoes
{
    // qualquer utilizador autenticado pode criar colecoes
    [Authorize]
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

        // propriedade que recebe os dados da colecao por binding
        [BindProperty]
        public Colecao Colecao { get; set; } = new Colecao();

        // metodo executado quando a pagina e carregada via get
        public IActionResult OnGet()
        {
            return Page(); // retorna a pagina
        }

        // metodo executado quando o formulario e submetido via post
        // com validacao do token antiforgery
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // remove a validacao do utilizadorid porque vai ser preenchido automaticamente
            ModelState.Remove("Colecao.UtilizadorId");

            try
            {
                // validacoes manuais

                // nome obrigatorio
                if (string.IsNullOrWhiteSpace(Colecao.Nome))
                {
                    ModelState.AddModelError("Colecao.Nome", "O nome da coleção é obrigatório.");
                }

                // nome com minimo de 3 caracteres
                if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length < 3)
                {
                    ModelState.AddModelError("Colecao.Nome", "O nome da coleção deve ter pelo menos 3 caracteres.");
                }

                // nome com maximo de 100 caracteres
                if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length > 100)
                {
                    ModelState.AddModelError("Colecao.Nome", "O nome da coleção não pode exceder 100 caracteres.");
                }

                // descricao com maximo de 500 caracteres
                if (!string.IsNullOrWhiteSpace(Colecao.Descricao) && Colecao.Descricao.Length > 500)
                {
                    ModelState.AddModelError("Colecao.Descricao", "A descrição não pode exceder 500 caracteres.");
                }

                // se houver erros de validacao volta a pagina
                if (!ModelState.IsValid)
                    return Page();

                // associa a colecao ao utilizador autenticado
                var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(utilizadorId))
                {
                    // se o utilizador nao estiver autenticado, mostra erro
                    ModelState.AddModelError("", "Erro: Utilizador não autenticado. Faça login novamente.");
                    return Page();
                }

                // atribui o id do utilizador a colecao
                Colecao.UtilizadorId = utilizadorId;

                // valida se ja existe uma colecao com o mesmo nome para este utilizador
                var colecaoDuplicada = await _context.Colecoes
                    .AnyAsync(c => c.UtilizadorId == Colecao.UtilizadorId &&
                                   c.Nome.ToLower() == Colecao.Nome.ToLower());

                // se existir, mostra erro
                if (colecaoDuplicada)
                {
                    ModelState.AddModelError("Colecao.Nome", $"Já existe uma coleção com o nome \"{Colecao.Nome}\"!");
                    return Page();
                }

                // define a data de criacao como agora
                Colecao.DataCriacao = DateTime.Now;

                // adiciona a colecao ao contexto
                _context.Colecoes.Add(Colecao);
                await _context.SaveChangesAsync(); // guarda na base de dados

                // notificacao signalr para todos os clientes (dentro de try-catch)
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                    {
                        Tipo = "Coleção",
                        Acao = "Criada",
                        Mensagem = $"Nova coleção '{Colecao.Nome}' foi criada!",
                        Data = DateTime.Now
                    });
                }
                catch (Exception signalREx)
                {
                    // faz log do erro do signalr mas nao interrompe a criacao
                    Console.WriteLine($"Erro ao enviar notificação SignalR: {signalREx.Message}");
                }

                // guarda mensagem de sucesso nos dados temporarios
                TempData["SuccessMessage"] = $"Coleção \"{Colecao.Nome}\" criada com sucesso!";
                return RedirectToPage("./Index"); // redireciona para a lista de colecoes
            }
            catch (DbUpdateException dbEx)
            {
                // erro ao guardar na base de dados
                ModelState.AddModelError("", $"Erro ao guardar a coleção na base de dados: {dbEx.InnerException?.Message}");
                return Page(); // volta para a pagina com o erro
            }
            catch (Exception ex)
            {
                // erro inesperado
                ModelState.AddModelError("", $"Erro inesperado: {ex.Message}");
                return Page(); // volta para a pagina com o erro
            }
        }
    }
}