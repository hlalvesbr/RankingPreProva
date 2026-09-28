using Microsoft.EntityFrameworkCore;
using RankingPreProvaServer.Api;
using RankingPreProvaServer.Auth;
using RankingPreProvaServer.Components;
using RankingPreProvaServer.Data;
using RankingPreProvaServer.Services;

namespace RankingPreProvaServer
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
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
				.AddAdditionalAssemblies(typeof(RankingPreProvaClient._Imports).Assembly);

			app.Run();
		}
	}
}
