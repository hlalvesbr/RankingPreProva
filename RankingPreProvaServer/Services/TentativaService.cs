using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;
using RankingPreProvaServer.Data;
using RankingPreProvaServer.Models;

namespace RankingPreProvaServer.Services;

public class TentativaService(ApplicationDbContext db, GamificacaoService gamificacao, RankingService ranking)
{
    public static readonly Expression<Func<Tentativa, TentativaResumoDto>> ProjecaoResumo = t => new TentativaResumoDto
    {
        TentativaId = t.Id,
        ProvaId = t.ProvaId,
        ProvaTitulo = t.Prova.Titulo,
        Nota = t.Nota,
        NotaMaxima = t.NotaMaxima,
        Acertos = t.Acertos,
        Erros = t.Erros,
        EmBranco = t.EmBranco,
        DuracaoSeg = t.DuracaoSeg,
        EhPrimeira = t.EhPrimeira,
        Finalizada = t.Finalizada ?? t.Iniciada
    };

    public async Task<TentativaIniciadaDto> IniciarAsync(int provaId, int uid)
    {
        var prova = await db.Provas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == provaId && p.Status != StatusItem.Arquivado)
            ?? throw new NaoEncontradoException("Prova não encontrada.");

        var tentativa = await db.Tentativas.FirstOrDefaultAsync(t => t.ProvaId == provaId && t.UsuarioId == uid && t.Finalizada == null);
        if (tentativa is null)
        {
            var ehPrimeira = !await db.Tentativas.AnyAsync(t => t.ProvaId == provaId && t.UsuarioId == uid);
            tentativa = new Tentativa { ProvaId = provaId, UsuarioId = uid, EhPrimeira = ehPrimeira };
            db.Tentativas.Add(tentativa);
            await db.SaveChangesAsync();
        }

        var questoes = await db.ProvaQuestoes.AsNoTracking()
            .Where(pq => pq.ProvaId == provaId)
            .OrderBy(pq => pq.Ordem)
            .Include(pq => pq.Questao).ThenInclude(q => q.Alternativas)
            .ToListAsync();
        if (questoes.Count == 0)
            throw new RegraNegocioException("Esta prova ainda não possui questões.");

        return new TentativaIniciadaDto
        {
            TentativaId = tentativa.Id,
            ProvaId = provaId,
            Titulo = prova.Titulo,
            TempoLimiteMin = prova.TempoLimiteMin,
            Iniciada = DateTime.SpecifyKind(tentativa.Iniciada, DateTimeKind.Utc),
            EhPrimeira = tentativa.EhPrimeira,
            Regra = prova.Regra,
            Questoes = questoes.Select(pq => new QuestaoProvaDto
            {
                QuestaoId = pq.QuestaoId,
                Ordem = pq.Ordem,
                Enunciado = pq.Questao.Enunciado,
                Tipo = pq.Questao.Tipo,
                Alternativas = QuestaoService.AlternativasDto(pq.Questao),
                Banca = pq.Questao.Banca,
                Ano = pq.Questao.Ano,
                Peso = pq.Peso
            }).ToList()
        };
    }

    public static decimal NotaMaxima(Prova prova, IEnumerable<ProvaQuestao> questoes) => prova.Regra == RegraPontuacao.PesoPersonalizado
        ? questoes.Sum(q => q.Peso)
        : questoes.Count() * prova.PontosAcerto;

    public async Task<TentativaResultadoDto> FinalizarAsync(int tentativaId, FinalizarTentativaDto dto, int uid)
    {
        var tentativa = await db.Tentativas
            .Include(t => t.Prova).ThenInclude(p => p.Questoes).ThenInclude(pq => pq.Questao)
            .FirstOrDefaultAsync(t => t.Id == tentativaId) ?? throw new NaoEncontradoException("Tentativa não encontrada.");
        if (tentativa.UsuarioId != uid) throw new ProibidoException();
        if (tentativa.Finalizada is not null) throw new RegraNegocioException("Esta tentativa já foi finalizada.");

        var prova = tentativa.Prova;
        var agora = DateTime.UtcNow;
        var duracao = (int)(agora - tentativa.Iniciada).TotalSeconds;
        if (prova.TempoLimiteMin is int limite)
            duracao = Math.Min(duracao, limite * 60);

        decimal nota = 0;
        int acertos = 0, erros = 0, brancos = 0;
        foreach (var pq in prova.Questoes.OrderBy(x => x.Ordem))
        {
            char? letra = dto.Respostas.TryGetValue(pq.QuestaoId, out var l) && l is char c ? char.ToUpperInvariant(c) : null;
            if (letra is char valida && !Letras.Para(pq.Questao.Tipo).Contains(valida)) letra = null;

            var correta = letra is not null && letra == pq.Questao.Gabarito;
            tentativa.Respostas.Add(new RespostaTentativa { QuestaoId = pq.QuestaoId, Letra = letra, Correta = correta });

            if (letra is null) { brancos++; continue; }
            if (correta)
            {
                acertos++;
                nota += prova.Regra == RegraPontuacao.PesoPersonalizado ? pq.Peso : prova.PontosAcerto;
            }
            else
            {
                erros++;
                if (prova.Regra == RegraPontuacao.CespeAnulacao) nota -= prova.PenalidadeErro;
            }

            if (tentativa.EhPrimeira)
            {
                pq.Questao.TotalRespostas++;
                if (correta) pq.Questao.TotalAcertos++;
            }
        }

        if (prova.NotaMinimaZero && nota < 0) nota = 0;

        tentativa.Finalizada = agora;
        tentativa.DuracaoSeg = Math.Max(duracao, 1);
        tentativa.Nota = nota;
        tentativa.NotaMaxima = NotaMaxima(prova, prova.Questoes);
        tentativa.Acertos = acertos;
        tentativa.Erros = erros;
        tentativa.EmBranco = brancos;
        await db.SaveChangesAsync();

        int xp;
        int? posicao = null;
        if (tentativa.EhPrimeira)
        {
            posicao = await ranking.PosicaoAsync(tentativa);
            xp = GamificacaoService.Xp.ConcluirProva
                 + (acertos + erros) * GamificacaoService.Xp.RespostaQuestao
                 + acertos * GamificacaoService.Xp.Acerto
                 + (posicao <= 3 ? GamificacaoService.Xp.Podio : 0);
        }
        else
        {
            xp = GamificacaoService.Xp.TreinoProva + acertos;
        }

        var recompensa = await gamificacao.ConcederAsync(uid, xp, tentativa.EhPrimeira ? "Prova concluída" : "Treino de prova", registrarAtividade: true);
        var resultado = await ObterResultadoAsync(tentativaId, uid);
        resultado.Recompensa = recompensa;
        return resultado;
    }

    public async Task<TentativaResultadoDto> ObterResultadoAsync(int tentativaId, int uid)
    {
        var t = await db.Tentativas.AsNoTracking()
            .Include(x => x.Prova)
            .Include(x => x.Respostas).ThenInclude(r => r.Questao).ThenInclude(q => q.Alternativas)
            .FirstOrDefaultAsync(x => x.Id == tentativaId) ?? throw new NaoEncontradoException("Tentativa não encontrada.");
        if (t.UsuarioId != uid) throw new ProibidoException();
        if (t.Finalizada is null) throw new RegraNegocioException("Esta tentativa ainda está em andamento.");

        var ordens = await db.ProvaQuestoes.Where(pq => pq.ProvaId == t.ProvaId).ToDictionaryAsync(pq => pq.QuestaoId, pq => pq.Ordem);

        return new TentativaResultadoDto
        {
            TentativaId = t.Id,
            ProvaId = t.ProvaId,
            ProvaTitulo = t.Prova.Titulo,
            Nota = t.Nota,
            NotaMaxima = t.NotaMaxima,
            Acertos = t.Acertos,
            Erros = t.Erros,
            EmBranco = t.EmBranco,
            DuracaoSeg = t.DuracaoSeg,
            EhPrimeira = t.EhPrimeira,
            Finalizada = t.Finalizada.Value,
            PosicaoRanking = t.EhPrimeira ? await ranking.PosicaoAsync(t) : null,
            TotalParticipantes = await db.Tentativas.CountAsync(x => x.ProvaId == t.ProvaId && x.EhPrimeira && x.Finalizada != null),
            Itens = t.Respostas
                .Select(r => new RevisaoItemDto
                {
                    Ordem = ordens.GetValueOrDefault(r.QuestaoId),
                    QuestaoId = r.QuestaoId,
                    Enunciado = r.Questao.Enunciado,
                    Tipo = r.Questao.Tipo,
                    Alternativas = QuestaoService.AlternativasDto(r.Questao),
                    Marcada = r.Letra,
                    Gabarito = r.Questao.Gabarito,
                    Correta = r.Correta,
                    Explicacao = r.Questao.Explicacao
                })
                .OrderBy(i => i.Ordem)
                .ToList()
        };
    }
}
