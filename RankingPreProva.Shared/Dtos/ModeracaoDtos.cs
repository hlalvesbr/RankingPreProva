using System.ComponentModel.DataAnnotations;

namespace RankingPreProva.Shared.Dtos;

public class DenunciaCriarDto
{
    public AlvoTipo AlvoTipo { get; set; }
    public int AlvoId { get; set; }
    public MotivoDenuncia Motivo { get; set; } = MotivoDenuncia.GabaritoErrado;

    [MaxLength(1000)]
    public string? Detalhe { get; set; }
}

public class DenunciaDto
{
    public int Id { get; set; }
    public AlvoTipo AlvoTipo { get; set; }
    public int AlvoId { get; set; }
    public string AlvoTitulo { get; set; } = string.Empty;
    public StatusItem AlvoStatus { get; set; }
    public MotivoDenuncia Motivo { get; set; }
    public string? Detalhe { get; set; }
    public string AutorNome { get; set; } = string.Empty;
    public DateTime CriadaEm { get; set; }
    public StatusDenuncia Status { get; set; }
    public int TotalDenunciasAlvo { get; set; }
}

public class DenunciaResolverDto
{
    public bool Aceitar { get; set; }
    public bool ArquivarItem { get; set; }
}
