using System.ComponentModel.DataAnnotations;

namespace RankingPreProva.Shared;

public enum TipoQuestao
{
    [Display(Name = "Múltipla escolha")] MultiplaEscolha = 0,
    [Display(Name = "Certo ou errado")] CertoErrado = 1
}

public enum OrigemQuestao
{
    [Display(Name = "Banca")] Banca = 0,
    [Display(Name = "Autoral")] Usuario = 1,
    [Display(Name = "Gerada por IA")] IA = 2
}

public enum Dificuldade
{
    [Display(Name = "Fácil")] Facil = 0,
    [Display(Name = "Média")] Media = 1,
    [Display(Name = "Difícil")] Dificil = 2
}

public enum RegraPontuacao
{
    [Display(Name = "Simples (só acertos)")] Simples = 0,
    [Display(Name = "Estilo Cebraspe (erro anula)")] CespeAnulacao = 1,
    [Display(Name = "Peso por questão")] PesoPersonalizado = 2
}

public enum AlvoTipo
{
    Questao = 0,
    Prova = 1
}

public enum MotivoDenuncia
{
    [Display(Name = "Gabarito errado")] GabaritoErrado = 0,
    [Display(Name = "Questão duplicada")] Duplicada = 1,
    [Display(Name = "Conteúdo ofensivo")] Ofensiva = 2,
    [Display(Name = "Desatualizada")] Desatualizada = 3,
    [Display(Name = "Outro")] Outro = 4
}

public enum StatusDenuncia
{
    Aberta = 0,
    Aceita = 1,
    Rejeitada = 2
}

public enum StatusItem
{
    [Display(Name = "Ativo")] Ativo = 0,
    [Display(Name = "Em revisão")] EmRevisao = 1,
    [Display(Name = "Arquivado")] Arquivado = 2
}
