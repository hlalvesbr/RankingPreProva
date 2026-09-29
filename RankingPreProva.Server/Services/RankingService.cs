using Microsoft.EntityFrameworkCore;
using RankingPreProva.Shared.Dtos;
using RankingPreProva.Server.Data;
using RankingPreProva.Server.Models;

namespace RankingPreProva.Server.Services;

public class RankingService(ApplicationDbContext db)
{
    public async Task<int> PosicaoAsync(Tentativa t) =>
        await db.Tentativas.CountAsync(o => o.ProvaId == t.ProvaId && o.EhPrimeira && o.Finalizada != null && o.Id != t.Id &&
            (o.Nota > t.Nota || (o.Nota == t.Nota && o.DuracaoSeg < t.DuracaoSeg) ||
             (o.Nota == t.Nota && o.DuracaoSeg == t.DuracaoSeg && o.Finalizada < t.Finalizada))) + 1;

    public async Task<RankingProvaDto> RankingProvaAsync(int provaId, int uid, int top = 100)
    {
        var prova = await db.Provas.AsNoTracking().Include(p => p.Questoes).FirstOrDefaultAsync(p => p.Id == provaId)
            ?? throw new NaoEncontradoException("Prova não encontrada.");

        // Apenas a primeira tentativa de cada usuário vale para o ranking.
        var todos = await db.Tentativas.AsNoTracking()
            .Where(t => t.ProvaId == provaId && t.EhPrimeira && t.Finalizada != null)
            .OrderByDescending(t => t.Nota).ThenBy(t => t.DuracaoSeg).ThenBy(t => t.Finalizada)
            .Select(t => new RankingItemDto
            {
                UsuarioId = t.UsuarioId,
                Nome = t.Usuario.Apelido ?? t.Usuario.NomeExibicao,
                Apelido = t.Usuario.Apelido,
                AvatarUrl = t.Usuario.AvatarUrl,
                Nivel = t.Usuario.Nivel,
                Nota = t.Nota,
                Acertos = t.Acertos,
                DuracaoSeg = t.DuracaoSeg,
                Data = t.Finalizada!.Value
            })
            .ToListAsync();

        for (var i = 0; i < todos.Count; i++)
        {
            todos[i].Posicao = i + 1;
            todos[i].EhVoce = todos[i].UsuarioId == uid;
        }

        return new RankingProvaDto
        {
            NotaMaxima = TentativaService.NotaMaxima(prova, prova.Questoes),
            TotalParticipantes = todos.Count,
            Itens = todos.Take(top).ToList(),
            Voce = todos.FirstOrDefault(r => r.EhVoce)
        };
    }
}
