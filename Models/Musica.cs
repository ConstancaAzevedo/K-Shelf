using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa uma faixa ou musica pertencente a um album
    /// contem detalhes tecnicos como duracao, ordem das faixas, creditos de autoria e links externos
    /// </summary>
    public class Musica
    {
        /// <summary>
        /// identificador unico da musica
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// titulo da musica (ex: dynamite, lovesick girls)
        /// </summary>
        [Required(ErrorMessage = "O título da música é obrigatório")] // campo obrigatorio
        [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter entre 1 e 200 caracteres")] // tamanho entre 1 e 200
        [Display(Name = "Título da Música")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// duracao da musica no formato "mm:ss" (ex: 3:45)
        /// </summary>
        [Display(Name = "Duração")]
        [RegularExpression(@"^([0-9]{1,2}):([0-9]{2})$", ErrorMessage = "Formato inválido. Use mm:ss (ex: 3:45)")] // valida o formato mm:ss
        public TimeSpan? Duracao { get; set; } // guardado como timespan

        /// <summary>
        /// posicao da faixa na ordem do album (track number)
        /// </summary>
        [Display(Name = "Número da Faixa")]
        [Range(1, 100, ErrorMessage = "O número da faixa deve ser entre 1 e 100")] // valor entre 1 e 100
        public int TrackNumber { get; set; }

        /// <summary>
        /// letra da musica
        /// </summary>
        [DataType(DataType.MultilineText)] // texto com multiplas linhas
        [Display(Name = "Letra")]
        public string? Letra { get; set; }

        /// <summary>
        /// nomes dos compositores da musica
        /// </summary>
        [Display(Name = "Compositores")]
        [StringLength(500)] // tamanho maximo de 500 caracteres
        public string? Compositores { get; set; }

        /// <summary>
        /// nomes dos produtores que trabalharam na faixa
        /// </summary>
        [Display(Name = "Produtores")]
        [StringLength(500)] // tamanho maximo de 500 caracteres
        public string? Produtores { get; set; }

        /// <summary>
        /// indica se a faixa foi lancada como single independente
        /// </summary>
        [Display(Name = "É Single")]
        public bool IsSingle { get; set; } = false; // false por padrao

        /// <summary>
        /// indica se a faixa e a principal (title track) promotora do album
        /// </summary>
        [Display(Name = "É Título (Title Track)")]
        public bool IsTitleTrack { get; set; } = false; // false por padrao

        /// <summary>
        /// identificador exclusivo da faixa no spotify para integracoes
        /// </summary>
        [Display(Name = "Spotify ID")]
        public string? SpotifyId { get; set; }

        /// <summary>
        /// url do video oficial da musica no youtube
        /// </summary>
        [Display(Name = "YouTube URL")]
        [Url] // valida se e uma url valida
        public string? YoutubeUrl { get; set; }

        /// <summary>
        /// url de pre-visualizacao de audio (geralmente um clip de 30 segundos)
        /// </summary>
        [Display(Name = "URL de Preview de Áudio")]
        [Url(ErrorMessage = "O URL de preview de áudio deve ser um URL válido.")] // valida se e uma url valida
        public string? PreviewAudioUrl { get; set; }

        /// <summary>
        /// chave estrangeira para o album ao qual a musica pertence
        /// </summary>
        [Required(ErrorMessage = "O álbum é obrigatório")] // campo obrigatorio
        [Display(Name = "Álbum")]
        public int AlbumId { get; set; }

        /// <summary>
        /// relacionamento: referencia de navegacao para o album associado
        /// </summary>
        [ForeignKey("AlbumId")] // chave estrangeira para a tabela albuns
        public virtual Album? Album { get; set; }

        /// <summary>
        /// propriedade calculada que garante exibicao legivel de duracao mesmo quando nula
        /// </summary>
        [NotMapped] // nao e guardado na base de dados
        public string DuracaoFormatada
        {
            get
            {
                // se nao tiver duracao, mostra "--:--"
                if (!Duracao.HasValue)
                    return "--:--";
                // formata a duracao como mm:ss
                return Duracao.Value.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// propriedade calculada para apresentar o titulo formatado com o numero da faixa e rotulo de title track se aplicavel
        /// </summary>
        [NotMapped] // nao e guardado na base de dados
        public string TituloCompleto
        {
            get
            {
                // se for title track, adiciona o rotulo
                if (IsTitleTrack)
                    return $"{TrackNumber}. {Titulo} (Title Track)";
                return $"{TrackNumber}. {Titulo}";
            }
        }
    }
}