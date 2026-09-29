using Microsoft.EntityFrameworkCore;
using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;
using RankingPreProva.Server.Data;
using RankingPreProva.Server.Models;

namespace RankingPreProva.Server.Services;

public class GamificacaoService(ApplicationDbContext db)
{
    public static class Xp
    {
        public const int RespostaQuestao = 2;
        public const int Acerto = 5;
        public const int ConcluirProva = 20;
        public const int TreinoProva = 5;
        public const int Podio = 50;
        public const int CriarQuestao = 10;
        public const int CriarProva = 15;
        public const int VotoPositivoRecebido = 1;
        public const int DesafioDoDia = 10;
        public const int DenunciaAceita = 5;
    }

    public async Task<RecompensaDto> ConcederAsync(int usuarioId, int xp, string motivo, bool registrarAtividade = false)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        var nivelAntes = usuario.Nivel;

        if (xp > 0)
        {
            usuario.Xp += xp;
            db.XpEventos.Add(new XpEvento { UsuarioId = usuarioId, Quantidade = xp, Motivo = motivo });
        }
        if (registrarAtividade)
            AtualizarSequencia(usuario);

        usuario.Nivel = Niveis.NivelPorXp(usuario.Xp);
        await db.SaveChangesAsync();

        var novas = await VerificarBadgesAsync(usuario);

        return new RecompensaDto
        {
            XpGanho = xp,
            XpTotal = usuario.Xp,
            Nivel = usuario.Nivel,
            SubiuNivel = usuario.Nivel > nivelAntes,
            SequenciaDias = usuario.SequenciaDias,
            NovasBadges = novas
        };
    }

    private static void AtualizarSequencia(Usuario usuario)
    {
        var hoje = Relogio.Hoje();
        if (usuario.UltimaAtividade == hoje) return;

        usuario.SequenciaDias = usuario.UltimaAtividade == hoje.AddDays(-1) ? usuario.SequenciaDias + 1 : 1;
        usuario.MaiorSequencia = Math.Max(usuario.MaiorSequencia, usuario.SequenciaDias);
        usuario.UltimaAtividade = hoje;
    }

    public async Task<Dictionary<string, int>> CalcularProgressoAsync(Usuario usuario)
    {
        var uid = usuario.Id;

        var respostasAvulsas = await db.RespostasAvulsas.CountAsync(r => r.UsuarioId == uid);
        var respostasProva = await db.Tentativas
            .Where(t => t.UsuarioId == uid && t.Finalizada != null)
            .SumAsync(t => t.Acertos + t.Erros);

        var questoesCriadas = await db.Questoes.CountAsync(q => q.AutorId == uid && q.Status != StatusItem.Arquivado);
        var provasCriadas = await db.Provas.CountAsync(p => p.AutorId == uid && p.Status != StatusItem.Arquivado);

        var finalizadas = db.Tentativas.Where(t => t.UsuarioId == uid && t.Finalizada != null);
        var provasConcluidas = await finalizadas.Select(t => t.ProvaId).Distinct().CountAsync();
        var gabaritou = await finalizadas.CountAsync(t => t.Erros == 0 && t.EmBranco == 0 && t.Acertos > 0);

        var podio = await db.Tentativas
            .Where(t => t.UsuarioId == uid && t.EhPrimeira && t.Finalizada != null)
            .CountAsync(t => db.Tentativas.Count(o => o.ProvaId == t.ProvaId && o.EhPrimeira && o.Finalizada != null &&
                (o.Nota > t.Nota || (o.Nota == t.Nota && o.DuracaoSeg < t.DuracaoSeg))) < 3);

        var acertosAvulsos = await db.RespostasAvulsas
            .Where(r => r.UsuarioId == uid && r.Correta)
            .SelectMany(r => r.Questao.Assuntos.Select(a => a.Id))
            .GroupBy(id => id)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToListAsync();
        var acertosProva = await db.Tentativas
            .Where(t => t.UsuarioId == uid)
            .SelectMany(t => t.Respostas.Where(r => r.Correta))
            .SelectMany(r => r.Questao.Assuntos.Select(a => a.Id))
            .GroupBy(id => id)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToListAsync();
        var especialista = acertosAvulsos.Concat(acertosProva)
            .GroupBy(x => x.Key)
            .Select(g => g.Sum(x => x.Total))
            .DefaultIfEmpty(0)
            .Max();

        var revisor = await db.Denuncias.CountAsync(d => d.AutorId == uid && d.Status == StatusDenuncia.Aceita);

        var maxQuestao = await db.Questoes.Where(q => q.AutorId == uid).Select(q => (int?)q.SaldoVotos).MaxAsync() ?? 0;
        var maxProva = await db.Provas.Where(p => p.AutorId == uid).Select(p => (int?)p.SaldoVotos).MaxAsync() ?? 0;

        var respostas = respostasAvulsas + respostasProva;

        return new Dictionary<string, int>
        {
            [BadgeCodigos.PrimeiroPasso] = respostas,
            [BadgeCodigos.Centuriao] = respostas,
            [BadgeCodigos.Maratonista] = respostas,
            [BadgeCodigos.Constancia] = Math.Max(usuario.MaiorSequencia, usuario.SequenciaDias),
            [BadgeCodigos.Disciplina] = Math.Max(usuario.MaiorSequencia, usuario.SequenciaDias),
            [BadgeCodigos.Criador] = questoesCriadas,
            [BadgeCodigos.Arquiteto] = provasCriadas,
            [BadgeCodigos.Estreante] = provasConcluidas,
            [BadgeCodigos.Podio] = podio,
            [BadgeCodigos.Gabaritou] = gabaritou,
            [BadgeCodigos.Especialista] = especialista,
            [BadgeCodigos.Revisor] = revisor,
            [BadgeCodigos.Popular] = Math.Max(Math.Max(maxQuestao, maxProva), 0),
        };
    }

    public async Task<List<BadgeDto>> VerificarBadgesAsync(Usuario usuario)
    {
        var progresso = await CalcularProgressoAsync(usuario);
        var badges = await db.Badges.AsNoTracking().ToListAsync();
        var possuidas = await db.UsuarioBadges.Where(ub => ub.UsuarioId == usuario.Id).Select(ub => ub.BadgeId).ToListAsync();

        var novas = new List<BadgeDto>();
        foreach (var badge in badges.Where(b => !possuidas.Contains(b.Id)))
        {
            var valor = progresso.GetValueOrDefault(badge.Codigo);
            if (valor < badge.Meta) continue;

            var ub = new UsuarioBadge { UsuarioId = usuario.Id, BadgeId = badge.Id };
            db.UsuarioBadges.Add(ub);
            novas.Add(ParaDto(badge, ub, valor));
        }

        if (novas.Count > 0)
            await db.SaveChangesAsync();

        return novas;
    }

    public async Task VerificarBadgesAsync(int usuarioId)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId);
        if (usuario is not null)
            await VerificarBadgesAsync(usuario);
    }

    public async Task<List<BadgeDto>> ListarBadgesAsync(int usuarioId, bool incluirProgresso = true)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        var progresso = incluirProgresso ? await CalcularProgressoAsync(usuario) : [];
        var badges = await db.Badges.AsNoTracking().OrderBy(b => b.Ordem).ToListAsync();
        var possuidas = await db.UsuarioBadges.AsNoTracking().Where(ub => ub.UsuarioId == usuarioId).ToDictionaryAsync(ub => ub.BadgeId);

        return badges.Select(b => ParaDto(b, possuidas.GetValueOrDefault(b.Id), progresso.GetValueOrDefault(b.Codigo))).ToList();
    }

    public static BadgeDto ParaDto(Badge badge, UsuarioBadge? conquista, int progresso) => new()
    {
        Codigo = badge.Codigo,
        Nome = badge.Nome,
        Descricao = badge.Descricao,
        Icone = badge.Icone,
        Meta = badge.Meta,
        Progresso = conquista is not null ? badge.Meta : Math.Min(progresso, badge.Meta),
        Conquistada = conquista is not null,
        ConquistadaEm = conquista?.ConquistadaEm
    };
}
