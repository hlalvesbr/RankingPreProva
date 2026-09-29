using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;
using RankingPreProva.Server.Data;
using RankingPreProva.Server.Models;

namespace RankingPreProva.Server.Services;

public class ProvaService(ApplicationDbContext db, GamificacaoService gamificacao)
{
    public static Expression<Func<Prova, ProvaResumoDto>> ProjecaoResumo(ApplicationDbContext db, int uid) => p => new ProvaResumoDto
    {
        Id = p.Id,
        Titulo = p.Titulo,
        ConcursoSimulado = p.ConcursoSimulado,
        Banca = p.Banca,
        Cargo = p.Cargo,
        TotalQuestoes = p.Questoes.Count,
        TotalParticipantes = p.Tentativas.Count(t => t.EhPrimeira && t.Finalizada != null),
        SaldoVotos = p.SaldoVotos,
        MeuVoto = db.Votos.Where(v => v.UsuarioId == uid && v.AlvoTipo == AlvoTipo.Prova && v.AlvoId == p.Id).Select(v => v.Valor).FirstOrDefault(),
        AutorId = p.AutorId,
        AutorNome = p.Autor.NomeExibicao,
        TempoLimiteMin = p.TempoLimiteMin,
        Regra = p.Regra,
        Status = p.Status,
        Publica = p.Publica,
        CriadoEm = p.CriadoEm
    };

    public async Task<PagedResult<ProvaResumoDto>> ListarAsync(ProvaFiltro f, int uid)
    {
        var query = db.Provas.AsNoTracking().AsQueryable();

        query = f.SomenteMinhas
            ? query.Where(p => p.AutorId == uid)
            : query.Where(p => p.Status != StatusItem.Arquivado && (p.Publica || p.AutorId == uid));

        if (f.SomenteFavoritas)
            query = query.Where(p => db.Favoritos.Any(x => x.UsuarioId == uid && x.AlvoTipo == AlvoTipo.Prova && x.AlvoId == p.Id));
        if (!string.IsNullOrWhiteSpace(f.Busca))
        {
            var termo = $"%{f.Busca.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Titulo, termo) ||
                                     EF.Functions.ILike(p.ConcursoSimulado ?? "", termo) ||
                                     EF.Functions.ILike(p.Cargo ?? "", termo) ||
                                     EF.Functions.ILike(p.Orgao ?? "", termo));
        }
        if (!string.IsNullOrWhiteSpace(f.Banca))
            query = query.Where(p => EF.Functions.ILike(p.Banca ?? "", f.Banca.Trim()));

        var total = await query.CountAsync();

        var ordenada = query.OrderBy(p => p.SaldoVotos < 0 ? 1 : 0);
        ordenada = f.Ordem switch
        {
            "votadas" => ordenada.ThenByDescending(p => p.SaldoVotos).ThenByDescending(p => p.Id),
            "populares" => ordenada.ThenByDescending(p => p.Tentativas.Count(t => t.EhPrimeira)).ThenByDescending(p => p.Id),
            _ => ordenada.ThenByDescending(p => p.Id)
        };

        var pageSize = Math.Clamp(f.PageSize, 1, 60);
        var page = Math.Max(f.Page, 1);
        var itens = await ordenada.Skip((page - 1) * pageSize).Take(pageSize).Select(ProjecaoResumo(db, uid)).ToListAsync();
        return new PagedResult<ProvaResumoDto> { Items = itens, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<ProvaDto> ObterAsync(int id, int uid)
    {
        var resumo = await db.Provas.AsNoTracking().Where(p => p.Id == id).Select(ProjecaoResumo(db, uid)).FirstOrDefaultAsync()
            ?? throw new NaoEncontradoException("Prova não encontrada.");
        var p = await db.Provas.AsNoTracking().FirstAsync(x => x.Id == id);

        if (p.Status == StatusItem.Arquivado && p.AutorId != uid)
            throw new NaoEncontradoException("Esta prova foi arquivada.");

        var primeira = await db.Tentativas.AsNoTracking()
            .Where(t => t.ProvaId == id && t.UsuarioId == uid && t.EhPrimeira && t.Finalizada != null)
            .Select(TentativaService.ProjecaoResumo)
            .FirstOrDefaultAsync();

        var dto = new ProvaDto
        {
            Descricao = p.Descricao,
            Orgao = p.Orgao,
            Ano = p.Ano,
            PontosAcerto = p.PontosAcerto,
            PenalidadeErro = p.PenalidadeErro,
            NotaMinimaZero = p.NotaMinimaZero,
            EhAutor = p.AutorId == uid,
            PossuiTentativas = await db.Tentativas.AnyAsync(t => t.ProvaId == id),
            Favorita = await db.Favoritos.AnyAsync(f => f.UsuarioId == uid && f.AlvoTipo == AlvoTipo.Prova && f.AlvoId == id),
            MinhaTentativaEmAndamento = await db.Tentativas
                .Where(t => t.ProvaId == id && t.UsuarioId == uid && t.Finalizada == null)
                .Select(t => (int?)t.Id).FirstOrDefaultAsync(),
            MinhaPrimeira = primeira,
            Assuntos = await db.ProvaQuestoes.Where(pq => pq.ProvaId == id)
                .SelectMany(pq => pq.Questao.Assuntos.Select(a => a.Nome))
                .GroupBy(n => n).OrderByDescending(g => g.Count()).Select(g => g.Key)
                .Take(10).ToListAsync()
        };
        CopiarResumo(resumo, dto);
        return dto;
    }

    private static void CopiarResumo(ProvaResumoDto origem, ProvaDto destino)
    {
        destino.Id = origem.Id;
        destino.Titulo = origem.Titulo;
        destino.ConcursoSimulado = origem.ConcursoSimulado;
        destino.Banca = origem.Banca;
        destino.Cargo = origem.Cargo;
        destino.TotalQuestoes = origem.TotalQuestoes;
        destino.TotalParticipantes = origem.TotalParticipantes;
        destino.SaldoVotos = origem.SaldoVotos;
        destino.MeuVoto = origem.MeuVoto;
        destino.AutorId = origem.AutorId;
        destino.AutorNome = origem.AutorNome;
        destino.TempoLimiteMin = origem.TempoLimiteMin;
        destino.Regra = origem.Regra;
        destino.Status = origem.Status;
        destino.Publica = origem.Publica;
        destino.CriadoEm = origem.CriadoEm;
    }

    public async Task<ProvaEdicaoDto> ObterEdicaoAsync(int id, int uid)
    {
        var p = await CarregarDoAutorAsync(id, uid);
        var questoes = await db.ProvaQuestoes.AsNoTracking()
            .Where(pq => pq.ProvaId == id)
            .OrderBy(pq => pq.Ordem)
            .Select(pq => new ProvaQuestaoItemDto
            {
                QuestaoId = pq.QuestaoId,
                Peso = pq.Peso,
                Enunciado = pq.Questao.Enunciado,
                Tipo = pq.Questao.Tipo,
                Banca = pq.Questao.Banca,
                Ano = pq.Questao.Ano,
                Assuntos = pq.Questao.Assuntos.Select(a => a.Nome).ToList()
            })
            .ToListAsync();
        questoes.ForEach(q => q.Enunciado = TextoUtil.Resumo(q.Enunciado, 160));

        return new ProvaEdicaoDto
        {
            Titulo = p.Titulo,
            Descricao = p.Descricao,
            ConcursoSimulado = p.ConcursoSimulado,
            Banca = p.Banca,
            Cargo = p.Cargo,
            Orgao = p.Orgao,
            Ano = p.Ano,
            TempoLimiteMin = p.TempoLimiteMin,
            Publica = p.Publica,
            Regra = p.Regra,
            PontosAcerto = p.PontosAcerto,
            PenalidadeErro = p.PenalidadeErro,
            NotaMinimaZero = p.NotaMinimaZero,
            Questoes = questoes
        };
    }

    private async Task<Prova> CarregarDoAutorAsync(int id, int uid)
    {
        var p = await db.Provas.Include(x => x.Questoes).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NaoEncontradoException("Prova não encontrada.");
        if (p.AutorId != uid)
            throw new ProibidoException("Apenas o autor pode editar esta prova.");
        return p;
    }

    public async Task<CriadoDto> CriarAsync(ProvaEdicaoDto dto, int uid)
    {
        var p = new Prova { AutorId = uid };
        AplicarMetadados(p, dto);
        AplicarRegras(p, dto);
        await AplicarQuestoesAsync(p, dto);
        db.Provas.Add(p);
        await db.SaveChangesAsync();

        var recompensa = await gamificacao.ConcederAsync(uid, GamificacaoService.Xp.CriarProva, "Prova criada");
        return new CriadoDto { Id = p.Id, Recompensa = recompensa };
    }

    public async Task AtualizarAsync(int id, ProvaEdicaoDto dto, int uid)
    {
        var p = await CarregarDoAutorAsync(id, uid);
        if (p.Status == StatusItem.Arquivado)
            throw new RegraNegocioException("Provas arquivadas não podem ser editadas.");

        AplicarMetadados(p, dto);

        // Com tentativas registradas, questões e regras ficam congeladas para preservar o ranking.
        if (!await db.Tentativas.AnyAsync(t => t.ProvaId == id))
        {
            AplicarRegras(p, dto);
            await AplicarQuestoesAsync(p, dto);
        }

        p.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id, int uid)
    {
        var p = await CarregarDoAutorAsync(id, uid);
        if (await db.Tentativas.AnyAsync(t => t.ProvaId == id))
            p.Status = StatusItem.Arquivado;
        else
            db.Provas.Remove(p);
        await db.SaveChangesAsync();
    }

    private static void AplicarMetadados(Prova p, ProvaEdicaoDto dto)
    {
        static string? Limpar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        p.Titulo = dto.Titulo.Trim();
        p.Descricao = Limpar(dto.Descricao);
        p.ConcursoSimulado = Limpar(dto.ConcursoSimulado);
        p.Banca = Limpar(dto.Banca);
        p.Cargo = Limpar(dto.Cargo);
        p.Orgao = Limpar(dto.Orgao);
        p.Ano = dto.Ano;
        p.Publica = dto.Publica;
    }

    private static void AplicarRegras(Prova p, ProvaEdicaoDto dto)
    {
        p.TempoLimiteMin = dto.TempoLimiteMin;
        p.Regra = dto.Regra;
        p.PontosAcerto = dto.PontosAcerto;
        p.PenalidadeErro = dto.Regra == RegraPontuacao.CespeAnulacao ? dto.PenalidadeErro : 0;
        p.NotaMinimaZero = dto.NotaMinimaZero;
    }

    private async Task AplicarQuestoesAsync(Prova p, ProvaEdicaoDto dto)
    {
        var ids = dto.Questoes.Select(q => q.QuestaoId).ToList();
        var atuais = p.Questoes.Select(x => x.QuestaoId).ToList();
        var validas = await db.Questoes
            .Where(q => ids.Contains(q.Id) && (q.Status != StatusItem.Arquivado || atuais.Contains(q.Id)))
            .Select(q => q.Id)
            .ToListAsync();
        if (validas.Count != ids.Distinct().Count())
            throw new RegraNegocioException("Uma ou mais questões não existem ou foram arquivadas.");

        foreach (var removida in p.Questoes.Where(x => !ids.Contains(x.QuestaoId)).ToList())
            p.Questoes.Remove(removida);

        var ordem = 1;
        foreach (var item in dto.Questoes)
        {
            var existente = p.Questoes.FirstOrDefault(x => x.QuestaoId == item.QuestaoId);
            if (existente is null)
            {
                existente = new ProvaQuestao { QuestaoId = item.QuestaoId };
                p.Questoes.Add(existente);
            }
            existente.Ordem = ordem++;
            existente.Peso = dto.Regra == RegraPontuacao.PesoPersonalizado ? item.Peso : 1;
        }
    }
}
