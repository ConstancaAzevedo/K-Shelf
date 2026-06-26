using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// Representa um artista individual de K-Pop.
    /// Pode ser membro de um Grupo, um artista Solista, ou ambos.
    /// </summary>
    public class Artista
    {
        /// <summary>Identificador único do Artista.</summary>
        public int Id { get; set; }

        /// <summary>Nome real/completo do Artista.</summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        [Display(Name = "Nome Real")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Nome artístico usado no palco (ex: Suga, Lisa).</summary>
        [StringLength(100)]
        [Display(Name = "Nome Artístico")]
        public string? NomeArtistico { get; set; }

        /// <summary>Data de nascimento do Artista (usada para calcular a idade).</summary>
        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime DataNascimento { get; set; }

        /// <summary>Posição/Papel principal do artista no grupo (ex: Rapper, Dançarino).</summary>
        [StringLength(50)]
        [Display(Name = "Posição")]
        public string? Posicao { get; set; }

        /// <summary>País de origem do Artista.</summary>
        [StringLength(50)]
        [Display(Name = "Nacionalidade")]
        public string? Pais { get; set; }

        /// <summary>Caminho local ou URL absoluto da foto do Artista.</summary>
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        /// <summary>Data em que o artista estreou ou entrou na agência.</summary>
        [Display(Name = "Data de Entrada")]
        [DataType(DataType.Date)]
        public DateTime? DataEntrada { get; set; }

        /// <summary>Data de saída (caso tenha deixado o grupo/agência).</summary>
        [Display(Name = "Data de Saída")]
        [DataType(DataType.Date)]
        public DateTime? DataSaida { get; set; }

        /// <summary>Indica se o artista se encontra no ativo na indústria musical.</summary>
        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        /// <summary>Chave Estrangeira opcional que liga o Artista a um Grupo.</summary>
        public int? GrupoId { get; set; }

        /// <summary>Propriedade de Navegação para o Grupo associado.</summary>
        [ForeignKey("GrupoId")]
        [Display(Name = "Grupo")]
        public virtual Grupo? Grupo { get; set; }

        /// <summary>Chave Estrangeira opcional que liga o Artista ao seu perfil de Solista.</summary>
        public int? SolistaId { get; set; }

        /// <summary>Propriedade de Navegação para o perfil de Solista associado.</summary>
        [ForeignKey("SolistaId")]
        [Display(Name = "Solista")]
        public virtual Solista? Solista { get; set; }

        /// <summary>Lista de Álbuns associados a este Artista.</summary>
        public virtual ICollection<Album>? Albuns { get; set; } = new List<Album>();

        /// <summary>
        /// Propriedade Calculada (não guardada na BD) que classifica o tipo de artista.
        /// </summary>
        [NotMapped]
        public string TipoArtista
        {
            get
            {
                if (GrupoId.HasValue && SolistaId.HasValue)
                    return "Membro de Grupo e Solista";
                if (GrupoId.HasValue)
                    return "Membro de Grupo";
                if (SolistaId.HasValue)
                    return "Solista";
                return "Artista Independente";
            }
        }

        /// <summary>
        /// Propriedade Calculada que gera o nome de exibição unindo o nome artístico e o nome real.
        /// </summary>
        [NotMapped]
        public string NomeExibicao
        {
            get
            {
                return string.IsNullOrEmpty(NomeArtistico) ? Nome : $"{NomeArtistico} ({Nome})";
            }
        }

        /// <summary>
        /// Propriedade Calculada que determina a idade atual do artista com base na data de nascimento.
        /// </summary>
        [NotMapped]
        public int Idade
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DataNascimento.Year;
                if (DataNascimento.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}