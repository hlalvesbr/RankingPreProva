using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RankingPreProva.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assuntos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Icone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Meta = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoogleSubject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NomeExibicao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Apelido = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CargoAlvo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConcursoAlvo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    SequenciaDias = table.Column<int>(type: "integer", nullable: false),
                    MaiorSequencia = table.Column<int>(type: "integer", nullable: false),
                    UltimaAtividade = table.Column<DateOnly>(type: "date", nullable: true),
                    MetaDiaria = table.Column<int>(type: "integer", nullable: false),
                    EhAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoAcesso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Denuncias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    AlvoTipo = table.Column<int>(type: "integer", nullable: false),
                    AlvoId = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<int>(type: "integer", nullable: false),
                    Detalhe = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Denuncias_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    AlvoTipo = table.Column<int>(type: "integer", nullable: false),
                    AlvoId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favoritos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Provas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ConcursoSimulado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Banca = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Cargo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Orgao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Ano = table.Column<int>(type: "integer", nullable: true),
                    TempoLimiteMin = table.Column<int>(type: "integer", nullable: true),
                    Publica = table.Column<bool>(type: "boolean", nullable: false),
                    Regra = table.Column<int>(type: "integer", nullable: false),
                    PontosAcerto = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    PenalidadeErro = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    NotaMinimaZero = table.Column<bool>(type: "boolean", nullable: false),
                    SaldoVotos = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Provas_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    Enunciado = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Gabarito = table.Column<char>(type: "character(1)", nullable: false),
                    Explicacao = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Ano = table.Column<int>(type: "integer", nullable: true),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    Banca = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Concurso = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Cargo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Orgao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Dificuldade = table.Column<int>(type: "integer", nullable: false),
                    SaldoVotos = table.Column<int>(type: "integer", nullable: false),
                    TotalRespostas = table.Column<int>(type: "integer", nullable: false),
                    TotalAcertos = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questoes_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioBadges",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    BadgeId = table.Column<int>(type: "integer", nullable: false),
                    ConquistadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioBadges", x => new { x.UsuarioId, x.BadgeId });
                    table.ForeignKey(
                        name: "FK_UsuarioBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioBadges_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    AlvoTipo = table.Column<int>(type: "integer", nullable: false),
                    AlvoId = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XpEventos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpEventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XpEventos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tentativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProvaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Iniciada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Finalizada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DuracaoSeg = table.Column<int>(type: "integer", nullable: false),
                    Nota = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    NotaMaxima = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Acertos = table.Column<int>(type: "integer", nullable: false),
                    Erros = table.Column<int>(type: "integer", nullable: false),
                    EmBranco = table.Column<int>(type: "integer", nullable: false),
                    EhPrimeira = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tentativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tentativas_Provas_ProvaId",
                        column: x => x.ProvaId,
                        principalTable: "Provas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tentativas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alternativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestaoId = table.Column<int>(type: "integer", nullable: false),
                    Letra = table.Column<char>(type: "character(1)", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alternativas_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvaQuestoes",
                columns: table => new
                {
                    ProvaId = table.Column<int>(type: "integer", nullable: false),
                    QuestaoId = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Peso = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvaQuestoes", x => new { x.ProvaId, x.QuestaoId });
                    table.ForeignKey(
                        name: "FK_ProvaQuestoes_Provas_ProvaId",
                        column: x => x.ProvaId,
                        principalTable: "Provas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvaQuestoes_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestaoAssunto",
                columns: table => new
                {
                    AssuntosId = table.Column<int>(type: "integer", nullable: false),
                    QuestoesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestaoAssunto", x => new { x.AssuntosId, x.QuestoesId });
                    table.ForeignKey(
                        name: "FK_QuestaoAssunto_Assuntos_AssuntosId",
                        column: x => x.AssuntosId,
                        principalTable: "Assuntos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestaoAssunto_Questoes_QuestoesId",
                        column: x => x.QuestoesId,
                        principalTable: "Questoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RespostasAvulsas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    QuestaoId = table.Column<int>(type: "integer", nullable: false),
                    Letra = table.Column<char>(type: "character(1)", nullable: false),
                    Correta = table.Column<bool>(type: "boolean", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasAvulsas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespostasAvulsas_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespostasAvulsas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RespostasTentativa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TentativaId = table.Column<int>(type: "integer", nullable: false),
                    QuestaoId = table.Column<int>(type: "integer", nullable: false),
                    Letra = table.Column<char>(type: "character(1)", nullable: true),
                    Correta = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasTentativa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespostasTentativa_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespostasTentativa_Tentativas_TentativaId",
                        column: x => x.TentativaId,
                        principalTable: "Tentativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "Codigo", "Descricao", "Icone", "Meta", "Nome", "Ordem" },
                values: new object[,]
                {
                    { 1, "PRIMEIRO_PASSO", "Responda sua primeira questão.", "👣", 1, "Primeiro Passo", 1 },
                    { 2, "CENTURIAO", "Responda 100 questões.", "💯", 100, "Centurião", 2 },
                    { 3, "MARATONISTA", "Responda 1.000 questões.", "🏃", 1000, "Maratonista", 3 },
                    { 4, "CONSTANCIA", "Estude 7 dias seguidos.", "🔥", 7, "Constância", 4 },
                    { 5, "DISCIPLINA", "Estude 30 dias seguidos.", "🛡️", 30, "Disciplina de Ferro", 5 },
                    { 6, "CRIADOR", "Cadastre 10 questões.", "✍️", 10, "Criador", 6 },
                    { 7, "ARQUITETO", "Crie 3 provas.", "🏛️", 3, "Arquiteto de Provas", 7 },
                    { 8, "ESTREANTE", "Conclua sua primeira prova.", "🎬", 1, "Estreante", 8 },
                    { 9, "PODIO", "Fique entre os 3 primeiros no ranking de uma prova.", "🏆", 1, "Pódio", 9 },
                    { 10, "GABARITOU", "Acerte 100% de uma prova.", "🎯", 1, "Gabaritou!", 10 },
                    { 11, "ESPECIALISTA", "Acerte 50 questões do mesmo assunto.", "🧠", 50, "Especialista", 11 },
                    { 12, "REVISOR", "Tenha uma denúncia aceita pela moderação.", "🔎", 1, "Revisor", 12 },
                    { 13, "POPULAR", "Tenha uma questão ou prova com saldo de 10 votos.", "⭐", 10, "Popular", 13 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alternativas_QuestaoId_Letra",
                table: "Alternativas",
                columns: new[] { "QuestaoId", "Letra" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assuntos_Slug",
                table: "Assuntos",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Badges_Codigo",
                table: "Badges",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_AutorId_AlvoTipo_AlvoId",
                table: "Denuncias",
                columns: new[] { "AutorId", "AlvoTipo", "AlvoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_Status_AlvoTipo_AlvoId",
                table: "Denuncias",
                columns: new[] { "Status", "AlvoTipo", "AlvoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_UsuarioId_AlvoTipo_AlvoId",
                table: "Favoritos",
                columns: new[] { "UsuarioId", "AlvoTipo", "AlvoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvaQuestoes_QuestaoId",
                table: "ProvaQuestoes",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Provas_AutorId",
                table: "Provas",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Provas_Status_SaldoVotos",
                table: "Provas",
                columns: new[] { "Status", "SaldoVotos" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestaoAssunto_QuestoesId",
                table: "QuestaoAssunto",
                column: "QuestoesId");

            migrationBuilder.CreateIndex(
                name: "IX_Questoes_Ano",
                table: "Questoes",
                column: "Ano");

            migrationBuilder.CreateIndex(
                name: "IX_Questoes_AutorId",
                table: "Questoes",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Questoes_Banca",
                table: "Questoes",
                column: "Banca");

            migrationBuilder.CreateIndex(
                name: "IX_Questoes_Status_SaldoVotos",
                table: "Questoes",
                columns: new[] { "Status", "SaldoVotos" });

            migrationBuilder.CreateIndex(
                name: "IX_RespostasAvulsas_QuestaoId",
                table: "RespostasAvulsas",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasAvulsas_UsuarioId_Data",
                table: "RespostasAvulsas",
                columns: new[] { "UsuarioId", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_RespostasAvulsas_UsuarioId_QuestaoId",
                table: "RespostasAvulsas",
                columns: new[] { "UsuarioId", "QuestaoId" });

            migrationBuilder.CreateIndex(
                name: "IX_RespostasTentativa_QuestaoId",
                table: "RespostasTentativa",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasTentativa_TentativaId",
                table: "RespostasTentativa",
                column: "TentativaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tentativas_PrimeiraPorUsuario",
                table: "Tentativas",
                columns: new[] { "ProvaId", "UsuarioId" },
                unique: true,
                filter: "\"EhPrimeira\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Tentativas_ProvaId_EhPrimeira_Nota",
                table: "Tentativas",
                columns: new[] { "ProvaId", "EhPrimeira", "Nota" });

            migrationBuilder.CreateIndex(
                name: "IX_Tentativas_UsuarioId",
                table: "Tentativas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioBadges_BadgeId",
                table: "UsuarioBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Apelido",
                table: "Usuarios",
                column: "Apelido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_GoogleSubject",
                table: "Usuarios",
                column: "GoogleSubject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Xp",
                table: "Usuarios",
                column: "Xp");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_AlvoTipo_AlvoId",
                table: "Votos",
                columns: new[] { "AlvoTipo", "AlvoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Votos_UsuarioId_AlvoTipo_AlvoId",
                table: "Votos",
                columns: new[] { "UsuarioId", "AlvoTipo", "AlvoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XpEventos_Data_UsuarioId",
                table: "XpEventos",
                columns: new[] { "Data", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_XpEventos_UsuarioId",
                table: "XpEventos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alternativas");

            migrationBuilder.DropTable(
                name: "Denuncias");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "ProvaQuestoes");

            migrationBuilder.DropTable(
                name: "QuestaoAssunto");

            migrationBuilder.DropTable(
                name: "RespostasAvulsas");

            migrationBuilder.DropTable(
                name: "RespostasTentativa");

            migrationBuilder.DropTable(
                name: "UsuarioBadges");

            migrationBuilder.DropTable(
                name: "Votos");

            migrationBuilder.DropTable(
                name: "XpEventos");

            migrationBuilder.DropTable(
                name: "Assuntos");

            migrationBuilder.DropTable(
                name: "Questoes");

            migrationBuilder.DropTable(
                name: "Tentativas");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "Provas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
