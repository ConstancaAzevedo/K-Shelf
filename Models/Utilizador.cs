using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    public class Utilizador
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public string SenhaHash { get; set; } = string.Empty;

        [Compare("SenhaHash", ErrorMessage = "As senhas não coincidem")]
        [DataType(DataType.Password)]
        [NotMapped]
        public string? ConfirmarSenha { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}