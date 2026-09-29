using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using RankingPreProva.Server.Services;

namespace RankingPreProva.Server.Auth;

public static class AuthSetup
{
    public const string ClaimUsuarioId = "rpp:uid";
    public const string PolicyAdmin = "Admin";

    public static bool GoogleConfigurado(IConfiguration config) =>
        !string.IsNullOrWhiteSpace(config["Authentication:Google:ClientId"]) &&
        !string.IsNullOrWhiteSpace(config["Authentication:Google:ClientSecret"]);

    public static int UsuarioId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimUsuarioId), out var id) ? id : 0;

    public static int? UsuarioIdOuNulo(this ClaimsPrincipal user)
    {
        var id = user.UsuarioId();
        return id == 0 ? null : id;
    }

    public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
    {
        var auth = builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.Cookie.Name = "rpp.auth";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.ExpireTimeSpan = TimeSpan.FromDays(30);
                o.SlidingExpiration = true;
                o.LoginPath = "/account/login";
                o.Events.OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
                o.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        if (GoogleConfigurado(builder.Configuration))
        {
            auth.AddGoogle(o =>
            {
                o.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                o.SaveTokens = false;
                o.ClaimActions.MapJsonKey("picture", "picture");
                o.Events.OnCreatingTicket = async ctx =>
                {
                    var principal = ctx.Principal!;
                    var usuarios = ctx.HttpContext.RequestServices.GetRequiredService<UsuarioService>();
                    var usuario = await usuarios.UpsertLoginAsync(
                        principal.FindFirstValue(ClaimTypes.NameIdentifier)!,
                        principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                        principal.FindFirstValue(ClaimTypes.Name) ?? "Concurseiro",
                        principal.FindFirstValue("picture"));
                    AdicionarClaims((ClaimsIdentity)principal.Identity!, usuario.Id, usuario.EhAdmin);
                };
            });
        }

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(PolicyAdmin, p => p.RequireRole("Admin"));

        return builder;
    }

    private static void AdicionarClaims(ClaimsIdentity identity, int usuarioId, bool ehAdmin)
    {
        identity.AddClaim(new Claim(ClaimUsuarioId, usuarioId.ToString()));
        if (ehAdmin)
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
    }

    private static string UrlLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") ? returnUrl : "/";

    public static void MapAccountEndpoints(this WebApplication app)
    {
        var googleOk = GoogleConfigurado(app.Configuration);
        var devLogin = !googleOk && app.Environment.IsDevelopment();

        app.MapGet("/account/login", (string? returnUrl) =>
        {
            var destino = UrlLocal(returnUrl);
            if (googleOk)
                return Results.Challenge(new AuthenticationProperties { RedirectUri = destino }, [GoogleDefaults.AuthenticationScheme]);
            if (devLogin)
                return Results.Redirect($"/account/login-dev?returnUrl={Uri.EscapeDataString(destino)}");
            return Results.Problem("Login com Google não configurado.", statusCode: 503);
        });

        if (devLogin)
        {
            // Login simulado para desenvolvimento local sem credenciais do Google.
            app.MapGet("/account/login-dev", async (HttpContext http, UsuarioService usuarios, string? returnUrl, int? n) =>
            {
                var numero = n ?? 1;
                var usuario = await usuarios.UpsertLoginAsync($"dev-{numero}", $"dev{numero}@localhost", $"Usuário Dev {numero}", null, forcarAdmin: numero == 1);
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.GoogleSubject));
                identity.AddClaim(new Claim(ClaimTypes.Name, usuario.NomeExibicao));
                identity.AddClaim(new Claim(ClaimTypes.Email, usuario.Email));
                AdicionarClaims(identity, usuario.Id, usuario.EhAdmin);
                await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                return Results.Redirect(UrlLocal(returnUrl));
            });
        }

        app.MapGet("/account/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });
    }
}
