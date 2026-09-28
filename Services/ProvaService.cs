using RankingPreProva.Models;
using RankingPreProva.Repositories;

namespace RankingPreProva.Services
{
    public class ProvaService : IProvaService
    {
        private readonly IProvaRepository _provaRepository;

        public ProvaService(IProvaRepository provaRepository)
        {
            _provaRepository = provaRepository;
        }

        public Task<List<Prova>> ListarAsync()
        {
            return _provaRepository.ObterTodasAsync();
        }

        public Task<Prova?> ObterAsync(int id)
        {
            return _provaRepository.ObterPorIdAsync(id);
        }

        public async Task<(bool Sucesso, string? Erro)> CriarAsync(Prova prova)
        {
            if (string.IsNullOrWhiteSpace(prova.Titulo))
            {
                return (false, "O título da prova é obrigatório.");
            }

            prova.CriadoEm = DateTime.UtcNow;
            prova.AtualizadoEm = DateTime.UtcNow;

            await _provaRepository.AdicionarAsync(prova);

            return (true, null);
        }

        public async Task<(bool Sucesso, string? Erro)> AtualizarAsync(Prova prova)
        {
            if (string.IsNullOrWhiteSpace(prova.Titulo))
            {
                return (false, "O título da prova é obrigatório.");
            }

            var existente = await _provaRepository.ObterPorIdAsync(prova.Id);
            if (existente is null)
            {
                return (false, "Prova não encontrada.");
            }

            existente.Titulo = prova.Titulo;
            existente.Descricao = prova.Descricao;
            existente.AtualizadoEm = DateTime.UtcNow;

            await _provaRepository.AtualizarAsync(existente);

            return (true, null);
        }

        public async Task<(bool Sucesso, string? Erro)> RemoverAsync(int id)
        {
            var existente = await _provaRepository.ObterPorIdAsync(id);
            if (existente is null)
            {
                return (false, "Prova não encontrada.");
            }

            await _provaRepository.RemoverAsync(existente);

            return (true, null);
        }
    }
}
