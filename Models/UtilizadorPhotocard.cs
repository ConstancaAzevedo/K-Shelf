using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// Define o estado do photocard no Binder do colecionador.
    /// </summary>
    public enum EstadoPhotocard
    {
        Possui,    // Na coleção pessoal
        Deseja,    // Na Wishlist
        ParaTroca  // Duplicado, disponível para troca
    }

    /// <summary>
    /// Representa a associação entre um utilizador e o seu photocard no Binder pessoal.
    /// </summary>
    public class UtilizadorPhotocard
    {
        public int Id { get; set; }

        /// <summary>
        /// ID do utilizador dono do Binder
        /// </summary>
        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        [ForeignKey("UtilizadorId")]
        public Utilizador? Utilizador { get; set; }

        /// <summary>
        /// ID do photocard colecionado
        /// </summary>
        [Required]
        public int PhotocardId { get; set; }

        [ForeignKey("PhotocardId")]
        public Photocard? Photocard { get; set; }

        /// <summary>
        /// Estado de posse do photocard (Possui, Deseja, ParaTroca)
        /// </summary>
        [Required(ErrorMessage = "O estado do photocard é obrigatório.")]
        public EstadoPhotocard Estado { get; set; } = EstadoPhotocard.Possui;

        /// <summary>
        /// Quantidade de cópias que o utilizador tem
        /// </summary>
        [Range(1, 100, ErrorMessage = "A quantidade deve estar entre 1 e 100.")]
        public int Quantidade { get; set; } = 1;

        /// <summary>
        /// Notas ou descrições adicionais do utilizador (ex: "Edição especial", "Troco por Felix")
        /// </summary>
        [StringLength(200, ErrorMessage = "As notas não podem exceder 200 caracteres.")]
        public string? Notas { get; set; }
    }
}
