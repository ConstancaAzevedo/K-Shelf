using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa um photocard colecionavel disponivel no catalogo do sistema
    /// </summary>
    public class Photocard
    {
        // identificador unico do photocard
        public int Id { get; set; }

        /// <summary>
        /// nome, descricao ou versao do photocard (ex: "selfie ver. a", "concept card")
        /// </summary>
        [Required(ErrorMessage = "A legenda/versão do photocard é obrigatória.")] // campo obrigatorio
        [StringLength(100, ErrorMessage = "A legenda/versão não pode ter mais de 100 caracteres.")] // tamanho maximo de 100
        public string Versao { get; set; } = string.Empty;

        /// <summary>
        /// url ou caminho relativo para a imagem do photocard
        /// </summary>
        [Required(ErrorMessage = "A imagem do photocard é obrigatória.")] // campo obrigatorio
        public string ImagemUrl { get; set; } = "/imagens/photocards/default.png"; // imagem padrao

        /// <summary>
        /// id do artista (membro ou solista) retratado no photocard
        /// </summary>
        [Required(ErrorMessage = "O artista é obrigatório.")] // campo obrigatorio
        public int ArtistaId { get; set; }

        // relacionamento: artista associado ao photocard
        [ForeignKey("ArtistaId")] // chave estrangeira para a tabela artistas
        public Artista? Artista { get; set; }

        /// <summary>
        /// id do album opcional do qual este photocard faz parte
        /// </summary>
        public int? AlbumId { get; set; }

        // relacionamento: album associado ao photocard (opcional)
        [ForeignKey("AlbumId")] // chave estrangeira para a tabela albuns
        public Album? Album { get; set; }

        /// <summary>
        /// relacao muitos-para-muitos indireta: utilizadores que adicionaram este photocard
        /// </summary>
        public ICollection<UtilizadorPhotocard>? UtilizadorPhotocards { get; set; }
    }
}