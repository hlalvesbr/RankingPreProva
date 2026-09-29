using System.Net;
using System.Net.Http.Json;

namespace RankingPreProva.Client.Services;

public class ApiException(int status, string mensagem) : Exception(mensagem)
{
    public int Status { get; } = status;
}

public abstract class ApiBase(HttpClient http)
{
    protected HttpClient Http { get; } = http;

    private sealed class ProblemInfo
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
    }

    protected async Task<T> GetAsync<T>(string url)
    {
        using var resp = await Http.GetAsync(url);
        await GarantirSucessoAsync(resp);
        return (await resp.Content.ReadFromJsonAsync<T>())!;
    }

    protected async Task<T> PostAsync<T>(string url, object? corpo = null)
    {
        using var resp = await Http.PostAsJsonAsync(url, corpo ?? new { });
        await GarantirSucessoAsync(resp);
        return (await resp.Content.ReadFromJsonAsync<T>())!;
    }

    protected async Task PostSemRetornoAsync(string url, object? corpo = null)
    {
        using var resp = await Http.PostAsJsonAsync(url, corpo ?? new { });
        await GarantirSucessoAsync(resp);
    }

    protected async Task PutAsync(string url, object corpo)
    {
        using var resp = await Http.PutAsJsonAsync(url, corpo);
        await GarantirSucessoAsync(resp);
    }

    protected async Task DeleteAsync(string url)
    {
        using var resp = await Http.DeleteAsync(url);
        await GarantirSucessoAsync(resp);
    }

    private static async Task GarantirSucessoAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;

        string? mensagem = null;
        try
        {
            var problema = await resp.Content.ReadFromJsonAsync<ProblemInfo>();
            mensagem = problema?.Detail ?? problema?.Title;
        }
        catch
        {
            // Corpo não é ProblemDetails.
        }

        mensagem ??= resp.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Sua sessão expirou. Entre novamente.",
            HttpStatusCode.Forbidden => "Você não tem permissão para esta ação.",
            HttpStatusCode.NotFound => "Item não encontrado.",
            _ => "Ops! Algo deu errado. Tente novamente."
        };
        throw new ApiException((int)resp.StatusCode, mensagem);
    }
}
