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
    [Authorize] // Qualquer utilizador autenticado pode criar coleções
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
        public Colecao Colecao { get; set; } = new Colecao();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {

            // validações manuais 

            // Nome obrigatório
            if (string.IsNullOrWhiteSpace(Colecao.Nome))
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção é obrigatório.");
            }

            // Nome com mínimo de 3 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length < 3)
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção deve ter pelo menos 3 caracteres.");
            }

            // Nome com máximo de 100 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Nome) && Colecao.Nome.Length > 100)
            {
                ModelState.AddModelError("Colecao.Nome", "O nome da coleção não pode exceder 100 caracteres.");
            }

            // Descrição com máximo de 500 caracteres
            if (!string.IsNullOrWhiteSpace(Colecao.Descricao) && Colecao.Descricao.Length > 500)
            {
                ModelState.AddModelError("Colecao.Descricao", "A descrição não pode exceder 500 caracteres.");
            }

            // Se houver erros de validação volta à página
            if (!ModelState.IsValid)
                return Page();

            // Associar a coleção ao utilizador autenticado
            Colecao.UtilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Validar se já existe uma coleção com o mesmo nome para este utilizador
            var colecaoDuplicada = await _context.Colecoes
                .AnyAsync(c => c.UtilizadorId == Colecao.UtilizadorId && 
                               c.Nome.ToLower() == Colecao.Nome.ToLower());

            if (colecaoDuplicada)
            {
                ModelState.AddModelError("Colecao.Nome", $"Já existe uma coleção com o nome \"{Colecao.Nome}\"!");
                return Page();
            }

            Colecao.DataCriacao = DateTime.Now;

            _context.Colecoes.Add(Colecao);
            await _context.SaveChangesAsync();


            // notificação em tempo real
            await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
            {
                Tipo = "Coleção",
                Acao = "Criada",
                Mensagem = $"Nova coleção '{Colecao.Nome}' foi criada!",
                Data = DateTime.Now
            });

            TempData["SuccessMessage"] = $"Coleção \"{Colecao.Nome}\" criada com sucesso!";
            return RedirectToPage("./Index");
        }
    }
}