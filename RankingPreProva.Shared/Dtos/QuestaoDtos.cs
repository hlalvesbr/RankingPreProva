using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RankingPreProva.Shared.Dtos;

public class AlternativaDto
{
    public char Letra { get; set; }

    [Required(ErrorMessage = "Preencha o texto da alternativa.")]
    [MaxLength(2000)]
    public string Texto { get; set; } = string.Empty;
}

public class QuestaoResumoDto
{
    public int Id { get; set; }
    public string Enunciado { get; set; } = string.Empty;
    public TipoQuestao Tipo { get; set; }
    public int? Ano { get; set; }
    public OrigemQuestao Origem { get; set; }
    public string? Banca { get; set; }
    public string? Concurso { get; set; }
    public string? Cargo { get; set; }
    public Dificuldade Dificuldade { get; set; }
    public List<string> Assuntos { get; set; } = [];
    public int SaldoVotos { get; set; }
    public int MeuVoto { get; set; }
    public int AutorId { get; set; }
    public string AutorNome { get; set; } = string.Empty;
    public StatusItem Status { get; set; }
    public int TotalRespostas { get; set; }
    public int TotalAcertos { get; set; }
    public double PercentualAcerto => TotalRespostas == 0 ? 0 : 100.0 * TotalAcertos / TotalRespostas;
    public bool JaRespondi { get; set; }
}

public class QuestaoDto : QuestaoResumoDto
{
    public string? Orgao { get; set; }
    public List<AlternativaDto> Alternativas { get; set; } = [];
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public bool EhAutor { get; set; }
    public bool Favorita { get; set; }
    public int TotalDenunciasAbertas { get; set; }
}

public class QuestaoEdicaoDto : IValidatableObject
{
    [Required(ErrorMessage = "O enunciado é obrigatório.")]
    [MinLength(10, ErrorMessage = "O enunciado deve ter ao menos 10 caracteres.")]
    [MaxLength(8000)]
    public string Enunciado { get; set; } = string.Empty;

    public TipoQuestao Tipo { get; set; } = TipoQuestao.MultiplaEscolha;

    public List<AlternativaDto> Alternativas { get; set; } = [];

    [Required(ErrorMessage = "Informe o gabarito.")]
    public char? Gabarito { get; set; }

    [MaxLength(8000)]
    public string? Explicacao { get; set; }

    [Range(1950, 2100, ErrorMessage = "Ano inválido.")]
    public int? Ano { get; set; } = DateTime.Today.Year;

    public OrigemQuestao Origem { get; set; } = OrigemQuestao.Usuario;

    [MaxLength(120)] public string? Banca { get; set; }
    [MaxLength(200)] public string? Concurso { get; set; }
    [MaxLength(200)] public string? Cargo { get; set; }
    [MaxLength(200)] public string? Orgao { get; set; }

    public Dificuldade Dificuldade { get; set; } = Dificuldade.Media;

    public List<string> Assuntos { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var letras = Letras.Para(Tipo);
        if (Tipo == TipoQuestao.MultiplaEscolha)
        {
            if (Alternativas.Count != letras.Length || Alternativas.Any(a => string.IsNullOrWhiteSpace(a.Texto)))
                yield return new ValidationResult("Preencha as 5 alternativas (A a E).", [nameof(Alternativas)]);
        }
        if (Gabarito is null || !letras.Contains(char.ToUpperInvariant(Gabarito.Value)))
            yield return new ValidationResult("Gabarito inválido para o tipo da questão.", [nameof(Gabarito)]);
        if (Origem == OrigemQuestao.Banca && string.IsNullOrWhiteSpace(Banca))
            yield return new ValidationResult("Informe a banca para questões de concurso.", [nameof(Banca)]);
        if (Assuntos.Count == 0)
            yield return new ValidationResult("Informe ao menos um assunto.", [nameof(Assuntos)]);
        if (Assuntos.Count > 10)
            yield return new ValidationResult("Máximo de 10 assuntos.", [nameof(Assuntos)]);
    }
}

public class QuestaoFiltro
{
    public string? Busca { get; set; }
    public string? Assunto { get; set; }
    public string? Banca { get; set; }
    public int? Ano { get; set; }
    public string? Concurso { get; set; }
    public string? Cargo { get; set; }
    public TipoQuestao? Tipo { get; set; }
    public OrigemQuestao? Origem { get; set; }
    public Dificuldade? Dificuldade { get; set; }
    public bool SomenteMinhas { get; set; }
    public bool SomenteFavoritas { get; set; }
    public bool NaoRespondidas { get; set; }
    public string Ordem { get; set; } = "recentes";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;

    public string ToQueryString()
    {
        var sb = new StringBuilder();
        void Add(string k, object? v)
        {
            if (v is null || (v is string s && string.IsNullOrWhiteSpace(s)) || (v is bool b && !b)) return;
            sb.Append(sb.Length == 0 ? '?' : '&').Append(k).Append('=').Append(Uri.EscapeDataString(v.ToString()!));
        }
        Add("busca", Busca); Add("assunto", Assunto); Add("banca", Banca); Add("ano", Ano);
        Add("concurso", Concurso); Add("cargo", Cargo); Add("tipo", Tipo); Add("origem", Origem);
        Add("dificuldade", Dificuldade); Add("somenteMinhas", SomenteMinhas); Add("somenteFavoritas", SomenteFavoritas);
        Add("naoRespondidas", NaoRespondidas); Add("ordem", Ordem); Add("page", Page); Add("pageSize", PageSize);
        return sb.ToString();
    }
}

public class ResponderQuestaoDto
{
    [Required] public char Letra { get; set; }
}

public class RespostaResultadoDto
{
    public bool Correta { get; set; }
    public char Gabarito { get; set; }
    public string? Explicacao { get; set; }
    public int TotalRespostas { get; set; }
    public int TotalAcertos { get; set; }
    public RecompensaDto Recompensa { get; set; } = new();
}

public class AssuntoDto
{
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int TotalQuestoes { get; set; }
}

public class FiltrosDisponiveisDto
{
    public List<string> Bancas { get; set; } = [];
    public List<AssuntoDto> Assuntos { get; set; } = [];
    public List<int> Anos { get; set; } = [];
}
