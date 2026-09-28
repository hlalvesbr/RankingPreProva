using System.ComponentModel.DataAnnotations;

namespace RankingPreProva.Shared.Dtos;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? CargoAlvo { get; set; }
    public string? ConcursoAlvo { get; set; }
    public int Xp { get; set; }
    public int Nivel { get; set; }
    public int SequenciaDias { get; set; }
    public int MaiorSequencia { get; set; }
    public DateTime CriadoEm { get; set; }
    public bool EhAdmin { get; set; }
    public int TotalQuestoesCriadas { get; set; }
    public int TotalProvasCriadas { get; set; }
    public int TotalRespostas { get; set; }
    public int TotalAcertos { get; set; }
    public int MetaDiaria { get; set; }
}

public class PerfilEdicaoDto
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(40)]
    [RegularExpression("^[a-zA-Z0-9_.-]{3,40}$", ErrorMessage = "Use de 3 a 40 letras, números, '.', '_' ou '-'.")]
    public string? Apelido { get; set; }

    [MaxLength(500)] public string? Bio { get; set; }
    [MaxLength(200)] public string? CargoAlvo { get; set; }
    [MaxLength(200)] public string? ConcursoAlvo { get; set; }

    [Range(1, 500, ErrorMessage = "Meta entre 1 e 500 questões.")]
    public int MetaDiaria { get; set; } = 20;
}

public class PerfilPublicoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? CargoAlvo { get; set; }
    public string? ConcursoAlvo { get; set; }
    public int Xp { get; set; }
    public int Nivel { get; set; }
    public int MaiorSequencia { get; set; }
    public DateTime CriadoEm { get; set; }
    public int TotalQuestoesCriadas { get; set; }
    public int TotalProvasCriadas { get; set; }
    public int TotalRespostas { get; set; }
    public List<BadgeDto> Badges { get; set; } = [];
    public List<ProvaResumoDto> Provas { get; set; } = [];
}

public class BadgeDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public bool Conquistada { get; set; }
    public DateTime? ConquistadaEm { get; set; }
    public int Progresso { get; set; }
    public int Meta { get; set; }
}

public class DashboardDto
{
    public UsuarioDto Usuario { get; set; } = new();
    public int RespostasHoje { get; set; }
    public int RespostasSemana { get; set; }
    public int AcertosSemana { get; set; }
    public int? PosicaoRankingSemanal { get; set; }
    public int? DesafioDoDiaId { get; set; }
    public bool DesafioRespondido { get; set; }
    public List<ProvaResumoDto> ProvasEmAlta { get; set; } = [];
    public List<BadgeDto> BadgesRecentes { get; set; } = [];
    public List<TentativaResumoDto> UltimasTentativas { get; set; } = [];
    public List<AtividadeDiaDto> Semana { get; set; } = [];
}

public class AtividadeDiaDto
{
    public DateOnly Data { get; set; }
    public int Respostas { get; set; }
    public int Acertos { get; set; }
}

public class DesempenhoAssuntoDto
{
    public string Assunto { get; set; } = string.Empty;
    public int Respostas { get; set; }
    public int Acertos { get; set; }
    public double Percentual => Respostas == 0 ? 0 : 100.0 * Acertos / Respostas;
}

public class HistoricoDto
{
    public List<TentativaResumoDto> Tentativas { get; set; } = [];
    public List<DesempenhoAssuntoDto> Assuntos { get; set; } = [];
    public List<AtividadeDiaDto> Dias { get; set; } = [];
}

public class RankingGeralItemDto
{
    public int Posicao { get; set; }
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? AvatarUrl { get; set; }
    public int Nivel { get; set; }
    public int Xp { get; set; }
    public bool EhVoce { get; set; }
}
