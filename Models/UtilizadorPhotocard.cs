using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// define o estado do photocard no binder do colecionador
    /// </summary>
    public enum EstadoPhotocard
    {
        Possui,    // na colecao pessoal
        Deseja,    // na wishlist
        ParaTroca  // duplicado, disponivel para troca
    }

    /// <summary>
    /// representa a associacao entre um utilizador e o seu photocard no binder pessoal
    /// </summary>
    public class UtilizadorPhotocard
    {
        // identificador unico da associacao
        public int Id { get; set; }

        /// <summary>
        /// id do utilizador dono do binder
        /// </summary>
        [Required] // campo obrigatorio
        public string UtilizadorId { get; set; } = string.Empty;

        // relacionamento: utilizador associado
        [ForeignKey("UtilizadorId")] // chave estrangeira para a tabela aspnetusers
        public Utilizador? Utilizador { get; set; }

        /// <summary>
        /// id do photocard colecionado
        /// </summary>
        [Required] // campo obrigatorio
        public int PhotocardId { get; set; }

        // relacionamento: photocard associado
        [ForeignKey("PhotocardId")] // chave estrangeira para a tabela photocards
        public Photocard? Photocard { get; set; }

        /// <summary>
        /// estado de posse do photocard (possui, deseja, paratroca)
        /// </summary>
        [Required(ErrorMessage = "O estado do photocard é obrigatório.")] // campo obrigatorio
        public EstadoPhotocard Estado { get; set; } = EstadoPhotocard.Possui; // possui por padrao

        /// <summary>
        /// quantidade de copias que o utilizador tem
        /// </summary>
        [Range(1, 100, ErrorMessage = "A quantidade deve estar entre 1 e 100.")] // valor entre 1 e 100
        public int Quantidade { get; set; } = 1; // 1 por padrao

        /// <summary>
        /// notas ou descricoes adicionais do utilizador (ex: "edicao especial", "troco por felix")
        /// </summary>
        [StringLength(200, ErrorMessage = "As notas não podem exceder 200 caracteres.")] // tamanho maximo de 200
        public string? Notas { get; set; }
    }
}