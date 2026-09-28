using RankingPreProva.Models;

namespace RankingPreProva.Services
{
    public interface IProvaService
    {
        Task<List<Prova>> ListarAsync();

        Task<Prova?> ObterAsync(int id);

        Task<(bool Sucesso, string? Erro)> CriarAsync(Prova prova);

        Task<(bool Sucesso, string? Erro)> AtualizarAsync(Prova prova);

        Task<(bool Sucesso, string? Erro)> RemoverAsync(int id);
    }
}
