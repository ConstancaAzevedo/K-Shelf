using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa um utilizador personalizado na aplicação.
    /// Herda de IdentityUser para integração com o ASP.NET Core Identity.
    /// </summary>
    public class Utilizador : IdentityUser
    {
        /// <summary>
        /// Nome completo ou nome de exibição do utilizador
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        public string NomeUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora em que a conta do utilizador foi criada
        /// </summary>
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}