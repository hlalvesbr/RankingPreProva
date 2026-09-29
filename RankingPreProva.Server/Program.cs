using Microsoft.EntityFrameworkCore;
using RankingPreProva.Server.Api;
using RankingPreProva.Server.Auth;
using RankingPreProva.Server.Components;
using RankingPreProva.Server.Data;
using RankingPreProva.Server.Services;

namespace RankingPreProva.Server
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddRazorComponents()
				.AddInteractiveWebAssemblyComponents();

			// Configure the PostgreSQL database context.
			var connectionString = ConverterDatabaseUrl(builder.Configuration["DATABASE_URL"])
				?? builder.Configuration.GetConnectionString("DefaultConnection");
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseNpgsql(connectionString));

			builder.AddAppAuthentication();
			builder.Services.AddCascadingAuthenticationState();
			builder.Services.AddProblemDetails();

			builder.Services.AddScoped<UsuarioService>();
			builder.Services.AddScoped<GamificacaoService>();
			builder.Services.AddScoped<QuestaoService>();
			builder.Services.AddScoped<ProvaService>();
			builder.Services.AddScoped<TentativaService>();
			builder.Services.AddScoped<RankingService>();
			builder.Services.AddScoped<VotoService>();
			builder.Services.AddScoped<DenunciaService>();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
			app.UseHttpsRedirection();

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseAntiforgery();

			app.MapStaticAssets();
			app.MapAccountEndpoints();
			app.MapApiEndpoints();

			app.MapRazorComponents<App>()
				.AddInteractiveWebAssemblyRenderMode()
				.AddAdditionalAssemblies(typeof(RankingPreProva.Client._Imports).Assembly);

			app.Run();
		}

		// Heroku fornece DATABASE_URL no formato postgres://usuario:senha@host:porta/banco
		private static string? ConverterDatabaseUrl(string? databaseUrl)
		{
			if (string.IsNullOrWhiteSpace(databaseUrl))
				return null;

			var uri = new Uri(databaseUrl);
			var credenciais = uri.UserInfo.Split(':', 2);
			return $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={uri.AbsolutePath.TrimStart('/')};" +
				$"Username={Uri.UnescapeDataString(credenciais[0])};Password={Uri.UnescapeDataString(credenciais[1])};" +
				"SSL Mode=Require;Trust Server Certificate=true";
		}
	}
}
