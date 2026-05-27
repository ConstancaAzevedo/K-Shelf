using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace K_Shelf.Models
{
    public class Colecao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da coleção é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Nome da Coleção")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // ALTERADO: IdentityUser → Utilizador
        [ForeignKey("UtilizadorId")]
        public virtual Utilizador? Utilizador { get; set; }  // ← MUDAR AQUI

        public virtual ICollection<AlbumColecao>? AlbumColecoes { get; set; }
    }
}