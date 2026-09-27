using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RankingPreProva.Models
{
    public class OpcaoResposta
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Texto { get; set; } = string.Empty;

        public bool Correta { get; set; }

        public int QuestaoId { get; set; }

        [ForeignKey(nameof(QuestaoId))]
        public Questao? Questao { get; set; }
    }
}
