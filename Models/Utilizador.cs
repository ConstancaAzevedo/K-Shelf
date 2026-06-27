using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa um utilizador personalizado na aplicacao
    /// herda de identityuser para integracao com o asp.net core identity
    /// </summary>
    public class Utilizador : IdentityUser
    {
        /// <summary>
        /// nome completo ou nome de exibicao do utilizador
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")] // campo obrigatorio
        public string NomeUsuario { get; set; } = string.Empty;

        /// <summary>
        /// data e hora em que a conta do utilizador foi criada
        /// </summary>
        public DateTime DataCadastro { get; set; } = DateTime.Now; // define a data atual por padrao
    }
}