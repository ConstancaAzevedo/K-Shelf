using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    public class Solista
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do solista é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data de Estreia")]
        public DateTime DataEstreia { get; set; }

        [StringLength(100)]
        [Display(Name = "Companhia")]
        public string? Companhia { get; set; }

        [Url]
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        // Relacionamento: Um solista tem um artista associado (ele próprio)
        public virtual Artista? Artista { get; set; }

        // Relacionamento: Um solista tem muitos álbuns
        public virtual ICollection<Album>? Albuns { get; set; }
    }
}