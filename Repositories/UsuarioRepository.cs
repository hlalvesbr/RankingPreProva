using Microsoft.EntityFrameworkCore;
using RankingPreProva.Data;
using RankingPreProva.Models;

namespace RankingPreProva.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly RankingPreProvaDbContext _context;

        public UsuarioRepository(RankingPreProvaDbContext context)
        {
            _context = context;
        }

        public Task<Usuario?> ObterPorIdAsync(int id)
        {
            return _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<bool> EmailEmUsoAsync(string email, int usuarioId)
        {
            return _context.Usuarios
                .AnyAsync(u => u.Email == email && u.Id != usuarioId);
        }

        public Task<bool> ApelidoEmUsoAsync(string apelido, int usuarioId)
        {
            return _context.Usuarios
                .AnyAsync(u => u.Apelido == apelido && u.Id != usuarioId);
        }

        public async Task AtualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
