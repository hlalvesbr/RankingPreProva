using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using RankingPreProvaClient.Services;

namespace RankingPreProvaClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var cultura = new System.Globalization.CultureInfo("pt-BR");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultura;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultura;

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<QuestoesApi>();
            builder.Services.AddScoped<ProvasApi>();
            builder.Services.AddScoped<UsuariosApi>();
            builder.Services.AddScoped<ModeracaoApi>();
            builder.Services.AddScoped<SessaoState>();

            builder.Services.AddAuthorizationCore(o => o.AddPolicy("Admin", p => p.RequireRole("Admin")));
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

            builder.Services.AddFluentUIComponents();

            await builder.Build().RunAsync();
        }
    }
}
