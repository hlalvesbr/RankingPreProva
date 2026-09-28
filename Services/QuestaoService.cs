using RankingPreProva.Models;
using RankingPreProva.Repositories;

namespace RankingPreProva.Services
{
    public class QuestaoService : IQuestaoService
    {
        private readonly IQuestaoRepository _questaoRepository;

        public QuestaoService(IQuestaoRepository questaoRepository)
        {
            _questaoRepository = questaoRepository;
        }

        public Task<List<Questao>> ListarAsync()
        {
            return _questaoRepository.ObterTodasAsync();
        }

        public Task<Questao?> ObterAsync(int id)
        {
            return _questaoRepository.ObterPorIdAsync(id);
        }

        public async Task<(bool Sucesso, string? Erro)> CriarAsync(Questao questao)
        {
            if (string.IsNullOrWhiteSpace(questao.Enunciado))
            {
                return (false, "O enunciado da questão é obrigatório.");
            }

            questao.CriadoEm = DateTime.UtcNow;
            questao.AtualizadoEm = DateTime.UtcNow;

            await _questaoRepository.AdicionarAsync(questao);

            return (true, null);
        }

        public async Task<(bool Sucesso, string? Erro)> AtualizarAsync(Questao questao)
        {
            if (string.IsNullOrWhiteSpace(questao.Enunciado))
            {
                return (false, "O enunciado da questão é obrigatório.");
            }

            var existente = await _questaoRepository.ObterPorIdAsync(questao.Id);
            if (existente is null)
            {
                return (false, "Questão não encontrada.");
            }

            existente.Enunciado = questao.Enunciado;
            existente.AtualizadoEm = DateTime.UtcNow;

            SincronizarOpcoes(existente, questao);

            await _questaoRepository.AtualizarAsync(existente);

            return (true, null);
        }

        public async Task<(bool Sucesso, string? Erro)> RemoverAsync(int id)
        {
            var existente = await _questaoRepository.ObterPorIdAsync(id);
            if (existente is null)
            {
                return (false, "Questão não encontrada.");
            }

            await _questaoRepository.RemoverAsync(existente);

            return (true, null);
        }

        private static void SincronizarOpcoes(Questao existente, Questao atualizada)
        {
            var idsMantidos = atualizada.Opcoes
                .Where(o => o.Id != 0)
                .Select(o => o.Id)
                .ToHashSet();

            var removidas = existente.Opcoes
                .Where(o => !idsMantidos.Contains(o.Id))
                .ToList();

            foreach (var removida in removidas)
            {
                existente.Opcoes.Remove(removida);
            }

            foreach (var opcao in atualizada.Opcoes)
            {
                if (opcao.Id == 0)
                {
                    existente.Opcoes.Add(new OpcaoResposta
                    {
                        Texto = opcao.Texto,
                        Correta = opcao.Correta,
                        QuestaoId = existente.Id
                    });
                }
                else
                {
                    var atual = existente.Opcoes.FirstOrDefault(o => o.Id == opcao.Id);
                    if (atual is not null)
                    {
                        atual.Texto = opcao.Texto;
                        atual.Correta = opcao.Correta;
                    }
                }
            }
        }
    }
}
