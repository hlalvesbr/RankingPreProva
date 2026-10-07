using RankingPreProva.Shared;
using RankingPreProva.Shared.Dtos;

namespace RankingPreProva.Client.Services;

public class QuestoesApi(HttpClient http) : ApiBase(http)
{
    public Task<PagedResult<QuestaoResumoDto>> ListarAsync(QuestaoFiltro filtro) => GetAsync<PagedResult<QuestaoResumoDto>>("api/questoes" + filtro.ToQueryString());
    public Task<QuestaoDto> ObterAsync(int id) => GetAsync<QuestaoDto>($"api/questoes/{id}");
    public Task<QuestaoEdicaoDto> ObterEdicaoAsync(int id) => GetAsync<QuestaoEdicaoDto>($"api/questoes/{id}/edicao");
    public Task<CriadoDto> CriarAsync(QuestaoEdicaoDto dto) => PostAsync<CriadoDto>("api/questoes", dto);
    public Task AtualizarAsync(int id, QuestaoEdicaoDto dto) => PutAsync($"api/questoes/{id}", dto);
    public Task ExcluirAsync(int id) => DeleteAsync($"api/questoes/{id}");
    public Task<RespostaResultadoDto> ResponderAsync(int id, char letra) => PostAsync<RespostaResultadoDto>($"api/questoes/{id}/responder", new ResponderQuestaoDto { Letra = letra });
    public Task<VotoResultadoDto> VotarAsync(int id, int valor) => PostAsync<VotoResultadoDto>($"api/questoes/{id}/voto", new VotoDto { Valor = valor });
    public Task<FavoritoResultadoDto> FavoritarAsync(int id) => PostAsync<FavoritoResultadoDto>($"api/questoes/{id}/favorito");
    public Task<FiltrosDisponiveisDto> FiltrosAsync() => GetAsync<FiltrosDisponiveisDto>("api/questoes/filtros");
    public Task<List<AssuntoDto>> SugerirAssuntosAsync(string? termo) => GetAsync<List<AssuntoDto>>($"api/questoes/assuntos?termo={Uri.EscapeDataString(termo ?? "")}");
}

public class ProvasApi(HttpClient http) : ApiBase(http)
{
    public Task<PagedResult<ProvaResumoDto>> ListarAsync(ProvaFiltro filtro) => GetAsync<PagedResult<ProvaResumoDto>>("api/provas" + filtro.ToQueryString());
    public Task<ProvaDto> ObterAsync(int id) => GetAsync<ProvaDto>($"api/provas/{id}");
    public Task<ProvaEdicaoDto> ObterEdicaoAsync(int id) => GetAsync<ProvaEdicaoDto>($"api/provas/{id}/edicao");
    public Task<RankingProvaDto> RankingAsync(int id) => GetAsync<RankingProvaDto>($"api/provas/{id}/ranking");
    public Task<CriadoDto> CriarAsync(ProvaEdicaoDto dto) => PostAsync<CriadoDto>("api/provas", dto);
    public Task AtualizarAsync(int id, ProvaEdicaoDto dto) => PutAsync($"api/provas/{id}", dto);
    public Task ExcluirAsync(int id) => DeleteAsync($"api/provas/{id}");
    public Task<TentativaIniciadaDto> IniciarTentativaAsync(int id) => PostAsync<TentativaIniciadaDto>($"api/provas/{id}/tentativas");
    public Task<TentativaResultadoDto> FinalizarTentativaAsync(int tentativaId, FinalizarTentativaDto dto) => PostAsync<TentativaResultadoDto>($"api/tentativas/{tentativaId}/finalizar", dto);
    public Task<TentativaResultadoDto> ObterResultadoAsync(int tentativaId) => GetAsync<TentativaResultadoDto>($"api/tentativas/{tentativaId}");
    public Task<VotoResultadoDto> VotarAsync(int id, int valor) => PostAsync<VotoResultadoDto>($"api/provas/{id}/voto", new VotoDto { Valor = valor });
    public Task<FavoritoResultadoDto> FavoritarAsync(int id) => PostAsync<FavoritoResultadoDto>($"api/provas/{id}/favorito");
}

public class UsuariosApi(HttpClient http) : ApiBase(http)
{
    public Task<UserInfo> AuthAsync() => GetAsync<UserInfo>("api/auth/me");
    public Task<UsuarioDto> MeAsync() => GetAsync<UsuarioDto>("api/usuarios/me");
    public Task AtualizarPerfilAsync(PerfilEdicaoDto dto) => PutAsync("api/usuarios/me", dto);
    public Task<ImagemEnviadaDto> AlterarAvatarAsync(byte[] bytes, string contentType)
    {
        var conteudo = new ByteArrayContent(bytes);
        conteudo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        return PostConteudoAsync<ImagemEnviadaDto>("api/usuarios/me/avatar", conteudo);
    }
    public Task<DashboardDto> DashboardAsync() => GetAsync<DashboardDto>("api/usuarios/me/dashboard");
    public Task<HistoricoDto> HistoricoAsync() => GetAsync<HistoricoDto>("api/usuarios/me/historico");
    public Task<List<BadgeDto>> BadgesAsync() => GetAsync<List<BadgeDto>>("api/usuarios/me/badges");
    public Task<PerfilPublicoDto> PerfilPublicoAsync(string chave) => GetAsync<PerfilPublicoDto>($"api/usuarios/{Uri.EscapeDataString(chave)}");
    public Task<List<RankingGeralItemDto>> RankingGeralAsync(string periodo) => GetAsync<List<RankingGeralItemDto>>($"api/ranking-geral?periodo={periodo}");
}

public class ImagensApi(HttpClient http) : ApiBase(http)
{
    public Task<ImagemEnviadaDto> EnviarAsync(byte[] bytes, string contentType)
    {
        var conteudo = new ByteArrayContent(bytes);
        conteudo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        return PostConteudoAsync<ImagemEnviadaDto>("api/imagens", conteudo);
    }
}

public class ModeracaoApi(HttpClient http) : ApiBase(http)
{
    public Task DenunciarAsync(DenunciaCriarDto dto) => PostSemRetornoAsync("api/denuncias", dto);
    public Task<List<DenunciaDto>> ListarAsync(StatusDenuncia status) => GetAsync<List<DenunciaDto>>($"api/admin/denuncias?status={status}");
    public Task ResolverAsync(int id, DenunciaResolverDto dto) => PostSemRetornoAsync($"api/admin/denuncias/{id}/resolver", dto);
}
