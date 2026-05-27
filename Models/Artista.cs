using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_Shelf.Models
{
    public class Artista
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        [Display(Name = "Nome Real")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Nome Artístico")]
        public string? NomeArtistico { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime DataNascimento { get; set; }

        [StringLength(50)]
        [Display(Name = "Posição")]
        public string? Posicao { get; set; } // Líder, Vocalista, Rapper, Dançarino, etc.

        [StringLength(50)]
        [Display(Name = "Nacionalidade")]
        public string? Nacionalidade { get; set; }

        [Display(Name = "URL da Imagem")]
        public string? ImagemUrl { get; set; }

        [Display(Name = "Data de Entrada")]
        [DataType(DataType.Date)]
        public DateTime? DataEntrada { get; set; }

        [Display(Name = "Data de Saída")]
        [DataType(DataType.Date)]
        public DateTime? DataSaida { get; set; }

        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        // FK para Grupo (se pertencer a um grupo)
        public int? GrupoId { get; set; }

        [ForeignKey("GrupoId")]
        [Display(Name = "Grupo")]
        public virtual Grupo? Grupo { get; set; }

        // FK para Solista (se for solista)
        public int? SolistaId { get; set; }

        [ForeignKey("SolistaId")]
        [Display(Name = "Solista")]
        public virtual Solista? Solista { get; set; }

        // Propriedade calculada para saber o tipo de artista
        [NotMapped] // Não guarda na base de dados
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

        // Propriedade calculada para o nome a exibir
        [NotMapped]
        public string NomeExibicao
        {
            get
            {
                return string.IsNullOrEmpty(NomeArtistico) ? Nome : $"{NomeArtistico} ({Nome})";
            }
        }

        // Idade calculada
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