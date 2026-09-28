using System.ComponentModel.DataAnnotations;

namespace RankingPreProva.Shared.Dtos;

public class ProvaResumoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? ConcursoSimulado { get; set; }
    public string? Banca { get; set; }
    public string? Cargo { get; set; }
    public int TotalQuestoes { get; set; }
    public int TotalParticipantes { get; set; }
    public int SaldoVotos { get; set; }
    public int MeuVoto { get; set; }
    public int AutorId { get; set; }
    public string AutorNome { get; set; } = string.Empty;
    public int? TempoLimiteMin { get; set; }
    public RegraPontuacao Regra { get; set; }
    public StatusItem Status { get; set; }
    public bool Publica { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class ProvaDto : ProvaResumoDto
{
    public string? Descricao { get; set; }
    public string? Orgao { get; set; }
    public int? Ano { get; set; }
    public decimal PontosAcerto { get; set; }
    public decimal PenalidadeErro { get; set; }
    public bool NotaMinimaZero { get; set; }
    public bool EhAutor { get; set; }
    public bool PossuiTentativas { get; set; }
    public bool Favorita { get; set; }
    public int? MinhaTentativaEmAndamento { get; set; }
    public TentativaResumoDto? MinhaPrimeira { get; set; }
    public List<string> Assuntos { get; set; } = [];
}

public class ProvaQuestaoItemDto
{
    public int QuestaoId { get; set; }
    public decimal Peso { get; set; } = 1;
    public string Enunciado { get; set; } = string.Empty;
    public TipoQuestao Tipo { get; set; }
    public string? Banca { get; set; }
    public int? Ano { get; set; }
    public List<string> Assuntos { get; set; } = [];
}

public class ProvaEdicaoDto : IValidatableObject
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(4000)] public string? Descricao { get; set; }
    [MaxLength(200)] public string? ConcursoSimulado { get; set; }
    [MaxLength(120)] public string? Banca { get; set; }
    [MaxLength(200)] public string? Cargo { get; set; }
    [MaxLength(200)] public string? Orgao { get; set; }

    [Range(1950, 2100, ErrorMessage = "Ano inválido.")]
    public int? Ano { get; set; }

    [Range(1, 600, ErrorMessage = "Tempo entre 1 e 600 minutos.")]
    public int? TempoLimiteMin { get; set; }

    public bool Publica { get; set; } = true;
    public RegraPontuacao Regra { get; set; } = RegraPontuacao.Simples;

    [Range(0.01, 100)] public decimal PontosAcerto { get; set; } = 1;
    [Range(0, 100)] public decimal PenalidadeErro { get; set; } = 1;
    public bool NotaMinimaZero { get; set; } = true;

    public List<ProvaQuestaoItemDto> Questoes { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Questoes.Count == 0)
            yield return new ValidationResult("Adicione ao menos uma questão.", [nameof(Questoes)]);
        if (Questoes.Count > 200)
            yield return new ValidationResult("Máximo de 200 questões por prova.", [nameof(Questoes)]);
        if (Questoes.Select(q => q.QuestaoId).Distinct().Count() != Questoes.Count)
            yield return new ValidationResult("Há questões repetidas.", [nameof(Questoes)]);
        if (Regra == RegraPontuacao.PesoPersonalizado && Questoes.Any(q => q.Peso <= 0))
            yield return new ValidationResult("Pesos devem ser maiores que zero.", [nameof(Questoes)]);
    }
}

public class ProvaFiltro
{
    public string? Busca { get; set; }
    public string? Banca { get; set; }
    public bool SomenteMinhas { get; set; }
    public bool SomenteFavoritas { get; set; }
    public string Ordem { get; set; } = "recentes";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public string ToQueryString()
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(Busca)) partes.Add("busca=" + Uri.EscapeDataString(Busca));
        if (!string.IsNullOrWhiteSpace(Banca)) partes.Add("banca=" + Uri.EscapeDataString(Banca));
        if (SomenteMinhas) partes.Add("somenteMinhas=true");
        if (SomenteFavoritas) partes.Add("somenteFavoritas=true");
        partes.Add("ordem=" + Ordem);
        partes.Add("page=" + Page);
        partes.Add("pageSize=" + PageSize);
        return "?" + string.Join('&', partes);
    }
}

public class QuestaoProvaDto
{
    public int QuestaoId { get; set; }
    public int Ordem { get; set; }
    public string Enunciado { get; set; } = string.Empty;
    public TipoQuestao Tipo { get; set; }
    public List<AlternativaDto> Alternativas { get; set; } = [];
    public string? Banca { get; set; }
    public int? Ano { get; set; }
    public decimal Peso { get; set; }
}

public class TentativaIniciadaDto
{
    public int TentativaId { get; set; }
    public int ProvaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int? TempoLimiteMin { get; set; }
    public DateTime Iniciada { get; set; }
    public bool EhPrimeira { get; set; }
    public RegraPontuacao Regra { get; set; }
    public List<QuestaoProvaDto> Questoes { get; set; } = [];
}

public class FinalizarTentativaDto
{
    public Dictionary<int, char?> Respostas { get; set; } = [];
}

public class TentativaResumoDto
{
    public int TentativaId { get; set; }
    public int ProvaId { get; set; }
    public string ProvaTitulo { get; set; } = string.Empty;
    public decimal Nota { get; set; }
    public decimal NotaMaxima { get; set; }
    public int Acertos { get; set; }
    public int Erros { get; set; }
    public int EmBranco { get; set; }
    public int DuracaoSeg { get; set; }
    public bool EhPrimeira { get; set; }
    public DateTime Finalizada { get; set; }
    public double Percentual => NotaMaxima == 0 ? 0 : (double)(Nota / NotaMaxima * 100);
}

public class RevisaoItemDto
{
    public int Ordem { get; set; }
    public int QuestaoId { get; set; }
    public string Enunciado { get; set; } = string.Empty;
    public TipoQuestao Tipo { get; set; }
    public List<AlternativaDto> Alternativas { get; set; } = [];
    public char? Marcada { get; set; }
    public char Gabarito { get; set; }
    public bool Correta { get; set; }
    public string? Explicacao { get; set; }
}

public class TentativaResultadoDto : TentativaResumoDto
{
    public int? PosicaoRanking { get; set; }
    public int TotalParticipantes { get; set; }
    public RecompensaDto? Recompensa { get; set; }
    public List<RevisaoItemDto> Itens { get; set; } = [];
}

public class RankingItemDto
{
    public int Posicao { get; set; }
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? AvatarUrl { get; set; }
    public int Nivel { get; set; }
    public decimal Nota { get; set; }
    public int Acertos { get; set; }
    public int DuracaoSeg { get; set; }
    public DateTime Data { get; set; }
    public bool EhVoce { get; set; }
}

public class RankingProvaDto
{
    public decimal NotaMaxima { get; set; }
    public int TotalParticipantes { get; set; }
    public List<RankingItemDto> Itens { get; set; } = [];
    public RankingItemDto? Voce { get; set; }
}
