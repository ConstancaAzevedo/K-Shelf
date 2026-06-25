using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa a coleção/estante virtual de um utilizador registado.
    /// Contém a lista de álbuns adicionados e a referência ao utilizador dono.
    /// </summary>
    public class Colecao
    {
        /// <summary>Identificador único da Coleção.</summary>
        public int Id { get; set; }

        /// <summary>Nome personalizado dado à Coleção pelo utilizador.</summary>
        [Required(ErrorMessage = "O nome da coleção é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Nome da Coleção")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Descrição curta ou observações sobre a Coleção.</summary>
        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        /// <summary>Chave Estrangeira do Utilizador (IdentityUser) proprietário da coleção.</summary>
        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        /// <summary>Data e Hora em que a coleção foi criada.</summary>
        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        /// <summary>Propriedade de Navegação para o Utilizador proprietário.</summary>
        [ForeignKey("UtilizadorId")]
        public virtual Utilizador? Utilizador { get; set; }

        /// <summary>Lista de ligações Muitos-para-Muitos com os Álbuns contidos na Coleção.</summary>
        public virtual ICollection<AlbumColecao>? AlbumColecoes { get; set; }
    }
}