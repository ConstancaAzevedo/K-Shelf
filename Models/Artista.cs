using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    public class Artista
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do artista é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        [Display(Name = "Nome do Artista")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O país de origem é obrigatório")]
        [StringLength(50)]
        [Display(Name = "País")]
        public string Pais { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data de Estreia")]
        public DateTime DataEstreia { get; set; }

        [Url(ErrorMessage = "URL inválido")]
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        // Relacionamento: um artista tem muitos álbuns
        public virtual ICollection<Album>? Albuns { get; set; }
    }
}