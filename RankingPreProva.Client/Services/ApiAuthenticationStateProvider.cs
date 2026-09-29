using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RankingPreProva.Shared.Dtos;

namespace RankingPreProva.Client.Services;

public class ApiAuthenticationStateProvider(UsuariosApi api) : AuthenticationStateProvider
{
    private Task<AuthenticationState>? _estado;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _estado ??= CarregarAsync();

    private async Task<AuthenticationState> CarregarAsync()
    {
        UserInfo info;
        try
        {
            info = await api.AuthAsync();
        }
        catch
        {
            info = new UserInfo();
        }

        if (!info.IsAuthenticated)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, info.UsuarioId.ToString()),
            new(ClaimTypes.Name, info.Nome),
            new(ClaimTypes.Email, info.Email)
        };
        if (info.EhAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie")));
    }
}
