namespace RankingPreProvaServer.Services;

public class NaoEncontradoException(string mensagem = "Item não encontrado.") : Exception(mensagem);

public class ProibidoException(string mensagem = "Você não tem permissão para esta ação.") : Exception(mensagem);

public class RegraNegocioException(string mensagem) : Exception(mensagem);

public static class Relogio
{
    private static readonly TimeZoneInfo Fuso = ObterFuso();

    private static TimeZoneInfo ObterFuso()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
    }

    public static DateOnly Hoje() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Fuso));

    public static DateOnly DataLocal(DateTime utc) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, Fuso));

    public static DateTime InicioDoDiaUtc(DateOnly dia) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dia.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified), Fuso);
}
