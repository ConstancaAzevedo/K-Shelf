using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    /// <summary>
    /// representa um artista individual de k-pop
    /// pode ser membro de um grupo, um artista solista, ou ambos
    /// </summary>
    public class Artista
    {
        /// <summary>identificador unico do artista</summary>
        public int Id { get; set; }

        /// <summary>nome real/completo do artista</summary>
        [Required(ErrorMessage = "O nome é obrigatório")] // campo obrigatorio
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")] // tamanho entre 2 e 100
        [Display(Name = "Nome Real")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>nome artistico usado no palco (ex: suga, lisa)</summary>
        [Required(ErrorMessage = "O nome artístico é obrigatório")] // campo obrigatorio
        [StringLength(100)] // tamanho maximo de 100 caracteres
        [Display(Name = "Nome Artístico")]
        public string? NomeArtistico { get; set; }

        /// <summary>data de nascimento do artista (usada para calcular a idade)</summary>
        [Required(ErrorMessage = "A data de nascimento é obrigatória")] // campo obrigatorio
        [DataType(DataType.Date)] // tipo de dados date
        [Display(Name = "Data de Nascimento")]
        public DateTime DataNascimento { get; set; }

        /// <summary>posicao/papel principal do artista no grupo (ex: rapper, dancarino)</summary>
        [StringLength(50)] // tamanho maximo de 50 caracteres
        [Display(Name = "Posição")]
        public string? Posicao { get; set; }

        /// <summary>pais de origem do artista</summary>
        [Required(ErrorMessage = "A nacionalidade é obrigatória")] // campo obrigatorio
        [StringLength(50)] // tamanho maximo de 50 caracteres
        [Display(Name = "Nacionalidade")]
        public string? Pais { get; set; }

        /// <summary>caminho local ou url absoluto da foto do artista</summary>
        [Required(ErrorMessage = "A imagem é obrigatória")] // campo obrigatorio
        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        /// <summary>data em que o artista estreou ou entrou na agencia</summary>
        [Required(ErrorMessage = "A data de entrada é obrigatória")] // campo obrigatorio
        [Display(Name = "Data de Entrada")]
        [DataType(DataType.Date)] // tipo de dados date
        public DateTime? DataEntrada { get; set; }

        /// <summary>data de saida (caso tenha deixado o grupo/agencia)</summary>
        [Display(Name = "Data de Saída")]
        [DataType(DataType.Date)] // tipo de dados date
        public DateTime? DataSaida { get; set; }

        /// <summary>indica se o artista se encontra no ativo na industria musical</summary>
        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true; // ativo por padrao

        /// <summary>chave estrangeira opcional que liga o artista a um grupo</summary>
        public int? GrupoId { get; set; }

        /// <summary>propriedade de navegacao para o grupo associado</summary>
        [ForeignKey("GrupoId")] // chave estrangeira para a tabela grupos
        [Display(Name = "Grupo")]
        public virtual Grupo? Grupo { get; set; }

        /// <summary>chave estrangeira opcional que liga o artista ao seu perfil de solista</summary>
        public int? SolistaId { get; set; }

        /// <summary>propriedade de navegacao para o perfil de solista associado</summary>
        [ForeignKey("SolistaId")] // chave estrangeira para a tabela solistas
        [Display(Name = "Solista")]
        public virtual Solista? Solista { get; set; }

        /// <summary>lista de albuns associados a este artista</summary>
        public virtual ICollection<Album>? Albuns { get; set; } = new List<Album>();

        /// <summary>
        /// propriedade calculada (nao guardada na bd) que classifica o tipo de artista
        /// </summary>
        [NotMapped] // nao e guardado na base de dados
        public string TipoArtista
        {
            get
            {
                // determina o tipo de artista com base nos ids
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
        /// propriedade calculada que gera o nome de exibicao unindo o nome artistico e o nome real
        /// </summary>
        [NotMapped] // nao e guardado na base de dados
        public string NomeExibicao
        {
            get
            {
                // se tiver nome artistico, mostra nome artistico (nome real)
                return string.IsNullOrEmpty(NomeArtistico) ? Nome : $"{NomeArtistico} ({Nome})";
            }
        }

        /// <summary>
        /// propriedade calculada que determina a idade atual do artista com base na data de nascimento
        /// </summary>
        [NotMapped] // nao e guardado na base de dados
        public int Idade
        {
            get
            {
                // calcula a idade com base na data de nascimento
                var today = DateTime.Today;
                var age = today.Year - DataNascimento.Year;
                // ajusta se o aniversario ainda nao aconteceu este ano
                if (DataNascimento.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}