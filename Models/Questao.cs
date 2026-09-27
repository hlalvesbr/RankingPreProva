using System.ComponentModel.DataAnnotations;

namespace RankingPreProva.Models
{
    public class Questao
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Enunciado { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<OpcaoResposta> Opcoes { get; set; } = new List<OpcaoResposta>();
    }
}
