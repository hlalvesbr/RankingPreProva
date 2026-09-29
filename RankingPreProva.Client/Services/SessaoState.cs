using RankingPreProva.Shared.Dtos;

namespace RankingPreProva.Client.Services;

/// <summary>Estado do usuário logado compartilhado entre layout e páginas (XP, nível, sequência).</summary>
public class SessaoState(UsuariosApi api)
{
    public UsuarioDto? Usuario { get; private set; }

    public event Action? Mudou;
    public event Action<RecompensaDto>? RecompensaRecebida;

    public async Task CarregarAsync()
    {
        try
        {
            Usuario = await api.MeAsync();
        }
        catch (ApiException)
        {
            Usuario = null;
        }
        Mudou?.Invoke();
    }

    public void AplicarRecompensa(RecompensaDto? recompensa)
    {
        if (recompensa is null) return;
        if (Usuario is not null)
        {
            Usuario.Xp = recompensa.XpTotal;
            Usuario.Nivel = recompensa.Nivel;
            Usuario.SequenciaDias = recompensa.SequenciaDias;
        }
        Mudou?.Invoke();
        if (recompensa.XpGanho > 0 || recompensa.SubiuNivel || recompensa.NovasBadges.Count > 0)
            RecompensaRecebida?.Invoke(recompensa);
    }
}
