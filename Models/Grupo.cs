using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa um grupo musical ou banda de K-Pop.
    /// Contém informações sobre o grupo, membros (Artistas) e os seus álbuns.
    /// </summary>
    public class Grupo
    {
        /// <summary>
        /// Identificador único do grupo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do grupo musical (ex: BTS, BLACKPINK).
        /// </summary>
        [Required(ErrorMessage = "O nome do grupo é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Data oficial de estreia (debut) do grupo.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Data de Estreia")]
        public DateTime DataEstreia { get; set; }

        /// <summary>
        /// Agência/Companhia discográfica responsável pelo grupo (ex: HYBE, YG Entertainment).
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Companhia")]
        public string? Companhia { get; set; }

        /// <summary>
        /// Nome oficial do fandom do grupo (ex: ARMY, BLINK).
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Fansigno")]
        public string? Fansigno { get; set; } // Nome do fandom (ex: ARMY, BLINK)

        /// <summary>
        /// URL de uma imagem representativa do grupo.
        /// </summary>
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        /// <summary>
        /// Indica se o grupo continua ativo na indústria musical.
        /// </summary>
        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        /// <summary>
        /// Relacionamento: Coleção de artistas que pertencem ou pertenceram a este grupo (membros).
        /// </summary>
        public virtual ICollection<Artista>? Artistas { get; set; }

        /// <summary>
        /// Relacionamento: Coleção de álbuns lançados por este grupo.
        /// </summary>
        public virtual ICollection<Album>? Albuns { get; set; }
    }
}