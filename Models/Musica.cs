using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa uma faixa ou música pertencente a um álbum.
    /// Contém detalhes técnicos como duração, ordem das faixas, créditos de autoria e links externos.
    /// </summary>
    public class Musica
    {
        /// <summary>
        /// Identificador único da música.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Título da música (ex: Dynamite, Lovesick Girls).
        /// </summary>
        [Required(ErrorMessage = "O título da música é obrigatório")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter entre 1 e 200 caracteres")]
        [Display(Name = "Título da Música")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Duração da música no formato "mm:ss" (ex: 3:45).
        /// </summary>
        [Display(Name = "Duração")]
        [RegularExpression(@"^([0-9]{1,2}):([0-9]{2})$", ErrorMessage = "Formato inválido. Use mm:ss (ex: 3:45)")]
        public TimeSpan? Duracao { get; set; } // Formato: 00:03:45

        /// <summary>
        /// Posição da faixa na ordem do álbum (Track Number).
        /// </summary>
        [Display(Name = "Número da Faixa")]
        [Range(1, 100, ErrorMessage = "O número da faixa deve ser entre 1 e 100")]
        public int TrackNumber { get; set; }

        /// <summary>
        /// Letra da música.
        /// </summary>
        [DataType(DataType.MultilineText)]
        [Display(Name = "Letra")]
        public string? Letra { get; set; }

        /// <summary>
        /// Nomes dos compositores da música.
        /// </summary>
        [Display(Name = "Compositores")]
        [StringLength(500)]
        public string? Compositores { get; set; }

        /// <summary>
        /// Nomes dos produtores que trabalharam na faixa.
        /// </summary>
        [Display(Name = "Produtores")]
        [StringLength(500)]
        public string? Produtores { get; set; }

        /// <summary>
        /// Indica se a faixa foi lançada como single independente.
        /// </summary>
        [Display(Name = "É Single")]
        public bool IsSingle { get; set; } = false;

        /// <summary>
        /// Indica se a faixa é a principal (Title Track) promotora do álbum.
        /// </summary>
        [Display(Name = "É Título (Title Track)")]
        public bool IsTitleTrack { get; set; } = false;

        /// <summary>
        /// Identificador exclusivo da faixa no Spotify para integrações.
        /// </summary>
        [Display(Name = "Spotify ID")]
        public string? SpotifyId { get; set; }

        /// <summary>
        /// URL do vídeo oficial da música no YouTube.
        /// </summary>
        [Display(Name = "YouTube URL")]
        [Url]
        public string? YoutubeUrl { get; set; }

        /// <summary>
        /// URL de pré-visualização de áudio (geralmente um clip de 30 segundos).
        /// </summary>
        [Display(Name = "URL de Preview de Áudio")]
        [Url(ErrorMessage = "O URL de preview de áudio deve ser um URL válido.")]
        public string? PreviewAudioUrl { get; set; }

        /// <summary>
        /// Chave estrangeira para o Álbum ao qual a música pertence.
        /// </summary>
        [Required(ErrorMessage = "O álbum é obrigatório")]
        [Display(Name = "Álbum")]
        public int AlbumId { get; set; }

        /// <summary>
        /// Relacionamento: Referência de navegação para o Álbum associado.
        /// </summary>
        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        /// <summary>
        /// Propriedade calculada que garante exibição legível de duração mesmo quando nula.
        /// </summary>
        [NotMapped]
        public string DuracaoFormatada
        {
            get
            {
                if (!Duracao.HasValue)
                    return "--:--";
                return Duracao.Value.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// Propriedade calculada para apresentar o título formatado com o número da faixa e rótulo de Title Track se aplicável.
        /// </summary>
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