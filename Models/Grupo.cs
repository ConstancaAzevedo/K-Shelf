using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    public class Grupo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do grupo é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data de Estreia")]
        public DateTime DataEstreia { get; set; }

        [StringLength(100)]
        [Display(Name = "Companhia")]
        public string? Companhia { get; set; }

        [StringLength(50)]
        [Display(Name = "Fansigno")]
        public string? Fansigno { get; set; } // Nome do fandom (ex: ARMY, BLINK)

        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        // Relacionamento: Um grupo tem muitos artistas (membros)
        public virtual ICollection<Artista>? Artistas { get; set; }

        // Relacionamento: Um grupo tem muitos álbuns
        public virtual ICollection<Album>? Albuns { get; set; }
    }
}