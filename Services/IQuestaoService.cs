using RankingPreProva.Models;

namespace RankingPreProva.Services
{
    public interface IQuestaoService
    {
        Task<List<Questao>> ListarAsync();

        Task<Questao?> ObterAsync(int id);

        Task<(bool Sucesso, string? Erro)> CriarAsync(Questao questao);

        Task<(bool Sucesso, string? Erro)> AtualizarAsync(Questao questao);

        Task<(bool Sucesso, string? Erro)> RemoverAsync(int id);
    }
}
