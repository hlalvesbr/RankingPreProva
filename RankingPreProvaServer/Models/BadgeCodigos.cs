namespace RankingPreProvaServer.Models;

public static class BadgeCodigos
{
    public const string PrimeiroPasso = "PRIMEIRO_PASSO";
    public const string Centuriao = "CENTURIAO";
    public const string Maratonista = "MARATONISTA";
    public const string Constancia = "CONSTANCIA";
    public const string Disciplina = "DISCIPLINA";
    public const string Criador = "CRIADOR";
    public const string Arquiteto = "ARQUITETO";
    public const string Estreante = "ESTREANTE";
    public const string Podio = "PODIO";
    public const string Gabaritou = "GABARITOU";
    public const string Especialista = "ESPECIALISTA";
    public const string Revisor = "REVISOR";
    public const string Popular = "POPULAR";

    public static readonly Badge[] Seed =
    [
        new() { Id = 1, Ordem = 1, Codigo = PrimeiroPasso, Nome = "Primeiro Passo", Descricao = "Responda sua primeira questão.", Icone = "👣", Meta = 1 },
        new() { Id = 2, Ordem = 2, Codigo = Centuriao, Nome = "Centurião", Descricao = "Responda 100 questões.", Icone = "💯", Meta = 100 },
        new() { Id = 3, Ordem = 3, Codigo = Maratonista, Nome = "Maratonista", Descricao = "Responda 1.000 questões.", Icone = "🏃", Meta = 1000 },
        new() { Id = 4, Ordem = 4, Codigo = Constancia, Nome = "Constância", Descricao = "Estude 7 dias seguidos.", Icone = "🔥", Meta = 7 },
        new() { Id = 5, Ordem = 5, Codigo = Disciplina, Nome = "Disciplina de Ferro", Descricao = "Estude 30 dias seguidos.", Icone = "🛡️", Meta = 30 },
        new() { Id = 6, Ordem = 6, Codigo = Criador, Nome = "Criador", Descricao = "Cadastre 10 questões.", Icone = "✍️", Meta = 10 },
        new() { Id = 7, Ordem = 7, Codigo = Arquiteto, Nome = "Arquiteto de Provas", Descricao = "Crie 3 provas.", Icone = "🏛️", Meta = 3 },
        new() { Id = 8, Ordem = 8, Codigo = Estreante, Nome = "Estreante", Descricao = "Conclua sua primeira prova.", Icone = "🎬", Meta = 1 },
        new() { Id = 9, Ordem = 9, Codigo = Podio, Nome = "Pódio", Descricao = "Fique entre os 3 primeiros no ranking de uma prova.", Icone = "🏆", Meta = 1 },
        new() { Id = 10, Ordem = 10, Codigo = Gabaritou, Nome = "Gabaritou!", Descricao = "Acerte 100% de uma prova.", Icone = "🎯", Meta = 1 },
        new() { Id = 11, Ordem = 11, Codigo = Especialista, Nome = "Especialista", Descricao = "Acerte 50 questões do mesmo assunto.", Icone = "🧠", Meta = 50 },
        new() { Id = 12, Ordem = 12, Codigo = Revisor, Nome = "Revisor", Descricao = "Tenha uma denúncia aceita pela moderação.", Icone = "🔎", Meta = 1 },
        new() { Id = 13, Ordem = 13, Codigo = Popular, Nome = "Popular", Descricao = "Tenha uma questão ou prova com saldo de 10 votos.", Icone = "⭐", Meta = 10 },
    ];
}
