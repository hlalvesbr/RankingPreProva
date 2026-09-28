using RankingPreProva.Models;

namespace RankingPreProva.Repositories
{
    public interface IProvaRepository
    {
        Task<List<Prova>> ObterTodasAsync();

        Task<Prova?> ObterPorIdAsync(int id);

        Task AdicionarAsync(Prova prova);

        Task AtualizarAsync(Prova prova);

        Task RemoverAsync(Prova prova);
    }
}
