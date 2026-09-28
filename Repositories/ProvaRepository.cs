using Microsoft.EntityFrameworkCore;
using RankingPreProva.Data;
using RankingPreProva.Models;

namespace RankingPreProva.Repositories
{
    public class ProvaRepository : IProvaRepository
    {
        private readonly RankingPreProvaDbContext _context;

        public ProvaRepository(RankingPreProvaDbContext context)
        {
            _context = context;
        }

        public Task<List<Prova>> ObterTodasAsync()
        {
            return _context.Provas
                .Include(p => p.ProvaQuestoes)
                .AsNoTracking()
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync();
        }

        public Task<Prova?> ObterPorIdAsync(int id)
        {
            return _context.Provas
                .Include(p => p.ProvaQuestoes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AdicionarAsync(Prova prova)
        {
            _context.Provas.Add(prova);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Prova prova)
        {
            _context.Provas.Update(prova);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Prova prova)
        {
            _context.Provas.Remove(prova);
            await _context.SaveChangesAsync();
        }
    }
}
