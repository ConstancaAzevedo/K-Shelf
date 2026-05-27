using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    public class Utilizador : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        public string NomeUsuario { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}