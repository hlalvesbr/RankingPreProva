using Microsoft.EntityFrameworkCore;
using RankingPreProva.Data;
using RankingPreProva.Models;

namespace RankingPreProva.Repositories
{
    public class QuestaoRepository : IQuestaoRepository
    {
        private readonly RankingPreProvaDbContext _context;

        public QuestaoRepository(RankingPreProvaDbContext context)
        {
            _context = context;
        }

        public Task<List<Questao>> ObterTodasAsync()
        {
            return _context.Questoes
                .Include(q => q.Opcoes)
                .AsNoTracking()
                .OrderByDescending(q => q.CriadoEm)
                .ToListAsync();
        }

        public Task<Questao?> ObterPorIdAsync(int id)
        {
            return _context.Questoes
                .Include(q => q.Opcoes)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task AdicionarAsync(Questao questao)
        {
            _context.Questoes.Add(questao);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Questao questao)
        {
            _context.Questoes.Update(questao);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Questao questao)
        {
            _context.Questoes.Remove(questao);
            await _context.SaveChangesAsync();
        }
    }
}
