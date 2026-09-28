namespace RankingPreProva.Shared.Dtos;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
}

public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool EhAdmin { get; set; }
}

public class RecompensaDto
{
    public int XpGanho { get; set; }
    public int XpTotal { get; set; }
    public int Nivel { get; set; }
    public bool SubiuNivel { get; set; }
    public int SequenciaDias { get; set; }
    public List<BadgeDto> NovasBadges { get; set; } = [];
}

public class VotoDto
{
    public int Valor { get; set; }
}

public class VotoResultadoDto
{
    public int SaldoVotos { get; set; }
    public int MeuVoto { get; set; }
}

public class FavoritoResultadoDto
{
    public bool Favorito { get; set; }
}

public class CriadoDto
{
    public int Id { get; set; }
    public RecompensaDto? Recompensa { get; set; }
}
