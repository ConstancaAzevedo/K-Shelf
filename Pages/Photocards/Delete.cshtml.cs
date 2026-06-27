using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Data;
using K_Shelf.Models;
using K_Shelf.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace K_Shelf.Pages.Photocards
{
    /// <summary>
    /// pagina de confirmacao e execucao da eliminacao de um photocard do catalogo
    /// acesso restrito a utilizadores com o papel de administrador
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        // contexto da base de dados para aceder as tabelas
        private readonly ApplicationDbContext _context;
        // hub do signalr para enviar notificacoes em tempo real
        private readonly IHubContext<NotificacaoHub> _hubContext;

        /// <summary>
        /// construtor com injecao de dependencia do contexto da base de dados
        /// </summary>
        public DeleteModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>o photocard a ser apresentado para confirmacao de eliminacao</summary>
        [BindProperty]
        public Photocard Photocard { get; set; } = default!;

        /// <summary>
        /// carrega os detalhes do photocard a eliminar para confirmacao do administrador
        /// </summary>
        /// <param name="id">id do photocard a eliminar</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // validacao do parametro obrigatorio
            if (id == null)
            {
                // guarda mensagem de erro e redireciona para a lista
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index");
            }

            // carrega o photocard com os dados relacionados para exibir ao utilizador
            var photocard = await _context.Photocards
                .Include(p => p.Artista) // inclui o artista associado
                .Include(p => p.Album) // inclui o album associado
                .FirstOrDefaultAsync(p => p.Id == id);

            // se o photocard nao existir, redireciona para a lista com erro
            if (photocard == null)
            {
                TempData["ErrorMessage"] = "Photocard não encontrado.";
                return RedirectToPage("./Index");
            }

            // atribui o photocard a propriedade da pagina
            Photocard = photocard;
            return Page(); // retorna a pagina
        }

        /// <summary>
        /// executa a eliminacao definitiva do photocard da base de dados apos confirmacao
        /// </summary>
        /// <param name="id">id do photocard a eliminar</param>
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // validacao do parametro obrigatorio
            if (id == null)
            {
                // guarda mensagem de erro e redireciona para a lista
                TempData["ErrorMessage"] = "ID do photocard não fornecido.";
                return RedirectToPage("./Index");
            }

            // procura o photocard pelo id
            var photocard = await _context.Photocards.FindAsync(id);

            // se o photocard existir, procede a eliminacao
            if (photocard != null)
            {
                try
                {
                    // remove o photocard do contexto
                    _context.Photocards.Remove(photocard);
                    await _context.SaveChangesAsync(); // guarda as alteracoes

                    // notificacao signalr para todos os clientes
                    await _hubContext.Clients.All.SendAsync("ReceberNotificacao", new
                    {
                        Tipo = "Photocard",
                        Acao = "Deletado",
                        Mensagem = $"Photocard '{photocard.Versao}' foi removido!",
                        Data = DateTime.Now
                    });

                    // guarda mensagem de sucesso nos dados temporarios
                    TempData["SuccessMessage"] = $"Photocard \"{photocard.Versao}\" eliminado com sucesso!";
                }
                catch (Exception ex)
                {
                    // captura erros de integridade referencial ou de acesso a bd
                    TempData["ErrorMessage"] = $"Erro ao eliminar photocard: {ex.Message}";
                }
            }

            // redireciona para a lista de photocards
            return RedirectToPage("./Index");
        }
    }
}