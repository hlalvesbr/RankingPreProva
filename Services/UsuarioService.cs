using RankingPreProva.Models;
using RankingPreProva.Repositories;

namespace RankingPreProva.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public Task<Usuario?> ObterPerfilAsync(int id)
        {
            return _usuarioRepository.ObterPorIdAsync(id);
        }

        public async Task<(bool Sucesso, string? Erro)> AtualizarPerfilAsync(Usuario usuario)
        {
            var existente = await _usuarioRepository.ObterPorIdAsync(usuario.Id);
            if (existente is null)
            {
                return (false, "Usuário não encontrado.");
            }

            if (await _usuarioRepository.EmailEmUsoAsync(usuario.Email, usuario.Id))
            {
                return (false, "Este e-mail já está em uso por outro usuário.");
            }

            if (await _usuarioRepository.ApelidoEmUsoAsync(usuario.Apelido, usuario.Id))
            {
                return (false, "Este apelido já está em uso por outro usuário.");
            }

            existente.Apelido = usuario.Apelido;
            existente.Nome = usuario.Nome;
            existente.Email = usuario.Email;
            existente.AtualizadoEm = DateTime.UtcNow;

            await _usuarioRepository.AtualizarAsync(existente);

            return (true, null);
        }
    }
}
