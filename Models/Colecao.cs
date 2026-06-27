using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa a colecao/estante virtual de um utilizador registado
    /// contem a lista de albuns adicionados e a referencia ao utilizador dono
    /// </summary>
    public class Colecao
    {
        /// <summary>identificador unico da colecao</summary>
        public int Id { get; set; }

        /// <summary>nome personalizado dado a colecao pelo utilizador</summary>
        [Required(ErrorMessage = "O nome da coleção é obrigatório")] // campo obrigatorio
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")] // tamanho entre 3 e 100
        [Display(Name = "Nome da Coleção")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>descricao curta ou observacoes sobre a colecao</summary>
        [StringLength(500)] // tamanho maximo de 500 caracteres
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        /// <summary>chave estrangeira do utilizador (identityuser) proprietario da colecao</summary>
        [Required] // campo obrigatorio
        public string UtilizadorId { get; set; } = string.Empty;

        /// <summary>data e hora em que a colecao foi criada</summary>
        [DataType(DataType.DateTime)] // tipo de dados datetime
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now; // define a data atual por padrao

        /// <summary>propriedade de navegacao para o utilizador proprietario</summary>
        [ForeignKey("UtilizadorId")] // chave estrangeira para a tabela aspnetusers
        public virtual Utilizador? Utilizador { get; set; }

        /// <summary>lista de ligacoes muitos-para-muitos com os albuns contidos na colecao</summary>
        public virtual ICollection<AlbumColecao>? AlbumColecoes { get; set; }
    }
}