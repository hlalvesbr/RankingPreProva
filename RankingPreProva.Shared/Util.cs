using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace RankingPreProva.Shared;

public static class TextoUtil
{
    public static string Slug(string texto)
    {
        var normalizado = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        var ultimoHifen = false;
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                ultimoHifen = false;
            }
            else if (!ultimoHifen && sb.Length > 0)
            {
                sb.Append('-');
                ultimoHifen = true;
            }
        }
        return sb.ToString().Trim('-');
    }

    public static string Resumo(string? texto, int max = 180)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
        texto = texto.ReplaceLineEndings(" ").Trim();
        return texto.Length <= max ? texto : texto[..max].TrimEnd() + "…";
    }

    public static string Nome<TEnum>(this TEnum valor) where TEnum : struct, Enum
    {
        var membro = typeof(TEnum).GetMember(valor.ToString()).FirstOrDefault();
        return membro?.GetCustomAttribute<DisplayAttribute>()?.Name ?? valor.ToString();
    }

    public static string Duracao(int segundos)
    {
        var t = TimeSpan.FromSeconds(segundos);
        return t.TotalHours >= 1 ? $"{(int)t.TotalHours}h{t.Minutes:00}m" : $"{t.Minutes}m{t.Seconds:00}s";
    }
}

public static class Niveis
{
    /// <summary>XP total necessário para atingir o nível informado (nível 1 = 0 XP).</summary>
    public static int XpParaNivel(int nivel) => nivel <= 1 ? 0 : (int)Math.Round(100 * Math.Pow(nivel - 1, 1.5));

    public static int NivelPorXp(int xp)
    {
        var nivel = 1;
        while (XpParaNivel(nivel + 1) <= xp) nivel++;
        return nivel;
    }

    public static string Titulo(int nivel) => nivel switch
    {
        < 3 => "Calouro",
        < 6 => "Estudante",
        < 10 => "Concurseiro",
        < 15 => "Aprovável",
        < 20 => "Classificado",
        < 30 => "Nomeado",
        _ => "Lenda dos Concursos"
    };
}

public static class Letras
{
    public static readonly char[] MultiplaEscolha = ['A', 'B', 'C', 'D', 'E'];
    public static readonly char[] CertoErrado = ['C', 'E'];
    public const int MinimoAlternativas = 4;
    public const int MaximoAlternativas = 5;

    public static char[] Para(TipoQuestao tipo) => tipo == TipoQuestao.CertoErrado ? CertoErrado : MultiplaEscolha;
}
