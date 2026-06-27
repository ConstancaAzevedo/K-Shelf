using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    /// <summary>
    /// tabela de juncao (muitos-para-muitos) entre album e colecao
    /// regista a associacao de um album especifico a uma determinada colecao de utilizador,
    /// bem como a data em que foi adicionado
    /// </summary>
    public class AlbumColecao
    {
        /// <summary>
        /// id do album associado
        /// </summary>
        public int AlbumId { get; set; }

        /// <summary>
        /// relacionamento: referencia de navegacao para o album
        /// </summary>
        public virtual Album? Album { get; set; }

        /// <summary>
        /// id da colecao associada
        /// </summary>
        public int ColecaoId { get; set; }

        /// <summary>
        /// relacionamento: referencia de navegacao para a colecao
        /// </summary>
        public virtual Colecao? Colecao { get; set; }

        /// <summary>
        /// data e hora em que o album foi inserido nesta colecao
        /// </summary>
        [DataType(DataType.DateTime)] // indica que o tipo de dados e datetime
        [Display(Name = "Data de Adição")] // nome amigavel para exibicao
        public DateTime DataAdicao { get; set; } = DateTime.Now; // define a data atual por padrao
    }
}