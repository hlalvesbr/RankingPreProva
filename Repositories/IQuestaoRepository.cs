using RankingPreProva.Models;

namespace RankingPreProva.Repositories
{
    public interface IQuestaoRepository
    {
        Task<List<Questao>> ObterTodasAsync();

        Task<Questao?> ObterPorIdAsync(int id);

        Task AdicionarAsync(Questao questao);

        Task AtualizarAsync(Questao questao);

        Task RemoverAsync(Questao questao);
    }
}
