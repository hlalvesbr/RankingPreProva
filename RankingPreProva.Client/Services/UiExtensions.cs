using Microsoft.FluentUI.AspNetCore.Components;
using RankingPreProva.Shared;

namespace RankingPreProva.Client.Services;

public static class UiExtensions
{
    public static async Task<int> UsuarioIdAsync(this Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState>? estado)
    {
        if (estado is null) return 0;
        var valor = (await estado).User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out var id) ? id : 0;
    }

    public static void Erro(this IToastService toast, Exception ex) =>
        toast.ShowError(ex is ApiException ? ex.Message : "Não foi possível completar a ação. Tente novamente.");

    public static string Pct(double valor) => valor.ToString("0") + "%";

    public static string Nota(decimal valor) => valor.ToString("0.##");

    public static string DataCurta(DateTime utc) => utc.ToLocalTime().ToString("dd/MM/yyyy");

    public static string DataHora(DateTime utc) => utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    public static string Medalha(int posicao) => posicao switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => $"{posicao}º"
    };

    public static string ClasseSaldo(int saldo, StatusItem status = StatusItem.Ativo) =>
        (saldo < 0 ? "item-negativo " : "") + (status == StatusItem.EmRevisao ? "item-revisao" : "");
}
