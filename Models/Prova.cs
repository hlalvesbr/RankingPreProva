using System.ComponentModel.DataAnnotations;

namespace RankingPreProva.Models
{
    public class Prova
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descricao { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<ProvaQuestao> ProvaQuestoes { get; set; } = new List<ProvaQuestao>();
    }
}
