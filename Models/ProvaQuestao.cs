namespace RankingPreProva.Models
{
    public class ProvaQuestao
    {
        public int ProvaId { get; set; }

        public Prova Prova { get; set; } = null!;

        public int QuestaoId { get; set; }

        public Questao Questao { get; set; } = null!;

        public int Ordem { get; set; }

        public DateTime AdicionadoEm { get; set; } = DateTime.UtcNow;
    }
}
