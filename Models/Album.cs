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

        public enum TipoAlbum
        {
            Studio,
            Single,
            EP,
            Compilação,
            AoVivo,
            Remix
        }

        [Display(Name = "Tipo de Álbum")]
        public TipoAlbum Tipo { get; set; }

        public enum EdicaoAlbum
        {
            Standard,
            Limited,
            Special,
            Platform,
            JewelCase,
            Photobook
        }

        [Display(Name = "Edição")]
        public EdicaoAlbum Edicao { get; set; }

        // FK para Grupo (se for álbum de grupo)
        public int? GrupoId { get; set; }

        [ForeignKey("GrupoId")]
        [Display(Name = "Grupo")]
        public virtual Grupo? Grupo { get; set; }

        // FK para Solista (se for álbum de solista)
        public int? SolistaId { get; set; }

        [ForeignKey("SolistaId")]
        [Display(Name = "Solista")]
        public virtual Solista? Solista { get; set; }

        // FK para Artista (se for álbum de um artista específico - caso raro)
        public int? ArtistaId { get; set; }

        [ForeignKey("ArtistaId")]
        [Display(Name = "Artista")]
        public virtual Artista? Artista { get; set; }

        // Relacionamentos
        public virtual ICollection<Musica>? Musicas { get; set; } // ← NOVO
        public virtual ICollection<AlbumColecao>? AlbumColecoes { get; set; }
    }
}