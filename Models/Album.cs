using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do álbum é obrigatório")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter entre 1 e 200 caracteres")]
        [Display(Name = "Título do Álbum")]
        public string Titulo { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data de Lançamento")]
        public DateTime DataLancamento { get; set; }

        [Url(ErrorMessage = "URL inválido")]
        [Display(Name = "URL da Capa")]
        public string? CapaUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecione um artista válido")]
        [Display(Name = "Artista")]
        public int ArtistaId { get; set; }

        // Relacionamento muitos-para-um
        [ForeignKey("ArtistaId")]
        public virtual Artista? Artista { get; set; }

        // Relacionamento muitos-para-muitos com Colecao
        public virtual ICollection<AlbumColecao>? AlbumColecoes { get; set; }
    }
}