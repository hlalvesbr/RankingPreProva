using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;
using RankingPreProvaServer.Data;
using RankingPreProvaServer.Models;

namespace RankingPreProvaServer.Services;

public class QuestaoService(ApplicationDbContext db, GamificacaoService gamificacao, UsuarioService usuarios)
{
    public static Expression<Func<Questao, QuestaoResumoDto>> ProjecaoResumo(ApplicationDbContext db, int uid) => q => new QuestaoResumoDto
    {
        Id = q.Id,
        Enunciado = q.Enunciado,
        Tipo = q.Tipo,
        Ano = q.Ano,
        Origem = q.Origem,
        Banca = q.Banca,
        Concurso = q.Concurso,
        Cargo = q.Cargo,
        Dificuldade = q.Dificuldade,
        Assuntos = q.Assuntos.OrderBy(a => a.Nome).Select(a => a.Nome).ToList(),
        SaldoVotos = q.SaldoVotos,
        MeuVoto = db.Votos.Where(v => v.UsuarioId == uid && v.AlvoTipo == AlvoTipo.Questao && v.AlvoId == q.Id).Select(v => v.Valor).FirstOrDefault(),
        AutorId = q.AutorId,
        AutorNome = q.Autor.NomeExibicao,
        Status = q.Status,
        TotalRespostas = q.TotalRespostas,
        TotalAcertos = q.TotalAcertos,
        JaRespondi = db.RespostasAvulsas.Any(r => r.UsuarioId == uid && r.QuestaoId == q.Id)
    };

    public async Task<PagedResult<QuestaoResumoDto>> ListarAsync(QuestaoFiltro f, int uid)
    {
        var query = db.Questoes.AsNoTracking().AsQueryable();

        query = f.SomenteMinhas
            ? query.Where(q => q.AutorId == uid)
            : query.Where(q => q.Status != StatusItem.Arquivado);

        if (f.SomenteFavoritas)
            query = query.Where(q => db.Favoritos.Any(x => x.UsuarioId == uid && x.AlvoTipo == AlvoTipo.Questao && x.AlvoId == q.Id));
        if (f.NaoRespondidas)
            query = query.Where(q => !db.RespostasAvulsas.Any(r => r.UsuarioId == uid && r.QuestaoId == q.Id));
        if (!string.IsNullOrWhiteSpace(f.Busca))
        {
            var termo = $"%{f.Busca.Trim()}%";
            query = query.Where(q => EF.Functions.ILike(q.Enunciado, termo) ||
                                     EF.Functions.ILike(q.Concurso ?? "", termo) ||
                                     EF.Functions.ILike(q.Cargo ?? "", termo) ||
                                     q.Assuntos.Any(a => EF.Functions.ILike(a.Nome, termo)));
        }
        if (!string.IsNullOrWhiteSpace(f.Assunto))
        {
            var slug = TextoUtil.Slug(f.Assunto);
            query = query.Where(q => q.Assuntos.Any(a => a.Slug == slug));
        }
        if (!string.IsNullOrWhiteSpace(f.Banca))
            query = query.Where(q => EF.Functions.ILike(q.Banca ?? "", f.Banca.Trim()));
        if (!string.IsNullOrWhiteSpace(f.Concurso))
            query = query.Where(q => EF.Functions.ILike(q.Concurso ?? "", $"%{f.Concurso.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(f.Cargo))
            query = query.Where(q => EF.Functions.ILike(q.Cargo ?? "", $"%{f.Cargo.Trim()}%"));
        if (f.Ano is not null) query = query.Where(q => q.Ano == f.Ano);
        if (f.Tipo is not null) query = query.Where(q => q.Tipo == f.Tipo);
        if (f.Origem is not null) query = query.Where(q => q.Origem == f.Origem);
        if (f.Dificuldade is not null) query = query.Where(q => q.Dificuldade == f.Dificuldade);

        var total = await query.CountAsync();

        // Itens com saldo negativo sempre vão para o fim da fila.
        var ordenada = query.OrderBy(q => q.SaldoVotos < 0 ? 1 : 0);
        ordenada = f.Ordem switch
        {
            "votadas" => ordenada.ThenByDescending(q => q.SaldoVotos).ThenByDescending(q => q.Id),
            "populares" => ordenada.ThenByDescending(q => q.TotalRespostas).ThenByDescending(q => q.Id),
            "antigas" => ordenada.ThenBy(q => q.Id),
            "aleatoria" => ordenada.ThenBy(q => EF.Functions.Random()),
            _ => ordenada.ThenByDescending(q => q.Id)
        };

        var pageSize = Math.Clamp(f.PageSize, 1, 100);
        var page = Math.Max(f.Page, 1);
        var itens = await ordenada.Skip((page - 1) * pageSize).Take(pageSize).Select(ProjecaoResumo(db, uid)).ToListAsync();
        itens.ForEach(i => i.Enunciado = TextoUtil.Resumo(i.Enunciado, 260));

        return new PagedResult<QuestaoResumoDto> { Items = itens, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<QuestaoDto> ObterAsync(int id, int uid)
    {
        var q = await db.Questoes.AsNoTracking()
            .Include(x => x.Autor)
            .Include(x => x.Alternativas)
            .Include(x => x.Assuntos)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new NaoEncontradoException("Questão não encontrada.");

        if (q.Status == StatusItem.Arquivado && q.AutorId != uid)
            throw new NaoEncontradoException("Esta questão foi arquivada.");

        return new QuestaoDto
        {
            Id = q.Id,
            Enunciado = q.Enunciado,
            Tipo = q.Tipo,
            Ano = q.Ano,
            Origem = q.Origem,
            Banca = q.Banca,
            Concurso = q.Concurso,
            Cargo = q.Cargo,
            Orgao = q.Orgao,
            Dificuldade = q.Dificuldade,
            Assuntos = q.Assuntos.Select(a => a.Nome).OrderBy(n => n).ToList(),
            SaldoVotos = q.SaldoVotos,
            MeuVoto = await db.Votos.Where(v => v.UsuarioId == uid && v.AlvoTipo == AlvoTipo.Questao && v.AlvoId == id).Select(v => v.Valor).FirstOrDefaultAsync(),
            AutorId = q.AutorId,
            AutorNome = q.Autor.NomeExibicao,
            Status = q.Status,
            TotalRespostas = q.TotalRespostas,
            TotalAcertos = q.TotalAcertos,
            JaRespondi = await db.RespostasAvulsas.AnyAsync(r => r.UsuarioId == uid && r.QuestaoId == id),
            Alternativas = AlternativasDto(q),
            CriadoEm = q.CriadoEm,
            AtualizadoEm = q.AtualizadoEm,
            EhAutor = q.AutorId == uid,
            Favorita = await db.Favoritos.AnyAsync(f => f.UsuarioId == uid && f.AlvoTipo == AlvoTipo.Questao && f.AlvoId == id),
            TotalDenunciasAbertas = await db.Denuncias.CountAsync(d => d.AlvoTipo == AlvoTipo.Questao && d.AlvoId == id && d.Status == StatusDenuncia.Aberta)
        };
    }

    public static List<AlternativaDto> AlternativasDto(Questao q) => q.Tipo == TipoQuestao.CertoErrado
        ? [new AlternativaDto { Letra = 'C', Texto = "Certo" }, new AlternativaDto { Letra = 'E', Texto = "Errado" }]
        : q.Alternativas.OrderBy(a => a.Letra).Select(a => new AlternativaDto { Letra = a.Letra, Texto = a.Texto }).ToList();

    public async Task<QuestaoEdicaoDto> ObterEdicaoAsync(int id, int uid)
    {
        var q = await CarregarDoAutorAsync(id, uid);
        return new QuestaoEdicaoDto
        {
            Enunciado = q.Enunciado,
            Tipo = q.Tipo,
            Alternativas = q.Tipo == TipoQuestao.MultiplaEscolha ? AlternativasDto(q) : [],
            Gabarito = q.Gabarito,
            Explicacao = q.Explicacao,
            Ano = q.Ano,
            Origem = q.Origem,
            Banca = q.Banca,
            Concurso = q.Concurso,
            Cargo = q.Cargo,
            Orgao = q.Orgao,
            Dificuldade = q.Dificuldade,
            Assuntos = q.Assuntos.Select(a => a.Nome).ToList()
        };
    }

    private async Task<Questao> CarregarDoAutorAsync(int id, int uid)
    {
        var q = await db.Questoes
            .Include(x => x.Alternativas)
            .Include(x => x.Assuntos)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new NaoEncontradoException("Questão não encontrada.");
        if (q.AutorId != uid)
            throw new ProibidoException("Apenas o autor pode editar esta questão.");
        return q;
    }

    public async Task<CriadoDto> CriarAsync(QuestaoEdicaoDto dto, int uid)
    {
        var q = new Questao { AutorId = uid };
        await AplicarAsync(q, dto);
        db.Questoes.Add(q);
        await db.SaveChangesAsync();

        var recompensa = await gamificacao.ConcederAsync(uid, GamificacaoService.Xp.CriarQuestao, "Questão criada");
        return new CriadoDto { Id = q.Id, Recompensa = recompensa };
    }

    public async Task AtualizarAsync(int id, QuestaoEdicaoDto dto, int uid)
    {
        var q = await CarregarDoAutorAsync(id, uid);
        if (q.Status == StatusItem.Arquivado)
            throw new RegraNegocioException("Questões arquivadas não podem ser editadas.");
        await AplicarAsync(q, dto);
        q.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id, int uid)
    {
        var q = await CarregarDoAutorAsync(id, uid);
        var emUso = await db.ProvaQuestoes.AnyAsync(pq => pq.QuestaoId == id) ||
                    await db.RespostasAvulsas.AnyAsync(r => r.QuestaoId == id) ||
                    await db.RespostasTentativa.AnyAsync(r => r.QuestaoId == id);
        if (emUso)
            q.Status = StatusItem.Arquivado;
        else
            db.Questoes.Remove(q);
        await db.SaveChangesAsync();
    }

    private async Task AplicarAsync(Questao q, QuestaoEdicaoDto dto)
    {
        q.Enunciado = dto.Enunciado.Trim();
        q.Tipo = dto.Tipo;
        q.Gabarito = char.ToUpperInvariant(dto.Gabarito!.Value);
        q.Explicacao = string.IsNullOrWhiteSpace(dto.Explicacao) ? null : dto.Explicacao.Trim();
        q.Ano = dto.Ano;
        q.Origem = dto.Origem;
        q.Banca = Limpar(dto.Banca);
        q.Concurso = Limpar(dto.Concurso);
        q.Cargo = Limpar(dto.Cargo);
        q.Orgao = Limpar(dto.Orgao);
        q.Dificuldade = dto.Dificuldade;

        var novas = dto.Tipo == TipoQuestao.MultiplaEscolha
            ? dto.Alternativas.Zip(Letras.MultiplaEscolha, (alt, letra) => (Letra: letra, Texto: alt.Texto.Trim())).ToList()
            : [];
        foreach (var removida in q.Alternativas.Where(a => novas.All(n => n.Letra != a.Letra)).ToList())
            q.Alternativas.Remove(removida);
        foreach (var (letra, texto) in novas)
        {
            var existente = q.Alternativas.FirstOrDefault(a => a.Letra == letra);
            if (existente is null)
                q.Alternativas.Add(new Alternativa { Letra = letra, Texto = texto });
            else
                existente.Texto = texto;
        }

        var assuntos = await ResolverAssuntosAsync(dto.Assuntos);
        foreach (var removido in q.Assuntos.Where(a => assuntos.All(n => n.Slug != a.Slug)).ToList())
            q.Assuntos.Remove(removido);
        foreach (var assunto in assuntos.Where(a => q.Assuntos.All(e => e.Slug != a.Slug)))
            q.Assuntos.Add(assunto);
    }

    private static string? Limpar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private async Task<List<Assunto>> ResolverAssuntosAsync(IEnumerable<string> nomes)
    {
        var porSlug = nomes
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => (Nome: n.Trim(), Slug: TextoUtil.Slug(n)))
            .Where(x => x.Slug.Length > 0)
            .DistinctBy(x => x.Slug)
            .ToList();
        var slugs = porSlug.Select(x => x.Slug).ToList();
        var existentes = await db.Assuntos.Where(a => slugs.Contains(a.Slug)).ToListAsync();

        foreach (var (nome, slug) in porSlug.Where(x => existentes.All(e => e.Slug != x.Slug)))
        {
            var novo = new Assunto { Nome = nome.Length > 100 ? nome[..100] : nome, Slug = slug };
            db.Assuntos.Add(novo);
            existentes.Add(novo);
        }
        return existentes;
    }

    public async Task<RespostaResultadoDto> ResponderAsync(int id, char letra, int uid)
    {
        var q = await db.Questoes.FirstOrDefaultAsync(x => x.Id == id && x.Status != StatusItem.Arquivado)
            ?? throw new NaoEncontradoException("Questão não encontrada.");

        letra = char.ToUpperInvariant(letra);
        if (!Letras.Para(q.Tipo).Contains(letra))
            throw new RegraNegocioException("Alternativa inválida.");

        var correta = letra == q.Gabarito;
        var primeiraVez = !await db.RespostasAvulsas.AnyAsync(r => r.UsuarioId == uid && r.QuestaoId == id);
        var inicioHoje = Relogio.InicioDoDiaUtc(Relogio.Hoje());
        var ehDesafio = await usuarios.DesafioDoDiaIdAsync() == id &&
                        !await db.RespostasAvulsas.AnyAsync(r => r.UsuarioId == uid && r.QuestaoId == id && r.Data >= inicioHoje);

        db.RespostasAvulsas.Add(new RespostaAvulsa { UsuarioId = uid, QuestaoId = id, Letra = letra, Correta = correta });
        if (primeiraVez)
        {
            q.TotalRespostas++;
            if (correta) q.TotalAcertos++;
        }
        await db.SaveChangesAsync();

        var xp = primeiraVez ? GamificacaoService.Xp.RespostaQuestao + (correta ? GamificacaoService.Xp.Acerto : 0) : 0;
        if (ehDesafio && correta) xp += GamificacaoService.Xp.DesafioDoDia;
        var recompensa = await gamificacao.ConcederAsync(uid, xp, ehDesafio ? "Desafio do dia" : "Questão respondida", registrarAtividade: true);

        return new RespostaResultadoDto
        {
            Correta = correta,
            Gabarito = q.Gabarito,
            Explicacao = q.Explicacao,
            TotalRespostas = q.TotalRespostas,
            TotalAcertos = q.TotalAcertos,
            Recompensa = recompensa
        };
    }

    public async Task<FiltrosDisponiveisDto> FiltrosAsync()
    {
        var ativas = db.Questoes.AsNoTracking().Where(q => q.Status != StatusItem.Arquivado);
        return new FiltrosDisponiveisDto
        {
            Bancas = await ativas.Where(q => q.Banca != null).Select(q => q.Banca!).Distinct().OrderBy(b => b).Take(200).ToListAsync(),
            Anos = await ativas.Where(q => q.Ano != null).Select(q => q.Ano!.Value).Distinct().OrderByDescending(a => a).ToListAsync(),
            Assuntos = await db.Assuntos.AsNoTracking()
                .Select(a => new AssuntoDto { Nome = a.Nome, Slug = a.Slug, TotalQuestoes = a.Questoes.Count(q => q.Status != StatusItem.Arquivado) })
                .Where(a => a.TotalQuestoes > 0)
                .OrderByDescending(a => a.TotalQuestoes)
                .Take(200)
                .ToListAsync()
        };
    }

    public async Task<List<AssuntoDto>> SugerirAssuntosAsync(string? termo)
    {
        var query = db.Assuntos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(termo))
        {
            var slug = TextoUtil.Slug(termo);
            query = query.Where(a => a.Slug.Contains(slug));
        }
        return await query
            .Select(a => new AssuntoDto { Nome = a.Nome, Slug = a.Slug, TotalQuestoes = a.Questoes.Count })
            .OrderByDescending(a => a.TotalQuestoes)
            .Take(15)
            .ToListAsync();
    }
}
