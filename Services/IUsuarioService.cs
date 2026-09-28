using RankingPreProva.Models;

namespace RankingPreProva.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> ObterPerfilAsync(int id);

        Task<(bool Sucesso, string? Erro)> AtualizarPerfilAsync(Usuario usuario);
    }
}
