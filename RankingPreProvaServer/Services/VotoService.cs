using Microsoft.EntityFrameworkCore;
using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;
using RankingPreProvaServer.Data;
using RankingPreProvaServer.Models;

namespace RankingPreProvaServer.Services;

public class VotoService(ApplicationDbContext db, GamificacaoService gamificacao)
{
    public async Task<VotoResultadoDto> VotarAsync(AlvoTipo alvo, int alvoId, int valor, int uid)
    {
        if (valor is < -1 or > 1)
            throw new RegraNegocioException("Voto inválido.");

        Questao? questao = null;
        Prova? prova = null;
        int autorId;
        if (alvo == AlvoTipo.Questao)
        {
            questao = await db.Questoes.FindAsync(alvoId) ?? throw new NaoEncontradoException("Questão não encontrada.");
            autorId = questao.AutorId;
        }
        else
        {
            prova = await db.Provas.FindAsync(alvoId) ?? throw new NaoEncontradoException("Prova não encontrada.");
            autorId = prova.AutorId;
        }

        if (autorId == uid)
            throw new RegraNegocioException("Você não pode votar no seu próprio conteúdo.");

        var voto = await db.Votos.FirstOrDefaultAsync(v => v.UsuarioId == uid && v.AlvoTipo == alvo && v.AlvoId == alvoId);
        var anterior = voto?.Valor ?? 0;
        var novo = valor == anterior ? 0 : valor;

        if (novo == 0 && voto is not null)
            db.Votos.Remove(voto);
        else if (novo != 0 && voto is null)
            db.Votos.Add(new Voto { UsuarioId = uid, AlvoTipo = alvo, AlvoId = alvoId, Valor = novo });
        else if (voto is not null)
        {
            voto.Valor = novo;
            voto.Data = DateTime.UtcNow;
        }

        var delta = novo - anterior;
        int saldo;
        if (questao is not null) saldo = questao.SaldoVotos += delta;
        else saldo = prova!.SaldoVotos += delta;

        await db.SaveChangesAsync();

        if (novo == 1 && anterior == 0)
            await gamificacao.ConcederAsync(autorId, GamificacaoService.Xp.VotoPositivoRecebido, "Voto positivo recebido");

        return new VotoResultadoDto { SaldoVotos = saldo, MeuVoto = novo };
    }

    public async Task<FavoritoResultadoDto> AlternarFavoritoAsync(AlvoTipo alvo, int alvoId, int uid)
    {
        var existe = alvo == AlvoTipo.Questao
            ? await db.Questoes.AnyAsync(q => q.Id == alvoId)
            : await db.Provas.AnyAsync(p => p.Id == alvoId);
        if (!existe) throw new NaoEncontradoException();

        var favorito = await db.Favoritos.FirstOrDefaultAsync(f => f.UsuarioId == uid && f.AlvoTipo == alvo && f.AlvoId == alvoId);
        if (favorito is null)
            db.Favoritos.Add(new Favorito { UsuarioId = uid, AlvoTipo = alvo, AlvoId = alvoId });
        else
            db.Favoritos.Remove(favorito);
        await db.SaveChangesAsync();

        return new FavoritoResultadoDto { Favorito = favorito is null };
    }
}

public class DenunciaService(ApplicationDbContext db, GamificacaoService gamificacao)
{
    public const int LimiteRevisao = 5;

    public async Task CriarAsync(DenunciaCriarDto dto, int uid)
    {
        var autorAlvo = dto.AlvoTipo == AlvoTipo.Questao
            ? await db.Questoes.Where(q => q.Id == dto.AlvoId).Select(q => (int?)q.AutorId).FirstOrDefaultAsync()
            : await db.Provas.Where(p => p.Id == dto.AlvoId).Select(p => (int?)p.AutorId).FirstOrDefaultAsync();
        if (autorAlvo is null) throw new NaoEncontradoException();
        if (autorAlvo == uid) throw new RegraNegocioException("Você não pode denunciar seu próprio conteúdo.");

        if (await db.Denuncias.AnyAsync(d => d.AutorId == uid && d.AlvoTipo == dto.AlvoTipo && d.AlvoId == dto.AlvoId))
            throw new RegraNegocioException("Você já denunciou este item. Obrigado!");

        db.Denuncias.Add(new Denuncia
        {
            AutorId = uid,
            AlvoTipo = dto.AlvoTipo,
            AlvoId = dto.AlvoId,
            Motivo = dto.Motivo,
            Detalhe = string.IsNullOrWhiteSpace(dto.Detalhe) ? null : dto.Detalhe.Trim()
        });
        await db.SaveChangesAsync();

        var abertas = await db.Denuncias.CountAsync(d => d.AlvoTipo == dto.AlvoTipo && d.AlvoId == dto.AlvoId && d.Status == StatusDenuncia.Aberta);
        if (abertas >= LimiteRevisao)
            await DefinirStatusAlvoAsync(dto.AlvoTipo, dto.AlvoId, StatusItem.EmRevisao, somenteSeAtivo: true);
    }

    public async Task<List<DenunciaDto>> ListarAsync(StatusDenuncia status)
    {
        var denuncias = await db.Denuncias.AsNoTracking()
            .Include(d => d.Autor)
            .Where(d => d.Status == status)
            .OrderBy(d => d.CriadaEm)
            .Take(200)
            .ToListAsync();

        var idsQuestoes = denuncias.Where(d => d.AlvoTipo == AlvoTipo.Questao).Select(d => d.AlvoId).Distinct().ToList();
        var idsProvas = denuncias.Where(d => d.AlvoTipo == AlvoTipo.Prova).Select(d => d.AlvoId).Distinct().ToList();
        var questoes = await db.Questoes.AsNoTracking().Where(q => idsQuestoes.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, q => (Titulo: TextoUtil.Resumo(q.Enunciado, 140), q.Status));
        var provas = await db.Provas.AsNoTracking().Where(p => idsProvas.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => (p.Titulo, p.Status));
        var contagem = await db.Denuncias.AsNoTracking()
            .Where(d => d.Status == StatusDenuncia.Aberta)
            .GroupBy(d => new { d.AlvoTipo, d.AlvoId })
            .Select(g => new { g.Key.AlvoTipo, g.Key.AlvoId, Total = g.Count() })
            .ToListAsync();

        return denuncias.Select(d =>
        {
            var alvo = d.AlvoTipo == AlvoTipo.Questao ? questoes.GetValueOrDefault(d.AlvoId) : provas.GetValueOrDefault(d.AlvoId);
            return new DenunciaDto
            {
                Id = d.Id,
                AlvoTipo = d.AlvoTipo,
                AlvoId = d.AlvoId,
                AlvoTitulo = alvo.Titulo ?? "(removido)",
                AlvoStatus = alvo.Status,
                Motivo = d.Motivo,
                Detalhe = d.Detalhe,
                AutorNome = d.Autor.NomeExibicao,
                CriadaEm = d.CriadaEm,
                Status = d.Status,
                TotalDenunciasAlvo = contagem.FirstOrDefault(c => c.AlvoTipo == d.AlvoTipo && c.AlvoId == d.AlvoId)?.Total ?? 0
            };
        }).ToList();
    }

    public async Task ResolverAsync(int id, DenunciaResolverDto dto)
    {
        var denuncia = await db.Denuncias.FindAsync(id) ?? throw new NaoEncontradoException("Denúncia não encontrada.");
        if (denuncia.Status != StatusDenuncia.Aberta)
            throw new RegraNegocioException("Esta denúncia já foi resolvida.");

        denuncia.Status = dto.Aceitar ? StatusDenuncia.Aceita : StatusDenuncia.Rejeitada;
        denuncia.ResolvidaEm = DateTime.UtcNow;
        await db.SaveChangesAsync();

        if (dto.Aceitar)
        {
            await gamificacao.ConcederAsync(denuncia.AutorId, GamificacaoService.Xp.DenunciaAceita, "Denúncia aceita");
            if (dto.ArquivarItem)
                await DefinirStatusAlvoAsync(denuncia.AlvoTipo, denuncia.AlvoId, StatusItem.Arquivado, somenteSeAtivo: false);
        }

        var restantes = await db.Denuncias.CountAsync(d => d.AlvoTipo == denuncia.AlvoTipo && d.AlvoId == denuncia.AlvoId && d.Status == StatusDenuncia.Aberta);
        if (restantes == 0 && !(dto.Aceitar && dto.ArquivarItem))
            await RestaurarSeEmRevisaoAsync(denuncia.AlvoTipo, denuncia.AlvoId);
    }

    private async Task DefinirStatusAlvoAsync(AlvoTipo tipo, int id, StatusItem status, bool somenteSeAtivo)
    {
        if (tipo == AlvoTipo.Questao)
        {
            var q = await db.Questoes.FindAsync(id);
            if (q is not null && (!somenteSeAtivo || q.Status == StatusItem.Ativo)) q.Status = status;
        }
        else
        {
            var p = await db.Provas.FindAsync(id);
            if (p is not null && (!somenteSeAtivo || p.Status == StatusItem.Ativo)) p.Status = status;
        }
        await db.SaveChangesAsync();
    }

    private async Task RestaurarSeEmRevisaoAsync(AlvoTipo tipo, int id)
    {
        if (tipo == AlvoTipo.Questao)
        {
            var q = await db.Questoes.FindAsync(id);
            if (q?.Status == StatusItem.EmRevisao) q.Status = StatusItem.Ativo;
        }
        else
        {
            var p = await db.Provas.FindAsync(id);
            if (p?.Status == StatusItem.EmRevisao) p.Status = StatusItem.Ativo;
        }
        await db.SaveChangesAsync();
    }
}
