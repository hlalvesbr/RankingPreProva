using RankingPreProva.Models;

namespace RankingPreProva.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorIdAsync(int id);

        Task<bool> EmailEmUsoAsync(string email, int usuarioId);

        Task<bool> ApelidoEmUsoAsync(string apelido, int usuarioId);

        Task AtualizarAsync(Usuario usuario);
    }
}
