using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa um artista solista de K-Pop.
    /// Contém informações sobre a sua carreira individual, álbuns associados e o perfil de Artista correspondente.
    /// </summary>
    public class Solista
    {
        /// <summary>
        /// Identificador único do solista.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome artístico do solista (ex: IU, Agust D).
        /// </summary>
        [Required(ErrorMessage = "O nome do solista é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Data de estreia oficial a solo.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Data de Estreia")]
        public DateTime DataEstreia { get; set; }

        /// <summary>
        /// Agência/Companhia discográfica responsável pela carreira do solista.
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Companhia")]
        public string? Companhia { get; set; }

        /// <summary>
        /// URL da imagem de perfil do solista.
        /// </summary>
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        /// <summary>
        /// Indica se o solista continua ativo na indústria musical.
        /// </summary>
        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        /// <summary>
        /// Relacionamento: Vinculação com a entidade de perfil Artista (um solista é também um artista).
        /// </summary>
        public virtual Artista? Artista { get; set; }

        /// <summary>
        /// Relacionamento: Coleção de álbuns lançados por este solista na sua carreira a solo.
        /// </summary>
        public virtual ICollection<Album>? Albuns { get; set; }
    }
}