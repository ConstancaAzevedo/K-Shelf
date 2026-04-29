using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    public class Musica
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título da música é obrigatório")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter entre 1 e 200 caracteres")]
        [Display(Name = "Título da Música")]
        public string Titulo { get; set; } = string.Empty;

        [Display(Name = "Duração")]
        [RegularExpression(@"^([0-9]{1,2}):([0-9]{2})$", ErrorMessage = "Formato inválido. Use mm:ss (ex: 3:45)")]
        public string? Duracao { get; set; } // Formato: "3:45"

        [Display(Name = "Número da Faixa")]
        [Range(1, 100, ErrorMessage = "O número da faixa deve ser entre 1 e 100")]
        public int TrackNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Letra")]
        public string? Letra { get; set; }

        [Display(Name = "Compositores")]
        [StringLength(500)]
        public string? Compositores { get; set; }

        [Display(Name = "Produtores")]
        [StringLength(500)]
        public string? Produtores { get; set; }

        [Display(Name = "É Single")]
        public bool IsSingle { get; set; } = false;

        [Display(Name = "É Título (Title Track)")]
        public bool IsTitleTrack { get; set; } = false;

        [Display(Name = "Spotify ID")]
        public string? SpotifyId { get; set; }

        [Display(Name = "YouTube URL")]
        [Url]
        public string? YoutubeUrl { get; set; }

        // FK para Album
        [Required(ErrorMessage = "O álbum é obrigatório")]
        [Display(Name = "Álbum")]
        public int AlbumId { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        // Propriedade calculada para exibição
        [NotMapped]
        public string DuracaoFormatada
        {
            get
            {
                if (string.IsNullOrEmpty(Duracao))
                    return "--:--";
                return Duracao;
            }
        }

        // Propriedade calculada para exibição completa
        [NotMapped]
        public string TituloCompleto
        {
            get
            {
                if (IsTitleTrack)
                    return $"{TrackNumber}. {Titulo} (Title Track)";
                return $"{TrackNumber}. {Titulo}";
            }
        }
    }
}