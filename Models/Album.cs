using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa um álbum de K-Pop.
    /// Pode pertencer a um Grupo, a um Solista, ou a um Artista individual.
    /// </summary>
    public class Album
    {
        /// <summary>Identificador único do Álbum.</summary>
        public int Id { get; set; }

        /// <summary>Título do Álbum (ex: Map of the Soul: 7).</summary>
        [Required(ErrorMessage = "O título do álbum é obrigatório")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter entre 1 e 200 caracteres")]
        [Display(Name = "Título do Álbum")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Data de lançamento oficial do Álbum.</summary>
        [DataType(DataType.Date)]
        [Display(Name = "Data de Lançamento")]
        public DateTime DataLancamento { get; set; }

        /// <summary>URL ou caminho local da imagem de capa do Álbum.</summary>
        [Url(ErrorMessage = "URL inválido")]
        [Display(Name = "URL da Capa")]
        public string? CapaUrl { get; set; }

        /// <summary>Tipos de formatos comuns de álbuns de K-Pop.</summary>
        public enum TipoAlbum
        {
            Studio,
            Single,
            EP,
            Compilação,
            AoVivo,
            Remix
        }

        /// <summary>Tipo do Álbum (ex: Studio, EP, Single).</summary>
        [Display(Name = "Tipo de Álbum")]
        public TipoAlbum Tipo { get; set; }

        /// <summary>Formatos/Edições físicas comuns na indústria de K-Pop.</summary>
        public enum EdicaoAlbum
        {
            Standard,
            Limited,
            Special,
            Platform,
            JewelCase,
            Photobook
        }

        /// <summary>Edição do Álbum (ex: Standard, Limited, Photobook).</summary>
        [Display(Name = "Edição")]
        public EdicaoAlbum Edicao { get; set; }

        /// <summary>Chave Estrangeira opcional para ligar o álbum a um Grupo.</summary>
        public int? GrupoId { get; set; }

        /// <summary>Propriedade de Navegação para o Grupo associado.</summary>
        [ForeignKey("GrupoId")]
        [Display(Name = "Grupo")]
        public virtual Grupo? Grupo { get; set; }

        /// <summary>Chave Estrangeira opcional para ligar o álbum a um Solista.</summary>
        public int? SolistaId { get; set; }

        /// <summary>Propriedade de Navegação para o Solista associado.</summary>
        [ForeignKey("SolistaId")]
        [Display(Name = "Solista")]
        public virtual Solista? Solista { get; set; }

        /// <summary>Chave Estrangeira opcional para associar o álbum a um Artista individual.</summary>
        public int? ArtistaId { get; set; }

        /// <summary>Propriedade de Navegação para o Artista associado.</summary>
        [ForeignKey("ArtistaId")]
        [Display(Name = "Artista")]
        public virtual Artista? Artista { get; set; }

        /// <summary>Lista de Músicas (faixas) pertencentes a este Álbum.</summary>
        public virtual ICollection<Musica>? Musicas { get; set; }

        /// <summary>Relações Muitos-para-Muitos com as Coleções de utilizadores.</summary>
        public virtual ICollection<AlbumColecao>? AlbumColecoes { get; set; }
    }
}