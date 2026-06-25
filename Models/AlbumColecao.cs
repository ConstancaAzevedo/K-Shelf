using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// Tabela de junção (Muitos-para-Muitos) entre Album e Colecao.
    /// Registra a associação de um álbum específico a uma determinada coleção de utilizador, bem como a data em que foi adicionado.
    /// </summary>
    public class AlbumColecao
    {
        /// <summary>
        /// ID do Álbum associado.
        /// </summary>
        public int AlbumId { get; set; }
        
        /// <summary>
        /// Relacionamento: Referência de navegação para o Álbum.
        /// </summary>
        public virtual Album? Album { get; set; }

        /// <summary>
        /// ID da Coleção associada.
        /// </summary>
        public int ColecaoId { get; set; }
        
        /// <summary>
        /// Relacionamento: Referência de navegação para a Coleção.
        /// </summary>
        public virtual Colecao? Colecao { get; set; }

        /// <summary>
        /// Data e hora em que o álbum foi inserido nesta coleção.
        /// </summary>
        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Adição")]
        public DateTime DataAdicao { get; set; } = DateTime.Now;
    }
}