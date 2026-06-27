using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa um grupo musical ou banda de k-pop
    /// contem informacoes sobre o grupo, membros (artistas) e os seus albuns
    /// </summary>
    public class Grupo
    {
        /// <summary>
        /// identificador unico do grupo
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// nome do grupo musical (ex: bts, blackpink)
        /// </summary>
        [Required(ErrorMessage = "O nome do grupo é obrigatório")] // campo obrigatorio
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")] // tamanho entre 2 e 100
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// data oficial de estreia (debut) do grupo
        /// </summary>
        [DataType(DataType.Date)] // tipo de dados date
        [Display(Name = "Data de Estreia")]
        public DateTime DataEstreia { get; set; }

        /// <summary>
        /// agencia/companhia discografica responsavel pelo grupo (ex: hybe, yg entertainment)
        /// </summary>
        [StringLength(100)] // tamanho maximo de 100 caracteres
        [Display(Name = "Companhia")]
        public string? Companhia { get; set; }

        /// <summary>
        /// nome oficial do fandom do grupo (ex: army, blink)
        /// </summary>
        [StringLength(50)] // tamanho maximo de 50 caracteres
        [Display(Name = "Fansigno")]
        public string? Fansigno { get; set; }

        /// <summary>
        /// url de uma imagem representativa do grupo
        /// </summary>
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        /// <summary>
        /// indica se o grupo continua ativo na industria musical
        /// </summary>
        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true; // ativo por padrao

        /// <summary>
        /// relacionamento: colecao de artistas que pertencem ou pertenceram a este grupo (membros)
        /// </summary>
        public virtual ICollection<Artista>? Artistas { get; set; }

        /// <summary>
        /// relacionamento: colecao de albuns lancados por este grupo
        /// </summary>
        public virtual ICollection<Album>? Albuns { get; set; }
    }
}