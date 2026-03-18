using System.ComponentModel.DataAnnotations;

namespace K_Shelf.Models
{
    public class AlbumColecao
    {
        public int AlbumId { get; set; }
        public virtual Album? Album { get; set; }

        public int ColecaoId { get; set; }
        public virtual Colecao? Colecao { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Adição")]
        public DateTime DataAdicao { get; set; } = DateTime.Now;
    }
}