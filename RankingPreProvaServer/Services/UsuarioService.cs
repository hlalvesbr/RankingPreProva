using Microsoft.EntityFrameworkCore;
using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;
using RankingPreProvaServer.Data;
using RankingPreProvaServer.Models;

namespace RankingPreProvaServer.Services;

public class UsuarioService(ApplicationDbContext db, IConfiguration config, GamificacaoService gamificacao)
{
    public async Task<Usuario> UpsertLoginAsync(string subject, string email, string nome, string? avatarUrl, bool forcarAdmin = false)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.GoogleSubject == subject);
        if (usuario is null)
        {
            usuario = new Usuario { GoogleSubject = subject, NomeExibicao = nome };
            db.Usuarios.Add(usuario);
        }

        usuario.Email = email;
        if (!string.IsNullOrEmpty(avatarUrl))
            usuario.AvatarUrl = avatarUrl;
        usuario.UltimoAcesso = DateTime.UtcNow;

        var admins = config.GetSection("Admin:Emails").Get<string[]>() ?? [];
        if (forcarAdmin || admins.Contains(email, StringComparer.OrdinalIgnoreCase))
            usuario.EhAdmin = true;

        await db.SaveChangesAsync();
        return usuario;
    }

    public async Task<UsuarioDto> ObterAsync(int usuarioId)
    {
        var u = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == usuarioId)
            ?? throw new NaoEncontradoException("Usuário não encontrado.");

        var respostasAvulsas = await db.RespostasAvulsas.Where(r => r.UsuarioId == usuarioId)
            .GroupBy(_ => 1).Select(g => new { Total = g.Count(), Acertos = g.Count(r => r.Correta) }).FirstOrDefaultAsync();
        var respostasProva = await db.Tentativas.Where(t => t.UsuarioId == usuarioId && t.Finalizada != null)
            .GroupBy(_ => 1).Select(g => new { Total = g.Sum(t => t.Acertos + t.Erros), Acertos = g.Sum(t => t.Acertos) }).FirstOrDefaultAsync();

        return new UsuarioDto
        {
            Id = u.Id,
            Nome = u.NomeExibicao,
            Apelido = u.Apelido,
            Email = u.Email,
            AvatarUrl = u.AvatarUrl,
            Bio = u.Bio,
            CargoAlvo = u.CargoAlvo,
            ConcursoAlvo = u.ConcursoAlvo,
            Xp = u.Xp,
            Nivel = u.Nivel,
            SequenciaDias = SequenciaVigente(u),
            MaiorSequencia = u.MaiorSequencia,
            CriadoEm = u.CriadoEm,
            EhAdmin = u.EhAdmin,
            MetaDiaria = u.MetaDiaria,
            TotalQuestoesCriadas = await db.Questoes.CountAsync(q => q.AutorId == usuarioId && q.Status != StatusItem.Arquivado),
            TotalProvasCriadas = await db.Provas.CountAsync(p => p.AutorId == usuarioId && p.Status != StatusItem.Arquivado),
            TotalRespostas = (respostasAvulsas?.Total ?? 0) + (respostasProva?.Total ?? 0),
            TotalAcertos = (respostasAvulsas?.Acertos ?? 0) + (respostasProva?.Acertos ?? 0)
        };
    }

    private static int SequenciaVigente(Usuario u)
    {
        var hoje = Relogio.Hoje();
        return u.UltimaAtividade == hoje || u.UltimaAtividade == hoje.AddDays(-1) ? u.SequenciaDias : 0;
    }

    public async Task AtualizarPerfilAsync(int usuarioId, PerfilEdicaoDto dto)
    {
        var u = await db.Usuarios.FindAsync(usuarioId) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        var apelido = string.IsNullOrWhiteSpace(dto.Apelido) ? null : dto.Apelido.Trim().ToLowerInvariant();

        if (apelido is not null && await db.Usuarios.AnyAsync(x => x.Apelido == apelido && x.Id != usuarioId))
            throw new RegraNegocioException("Este apelido já está em uso.");

        u.NomeExibicao = dto.Nome.Trim();
        u.Apelido = apelido;
        u.Bio = dto.Bio?.Trim();
        u.CargoAlvo = dto.CargoAlvo?.Trim();
        u.ConcursoAlvo = dto.ConcursoAlvo?.Trim();
        u.MetaDiaria = dto.MetaDiaria;
        await db.SaveChangesAsync();
    }

    public async Task<PerfilPublicoDto> PerfilPublicoAsync(string idOuApelido)
    {
        var chave = idOuApelido.Trim().ToLowerInvariant();
        var query = db.Usuarios.AsNoTracking();
        var u = int.TryParse(chave, out var id)
            ? await query.FirstOrDefaultAsync(x => x.Id == id)
            : await query.FirstOrDefaultAsync(x => x.Apelido == chave);
        if (u is null) throw new NaoEncontradoException("Perfil não encontrado.");

        var completo = await ObterAsync(u.Id);
        var badges = await gamificacao.ListarBadgesAsync(u.Id, incluirProgresso: false);

        var provas = await db.Provas.AsNoTracking()
            .Where(p => p.AutorId == u.Id && p.Publica && p.Status != StatusItem.Arquivado)
            .OrderByDescending(p => p.SaldoVotos).ThenByDescending(p => p.Id)
            .Take(6)
            .Select(ProvaService.ProjecaoResumo(db, 0))
            .ToListAsync();

        return new PerfilPublicoDto
        {
            Id = u.Id,
            Nome = u.NomeExibicao,
            Apelido = u.Apelido,
            AvatarUrl = u.AvatarUrl,
            Bio = u.Bio,
            CargoAlvo = u.CargoAlvo,
            ConcursoAlvo = u.ConcursoAlvo,
            Xp = u.Xp,
            Nivel = u.Nivel,
            MaiorSequencia = u.MaiorSequencia,
            CriadoEm = u.CriadoEm,
            TotalQuestoesCriadas = completo.TotalQuestoesCriadas,
            TotalProvasCriadas = completo.TotalProvasCriadas,
            TotalRespostas = completo.TotalRespostas,
            Badges = badges.Where(b => b.Conquistada).ToList(),
            Provas = provas
        };
    }

    public async Task<int?> DesafioDoDiaIdAsync()
    {
        var elegiveis = db.Questoes.Where(q => q.Status == StatusItem.Ativo && q.SaldoVotos >= 0);
        var total = await elegiveis.CountAsync();
        if (total == 0) return null;

        var hoje = Relogio.Hoje();
        var indice = (hoje.DayNumber * 7919) % total;
        return await elegiveis.OrderBy(q => q.Id).Skip(indice).Select(q => (int?)q.Id).FirstOrDefaultAsync();
    }

    public async Task<List<AtividadeDiaDto>> AtividadePorDiaAsync(int usuarioId, int dias)
    {
        var hoje = Relogio.Hoje();
        var inicio = hoje.AddDays(-(dias - 1));
        var inicioUtc = Relogio.InicioDoDiaUtc(inicio);

        var avulsas = await db.RespostasAvulsas.AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId && r.Data >= inicioUtc)
            .Select(r => new { r.Data, r.Correta })
            .ToListAsync();
        var tentativas = await db.Tentativas.AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId && t.Finalizada >= inicioUtc)
            .Select(t => new { Data = t.Finalizada!.Value, Respostas = t.Acertos + t.Erros, t.Acertos })
            .ToListAsync();

        return Enumerable.Range(0, dias).Select(i =>
        {
            var dia = inicio.AddDays(i);
            var a = avulsas.Where(x => Relogio.DataLocal(x.Data) == dia).ToList();
            var t = tentativas.Where(x => Relogio.DataLocal(x.Data) == dia).ToList();
            return new AtividadeDiaDto
            {
                Data = dia,
                Respostas = a.Count + t.Sum(x => x.Respostas),
                Acertos = a.Count(x => x.Correta) + t.Sum(x => x.Acertos)
            };
        }).ToList();
    }

    public async Task<DashboardDto> DashboardAsync(int usuarioId)
    {
        var semana = await AtividadePorDiaAsync(usuarioId, 7);
        var desafio = await DesafioDoDiaIdAsync();
        var inicioHoje = Relogio.InicioDoDiaUtc(Relogio.Hoje());

        var badges = await db.UsuarioBadges.AsNoTracking()
            .Where(ub => ub.UsuarioId == usuarioId)
            .OrderByDescending(ub => ub.ConquistadaEm)
            .Take(4)
            .Include(ub => ub.Badge)
            .ToListAsync();

        var ranking = await RankingGeralAsync("semana", usuarioId, 1);

        return new DashboardDto
        {
            Usuario = await ObterAsync(usuarioId),
            RespostasHoje = semana[^1].Respostas,
            RespostasSemana = semana.Sum(d => d.Respostas),
            AcertosSemana = semana.Sum(d => d.Acertos),
            Semana = semana,
            PosicaoRankingSemanal = ranking.FirstOrDefault(r => r.EhVoce)?.Posicao,
            DesafioDoDiaId = desafio,
            DesafioRespondido = desafio is not null && await db.RespostasAvulsas.AnyAsync(r =>
                r.UsuarioId == usuarioId && r.QuestaoId == desafio && r.Data >= inicioHoje),
            BadgesRecentes = badges.Select(ub => GamificacaoService.ParaDto(ub.Badge, ub, ub.Badge.Meta)).ToList(),
            ProvasEmAlta = await db.Provas.AsNoTracking()
                .Where(p => p.Publica && p.Status == StatusItem.Ativo && p.SaldoVotos >= 0)
                .OrderByDescending(p => p.Tentativas.Count(t => t.Finalizada >= DateTime.UtcNow.AddDays(-14)))
                .ThenByDescending(p => p.SaldoVotos)
                .ThenByDescending(p => p.Id)
                .Take(4)
                .Select(ProvaService.ProjecaoResumo(db, usuarioId))
                .ToListAsync(),
            UltimasTentativas = await db.Tentativas.AsNoTracking()
                .Where(t => t.UsuarioId == usuarioId && t.Finalizada != null)
                .OrderByDescending(t => t.Finalizada)
                .Take(5)
                .Select(TentativaService.ProjecaoResumo)
                .ToListAsync()
        };
    }

    public async Task<HistoricoDto> HistoricoAsync(int usuarioId)
    {
        var avulsas = await db.RespostasAvulsas.AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .SelectMany(r => r.Questao.Assuntos.Select(a => new { a.Nome, r.Correta }))
            .GroupBy(x => x.Nome)
            .Select(g => new { Nome = g.Key, Total = g.Count(), Acertos = g.Count(x => x.Correta) })
            .ToListAsync();
        var emProvas = await db.Tentativas.AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId && t.Finalizada != null)
            .SelectMany(t => t.Respostas.Where(r => r.Letra != null))
            .SelectMany(r => r.Questao.Assuntos.Select(a => new { a.Nome, r.Correta }))
            .GroupBy(x => x.Nome)
            .Select(g => new { Nome = g.Key, Total = g.Count(), Acertos = g.Count(x => x.Correta) })
            .ToListAsync();

        var assuntos = avulsas.Concat(emProvas)
            .GroupBy(x => x.Nome)
            .Select(g => new DesempenhoAssuntoDto { Assunto = g.Key, Respostas = g.Sum(x => x.Total), Acertos = g.Sum(x => x.Acertos) })
            .OrderByDescending(x => x.Respostas)
            .Take(20)
            .ToList();

        return new HistoricoDto
        {
            Assuntos = assuntos,
            Dias = await AtividadePorDiaAsync(usuarioId, 30),
            Tentativas = await db.Tentativas.AsNoTracking()
                .Where(t => t.UsuarioId == usuarioId && t.Finalizada != null)
                .OrderByDescending(t => t.Finalizada)
                .Take(50)
                .Select(TentativaService.ProjecaoResumo)
                .ToListAsync()
        };
    }

    public async Task<List<RankingGeralItemDto>> RankingGeralAsync(string periodo, int usuarioId, int top = 50)
    {
        List<(int UsuarioId, int Xp)> pontuacao;
        if (periodo == "semana")
        {
            var desde = DateTime.UtcNow.AddDays(-7);
            pontuacao = (await db.XpEventos.AsNoTracking()
                    .Where(x => x.Data >= desde)
                    .GroupBy(x => x.UsuarioId)
                    .Select(g => new { UsuarioId = g.Key, Xp = g.Sum(x => x.Quantidade) })
                    .OrderByDescending(x => x.Xp)
                    .ToListAsync())
                .Select(x => (x.UsuarioId, x.Xp)).ToList();
        }
        else
        {
            pontuacao = (await db.Usuarios.AsNoTracking()
                    .Where(u => u.Xp > 0)
                    .OrderByDescending(u => u.Xp)
                    .Select(u => new { u.Id, u.Xp })
                    .ToListAsync())
                .Select(x => (x.Id, x.Xp)).ToList();
        }

        var selecionados = pontuacao.Select((p, i) => (p.UsuarioId, p.Xp, Posicao: i + 1))
            .Where(x => x.Posicao <= top || x.UsuarioId == usuarioId)
            .ToList();
        var ids = selecionados.Select(x => x.UsuarioId).ToList();
        var usuarios = await db.Usuarios.AsNoTracking().Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

        return selecionados.Select(x =>
        {
            var u = usuarios[x.UsuarioId];
            return new RankingGeralItemDto
            {
                Posicao = x.Posicao,
                UsuarioId = u.Id,
                Nome = u.NomeExibicao,
                Apelido = u.Apelido,
                AvatarUrl = u.AvatarUrl,
                Nivel = u.Nivel,
                Xp = x.Xp,
                EhVoce = u.Id == usuarioId
            };
        }).ToList();
    }
}
