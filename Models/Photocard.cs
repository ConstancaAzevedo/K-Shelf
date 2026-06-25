using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa um Photocard colecionável disponível no catálogo do sistema.
    /// </summary>
    public class Photocard
    {
        public int Id { get; set; }

        /// <summary>
        /// Nome, descrição ou versão do photocard (ex: "Selfie Ver. A", "Concept Card")
        /// </summary>
        [Required(ErrorMessage = "A legenda/versão do photocard é obrigatória.")]
        [StringLength(100, ErrorMessage = "A legenda/versão não pode ter mais de 100 caracteres.")]
        public string Versao { get; set; } = string.Empty;

        /// <summary>
        /// URL ou caminho relativo para a imagem do photocard
        /// </summary>
        [Required(ErrorMessage = "A imagem do photocard é obrigatória.")]
        public string ImagemUrl { get; set; } = "/imagens/photocards/default.png";

        /// <summary>
        /// ID do Artista (membro ou solista) retratado no photocard
        /// </summary>
        [Required(ErrorMessage = "O artista é obrigatório.")]
        public int ArtistaId { get; set; }

        [ForeignKey("ArtistaId")]
        public Artista? Artista { get; set; }

        /// <summary>
        /// ID do Álbum opcional do qual este photocard faz parte
        /// </summary>
        public int? AlbumId { get; set; }

        [ForeignKey("AlbumId")]
        public Album? Album { get; set; }

        /// <summary>
        /// Relação muitos-para-muitos indireta: utilizadores que adicionaram este photocard
        /// </summary>
        public ICollection<UtilizadorPhotocard>? UtilizadorPhotocards { get; set; }
    }
}
