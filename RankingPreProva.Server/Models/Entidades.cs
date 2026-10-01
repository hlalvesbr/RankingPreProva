using RankingPreProva.Shared;

namespace RankingPreProva.Server.Models;

public class Usuario
{
    public int Id { get; set; }
    public string GoogleSubject { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NomeExibicao { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? CargoAlvo { get; set; }
    public string? ConcursoAlvo { get; set; }
    public int Xp { get; set; }
    public int Nivel { get; set; } = 1;
    public int SequenciaDias { get; set; }
    public int MaiorSequencia { get; set; }
    public DateOnly? UltimaAtividade { get; set; }
    public int MetaDiaria { get; set; } = 20;
    public bool EhAdmin { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime UltimoAcesso { get; set; } = DateTime.UtcNow;

    public List<UsuarioBadge> Badges { get; set; } = [];
}

public class Assunto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<Questao> Questoes { get; set; } = [];
}

public class Questao
{
    public int Id { get; set; }
    public int AutorId { get; set; }
    public Usuario Autor { get; set; } = null!;
    public string Enunciado { get; set; } = string.Empty;
    public TipoQuestao Tipo { get; set; }
    public char Gabarito { get; set; }
    public string? Explicacao { get; set; }
    public int? Ano { get; set; }
    public OrigemQuestao Origem { get; set; }
    public string? Banca { get; set; }
    public string? Concurso { get; set; }
    public string? Cargo { get; set; }
    public string? Orgao { get; set; }
    public Dificuldade Dificuldade { get; set; }
    public int SaldoVotos { get; set; }
    public int TotalRespostas { get; set; }
    public int TotalAcertos { get; set; }
    public StatusItem Status { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public List<Alternativa> Alternativas { get; set; } = [];
    public List<Assunto> Assuntos { get; set; } = [];
}

public class Alternativa
{
    public int Id { get; set; }
    public int QuestaoId { get; set; }
    public char Letra { get; set; }
    public string Texto { get; set; } = string.Empty;
}

public class Prova
{
    public int Id { get; set; }
    public int AutorId { get; set; }
    public Usuario Autor { get; set; } = null!;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ConcursoSimulado { get; set; }
    public string? Banca { get; set; }
    public string? Cargo { get; set; }
    public string? Orgao { get; set; }
    public int? Ano { get; set; }
    public int? TempoLimiteMin { get; set; }
    public bool Publica { get; set; } = true;
    public RegraPontuacao Regra { get; set; }
    public decimal PontosAcerto { get; set; } = 1;
    public decimal PenalidadeErro { get; set; } = 1;
    public bool NotaMinimaZero { get; set; } = true;
    public int SaldoVotos { get; set; }
    public StatusItem Status { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public List<ProvaQuestao> Questoes { get; set; } = [];
    public List<Tentativa> Tentativas { get; set; } = [];
}

public class ProvaQuestao
{
    public int ProvaId { get; set; }
    public Prova Prova { get; set; } = null!;
    public int QuestaoId { get; set; }
    public Questao Questao { get; set; } = null!;
    public int Ordem { get; set; }
    public decimal Peso { get; set; } = 1;
}

public class Tentativa
{
    public int Id { get; set; }
    public int ProvaId { get; set; }
    public Prova Prova { get; set; } = null!;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public DateTime Iniciada { get; set; } = DateTime.UtcNow;
    public DateTime? Finalizada { get; set; }
    public int DuracaoSeg { get; set; }
    public decimal Nota { get; set; }
    public decimal NotaMaxima { get; set; }
    public int Acertos { get; set; }
    public int Erros { get; set; }
    public int EmBranco { get; set; }
    public bool EhPrimeira { get; set; }

    public List<RespostaTentativa> Respostas { get; set; } = [];
}

public class RespostaTentativa
{
    public int Id { get; set; }
    public int TentativaId { get; set; }
    public int QuestaoId { get; set; }
    public Questao Questao { get; set; } = null!;
    public char? Letra { get; set; }
    public bool Correta { get; set; }
}

public class RespostaAvulsa
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int QuestaoId { get; set; }
    public Questao Questao { get; set; } = null!;
    public char Letra { get; set; }
    public bool Correta { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
}

public class Voto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public AlvoTipo AlvoTipo { get; set; }
    public int AlvoId { get; set; }
    public int Valor { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
}

public class Denuncia
{
    public int Id { get; set; }
    public int AutorId { get; set; }
    public Usuario Autor { get; set; } = null!;
    public AlvoTipo AlvoTipo { get; set; }
    public int AlvoId { get; set; }
    public MotivoDenuncia Motivo { get; set; }
    public string? Detalhe { get; set; }
    public StatusDenuncia Status { get; set; }
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvidaEm { get; set; }
}

public class Favorito
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public AlvoTipo AlvoTipo { get; set; }
    public int AlvoId { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
}

public class Badge
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public int Meta { get; set; } = 1;
    public int Ordem { get; set; }
}

public class UsuarioBadge
{
    public int UsuarioId { get; set; }
    public int BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;
    public DateTime ConquistadaEm { get; set; } = DateTime.UtcNow;
}

public class XpEvento
{
    public long Id { get; set; }
    public int UsuarioId { get; set; }
    public int Quantidade { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime Data { get; set; } = DateTime.UtcNow;
}

public class Imagem
{
    public Guid Id { get; set; }
    public int AutorId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int Tamanho { get; set; }
    public byte[] Bytes { get; set; } = [];
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
