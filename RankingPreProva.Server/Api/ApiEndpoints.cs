using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;
using RankingPreProva.Server.Auth;
using RankingPreProva.Server.Services;

namespace RankingPreProva.Server.Api;

public static class ApiEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api").AddEndpointFilter(TratarErrosAsync);

        // Público: estado de autenticação.
        api.MapGet("/auth/me", (ClaimsPrincipal user) => new UserInfo
        {
            IsAuthenticated = user.Identity?.IsAuthenticated == true && user.UsuarioId() > 0,
            UsuarioId = user.UsuarioId(),
            Nome = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            EhAdmin = user.IsInRole("Admin")
        });

        var auth = api.MapGroup("").RequireAuthorization();

        // Imagens (corpo bruto com o arquivo; GET público com cache imutável)
        api.MapGet("/imagens/{id:guid}", async (Guid id, ImagemService s, HttpContext ctx) =>
        {
            var img = await s.ObterAsync(id);
            if (img is null) return Results.NotFound();
            ctx.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            ctx.Response.Headers.XContentTypeOptions = "nosniff";
            return Results.File(img.Bytes, img.ContentType);
        });
        auth.MapPost("/imagens", (ClaimsPrincipal u, ImagemService s, HttpRequest req) =>
        {
            if (req.ContentLength > ImagemService.TamanhoMaximo)
                throw new RegraNegocioException("A imagem deve ter no máximo 1 MB.");
            return s.SalvarAsync(req.Body, u.UsuarioId());
        }).WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(ImagemService.TamanhoMaximo + 64 * 1024));

        // Usuário
        auth.MapGet("/usuarios/me", (ClaimsPrincipal u, UsuarioService s) => s.ObterAsync(u.UsuarioId()));
        auth.MapPut("/usuarios/me", async (ClaimsPrincipal u, UsuarioService s, PerfilEdicaoDto dto) =>
        {
            Validar(dto);
            await s.AtualizarPerfilAsync(u.UsuarioId(), dto);
            return Results.NoContent();
        });
        auth.MapGet("/usuarios/me/dashboard", (ClaimsPrincipal u, UsuarioService s) => s.DashboardAsync(u.UsuarioId()));
        auth.MapGet("/usuarios/me/historico", (ClaimsPrincipal u, UsuarioService s) => s.HistoricoAsync(u.UsuarioId()));
        auth.MapGet("/usuarios/me/badges", (ClaimsPrincipal u, GamificacaoService s) => s.ListarBadgesAsync(u.UsuarioId()));
        auth.MapGet("/usuarios/{chave}", (string chave, UsuarioService s) => s.PerfilPublicoAsync(chave));
        auth.MapGet("/ranking-geral", (ClaimsPrincipal u, UsuarioService s, string? periodo) =>
            s.RankingGeralAsync(periodo == "semana" ? "semana" : "total", u.UsuarioId()));

        // Questões
        var questoes = auth.MapGroup("/questoes");
        questoes.MapGet("", (ClaimsPrincipal u, QuestaoService s, [AsParameters] QuestaoFiltroQuery q) => s.ListarAsync(q.ParaFiltro(), u.UsuarioId()));
        questoes.MapGet("/filtros", (QuestaoService s) => s.FiltrosAsync());
        questoes.MapGet("/assuntos", (QuestaoService s, string? termo) => s.SugerirAssuntosAsync(termo));
        questoes.MapGet("/desafio-do-dia", async (UsuarioService s) => Results.Ok(new { Id = await s.DesafioDoDiaIdAsync() }));
        questoes.MapGet("/{id:int}", (int id, ClaimsPrincipal u, QuestaoService s) => s.ObterAsync(id, u.UsuarioId()));
        questoes.MapGet("/{id:int}/edicao", (int id, ClaimsPrincipal u, QuestaoService s) => s.ObterEdicaoAsync(id, u.UsuarioId()));
        questoes.MapPost("", (ClaimsPrincipal u, QuestaoService s, QuestaoEdicaoDto dto) =>
        {
            Validar(dto);
            return s.CriarAsync(dto, u.UsuarioId());
        });
        questoes.MapPut("/{id:int}", async (int id, ClaimsPrincipal u, QuestaoService s, QuestaoEdicaoDto dto) =>
        {
            Validar(dto);
            await s.AtualizarAsync(id, dto, u.UsuarioId());
            return Results.NoContent();
        });
        questoes.MapDelete("/{id:int}", async (int id, ClaimsPrincipal u, QuestaoService s) =>
        {
            await s.ExcluirAsync(id, u.UsuarioId());
            return Results.NoContent();
        });
        questoes.MapPost("/{id:int}/responder", (int id, ClaimsPrincipal u, QuestaoService s, ResponderQuestaoDto dto) =>
            s.ResponderAsync(id, dto.Letra, u.UsuarioId()));
        questoes.MapPost("/{id:int}/voto", (int id, ClaimsPrincipal u, VotoService s, VotoDto dto) =>
            s.VotarAsync(AlvoTipo.Questao, id, dto.Valor, u.UsuarioId()));
        questoes.MapPost("/{id:int}/favorito", (int id, ClaimsPrincipal u, VotoService s) =>
            s.AlternarFavoritoAsync(AlvoTipo.Questao, id, u.UsuarioId()));

        // Provas
        var provas = auth.MapGroup("/provas");
        provas.MapGet("", (ClaimsPrincipal u, ProvaService s, string? busca, string? banca, bool? somenteMinhas, bool? somenteFavoritas, string? ordem, int? page, int? pageSize) =>
            s.ListarAsync(new ProvaFiltro
            {
                Busca = busca,
                Banca = banca,
                SomenteMinhas = somenteMinhas == true,
                SomenteFavoritas = somenteFavoritas == true,
                Ordem = ordem ?? "recentes",
                Page = page ?? 1,
                PageSize = pageSize ?? 12
            }, u.UsuarioId()));
        provas.MapGet("/{id:int}", (int id, ClaimsPrincipal u, ProvaService s) => s.ObterAsync(id, u.UsuarioId()));
        provas.MapGet("/{id:int}/edicao", (int id, ClaimsPrincipal u, ProvaService s) => s.ObterEdicaoAsync(id, u.UsuarioId()));
        provas.MapGet("/{id:int}/ranking", (int id, ClaimsPrincipal u, RankingService s) => s.RankingProvaAsync(id, u.UsuarioId()));
        provas.MapPost("", (ClaimsPrincipal u, ProvaService s, ProvaEdicaoDto dto) =>
        {
            Validar(dto);
            return s.CriarAsync(dto, u.UsuarioId());
        });
        provas.MapPut("/{id:int}", async (int id, ClaimsPrincipal u, ProvaService s, ProvaEdicaoDto dto) =>
        {
            Validar(dto);
            await s.AtualizarAsync(id, dto, u.UsuarioId());
            return Results.NoContent();
        });
        provas.MapDelete("/{id:int}", async (int id, ClaimsPrincipal u, ProvaService s) =>
        {
            await s.ExcluirAsync(id, u.UsuarioId());
            return Results.NoContent();
        });
        provas.MapPost("/{id:int}/tentativas", (int id, ClaimsPrincipal u, TentativaService s) => s.IniciarAsync(id, u.UsuarioId()));
        provas.MapPost("/{id:int}/voto", (int id, ClaimsPrincipal u, VotoService s, VotoDto dto) =>
            s.VotarAsync(AlvoTipo.Prova, id, dto.Valor, u.UsuarioId()));
        provas.MapPost("/{id:int}/favorito", (int id, ClaimsPrincipal u, VotoService s) =>
            s.AlternarFavoritoAsync(AlvoTipo.Prova, id, u.UsuarioId()));

        // Tentativas
        auth.MapGet("/tentativas/{id:int}", (int id, ClaimsPrincipal u, TentativaService s) => s.ObterResultadoAsync(id, u.UsuarioId()));
        auth.MapPost("/tentativas/{id:int}/finalizar", (int id, ClaimsPrincipal u, TentativaService s, FinalizarTentativaDto dto) =>
            s.FinalizarAsync(id, dto, u.UsuarioId()));

        // Denúncias
        auth.MapPost("/denuncias", async (ClaimsPrincipal u, DenunciaService s, DenunciaCriarDto dto) =>
        {
            Validar(dto);
            await s.CriarAsync(dto, u.UsuarioId());
            return Results.NoContent();
        });

        // Administração
        var admin = auth.MapGroup("/admin").RequireAuthorization(AuthSetup.PolicyAdmin);
        admin.MapGet("/denuncias", (DenunciaService s, StatusDenuncia? status) => s.ListarAsync(status ?? StatusDenuncia.Aberta));
        admin.MapPost("/denuncias/{id:int}/resolver", async (int id, DenunciaService s, DenunciaResolverDto dto) =>
        {
            await s.ResolverAsync(id, dto);
            return Results.NoContent();
        });
    }

    private static void Validar(object dto)
    {
        var resultados = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), resultados, validateAllProperties: true))
            throw new RegraNegocioException(string.Join(" ", resultados.Select(r => r.ErrorMessage).Distinct()));
    }

    private static async ValueTask<object?> TratarErrosAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        try
        {
            return await next(ctx);
        }
        catch (NaoEncontradoException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (ProibidoException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (RegraNegocioException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}

public class QuestaoFiltroQuery
{
    public string? Busca { get; set; }
    public string? Assunto { get; set; }
    public string? Banca { get; set; }
    public int? Ano { get; set; }
    public string? Concurso { get; set; }
    public string? Cargo { get; set; }
    public TipoQuestao? Tipo { get; set; }
    public OrigemQuestao? Origem { get; set; }
    public Dificuldade? Dificuldade { get; set; }
    public bool? SomenteMinhas { get; set; }
    public bool? SomenteFavoritas { get; set; }
    public bool? NaoRespondidas { get; set; }
    public string? Ordem { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }

    public QuestaoFiltro ParaFiltro() => new()
    {
        Busca = Busca,
        Assunto = Assunto,
        Banca = Banca,
        Ano = Ano,
        Concurso = Concurso,
        Cargo = Cargo,
        Tipo = Tipo,
        Origem = Origem,
        Dificuldade = Dificuldade,
        SomenteMinhas = SomenteMinhas == true,
        SomenteFavoritas = SomenteFavoritas == true,
        NaoRespondidas = NaoRespondidas == true,
        Ordem = Ordem ?? "recentes",
        Page = Page ?? 1,
        PageSize = PageSize ?? 15
    };
}
