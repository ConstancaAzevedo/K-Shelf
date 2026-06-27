using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa um artista solista de k-pop
    /// contem informacoes sobre a sua carreira individual, albuns associados e o perfil de artista correspondente
    /// </summary>
    public class Solista
    {
        /// <summary>
        /// identificador unico do solista
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// nome artistico do solista (ex: iu, agust d)
        /// </summary>
        [Required(ErrorMessage = "O nome do solista é obrigatório")] // campo obrigatorio
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")] // tamanho entre 2 e 100
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// data de estreia oficial a solo
        /// </summary>
        [DataType(DataType.Date)] // tipo de dados date
        [Display(Name = "Data de Estreia")]
        public DateTime DataEstreia { get; set; }

        /// <summary>
        /// agencia/companhia discografica responsavel pela carreira do solista
        /// </summary>
        [StringLength(100)] // tamanho maximo de 100 caracteres
        [Display(Name = "Companhia")]
        public string? Companhia { get; set; }

        /// <summary>
        /// url da imagem de perfil do solista
        /// </summary>
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        /// <summary>
        /// indica se o solista continua ativo na industria musical
        /// </summary>
        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true; // ativo por padrao

        /// <summary>
        /// relacionamento: vinculacao com a entidade de perfil artista (um solista e tambem um artista)
        /// </summary>
        public virtual Artista? Artista { get; set; }

        /// <summary>
        /// relacionamento: colecao de albuns lancados por este solista na sua carreira a solo
        /// </summary>
        public virtual ICollection<Album>? Albuns { get; set; }
    }
}