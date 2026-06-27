using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa um album de k-pop
    /// pode pertencer a um grupo, a um solista, ou a um artista individual
    /// </summary>
    public class Album
    {
        /// <summary>identificador unico do album</summary>
        public int Id { get; set; }

        /// <summary>titulo do album (ex: map of the soul: 7)</summary>
        [Required(ErrorMessage = "O título do álbum é obrigatório")] // campo obrigatorio
        [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter entre 1 e 200 caracteres")] // tamanho maximo de 200 caracteres
        [Display(Name = "Título do Álbum")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>data de lancamento oficial do album</summary>
        [Required(ErrorMessage = "A data de lançamento é obrigatória")] // campo obrigatorio
        [DataType(DataType.Date)] // tipo de dados date
        [Display(Name = "Data de Lançamento")]
        public DateTime DataLancamento { get; set; }

        /// <summary>url ou caminho local da imagem de capa do album</summary>
        [Required(ErrorMessage = "A imagem da capa é obrigatória")] // campo obrigatorio
        [Url(ErrorMessage = "URL inválido")] // valida se e uma url valida
        [Display(Name = "URL da Capa")]
        public string? CapaUrl { get; set; }

        /// <summary>tipos de formatos comuns de albuns de k-pop</summary>
        public enum TipoAlbum
        {
            Studio, // album de estúdio
            Single, // single
            EP, // extended play
            Compilação, // compilacao
            AoVivo, // ao vivo
            Remix, // remix
            MiniAlbum, // mini album
            Album // album generico
        }

        /// <summary>tipo do album (ex: studio, ep, single)</summary>
        [Required(ErrorMessage = "O tipo de álbum é obrigatório")] // campo obrigatorio
        [Display(Name = "Tipo de Álbum")]
        public TipoAlbum Tipo { get; set; }

        /// <summary>formatos/edicoes fisicas comuns na industria de k-pop</summary>
        public enum EdicaoAlbum
        {
            Standard, // edicao standard
            Limited, // edicao limitada
            Special, // edicao especial
            Platform, // edicao platform
            JewelCase, // edicao jewel case
            Photobook // edicao photobook
        }

        /// <summary>edicao do album (ex: standard, limited, photobook)</summary>
        [Display(Name = "Edição")]
        public EdicaoAlbum Edicao { get; set; }

        /// <summary>chave estrangeira opcional para ligar o album a um grupo</summary>
        public int? GrupoId { get; set; }

        /// <summary>propriedade de navegacao para o grupo associado</summary>
        [ForeignKey("GrupoId")] // chave estrangeira para a tabela grupos
        [Display(Name = "Grupo")]
        public virtual Grupo? Grupo { get; set; }

        /// <summary>chave estrangeira opcional para ligar o album a um solista</summary>
        public int? SolistaId { get; set; }

        /// <summary>propriedade de navegacao para o solista associado</summary>
        [ForeignKey("SolistaId")] // chave estrangeira para a tabela solistas
        [Display(Name = "Solista")]
        public virtual Solista? Solista { get; set; }

        /// <summary>chave estrangeira opcional para associar o album a um artista individual</summary>
        public int? ArtistaId { get; set; }

        /// <summary>propriedade de navegacao para o artista associado</summary>
        [ForeignKey("ArtistaId")] // chave estrangeira para a tabela artistas
        [Display(Name = "Artista")]
        public virtual Artista? Artista { get; set; }

        /// <summary>lista de musicas (faixas) pertencentes a este album</summary>
        public virtual ICollection<Musica>? Musicas { get; set; }

        /// <summary>relacoes muitos-para-muitos com as colecoes de utilizadores</summary>
        public virtual ICollection<AlbumColecao>? AlbumColecoes { get; set; }
    }
}