using Microsoft.EntityFrameworkCore;
using RankingPreProvaServer.Models;

namespace RankingPreProvaServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Assunto> Assuntos => Set<Assunto>();
        public DbSet<Questao> Questoes => Set<Questao>();
        public DbSet<Alternativa> Alternativas => Set<Alternativa>();
        public DbSet<Prova> Provas => Set<Prova>();
        public DbSet<ProvaQuestao> ProvaQuestoes => Set<ProvaQuestao>();
        public DbSet<Tentativa> Tentativas => Set<Tentativa>();
        public DbSet<RespostaTentativa> RespostasTentativa => Set<RespostaTentativa>();
        public DbSet<RespostaAvulsa> RespostasAvulsas => Set<RespostaAvulsa>();
        public DbSet<Voto> Votos => Set<Voto>();
        public DbSet<Denuncia> Denuncias => Set<Denuncia>();
        public DbSet<Favorito> Favoritos => Set<Favorito>();
        public DbSet<Badge> Badges => Set<Badge>();
        public DbSet<UsuarioBadge> UsuarioBadges => Set<UsuarioBadge>();
        public DbSet<XpEvento> XpEventos => Set<XpEvento>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasIndex(u => u.GoogleSubject).IsUnique();
                e.HasIndex(u => u.Apelido).IsUnique();
                e.HasIndex(u => u.Xp);
                e.Property(u => u.GoogleSubject).HasMaxLength(200).IsRequired();
                e.Property(u => u.Email).HasMaxLength(320).IsRequired();
                e.Property(u => u.NomeExibicao).HasMaxLength(120).IsRequired();
                e.Property(u => u.Apelido).HasMaxLength(40);
                e.Property(u => u.AvatarUrl).HasMaxLength(1000);
                e.Property(u => u.Bio).HasMaxLength(500);
                e.Property(u => u.CargoAlvo).HasMaxLength(200);
                e.Property(u => u.ConcursoAlvo).HasMaxLength(200);
            });

            modelBuilder.Entity<Assunto>(e =>
            {
                e.HasIndex(a => a.Slug).IsUnique();
                e.Property(a => a.Nome).HasMaxLength(100).IsRequired();
                e.Property(a => a.Slug).HasMaxLength(120).IsRequired();
            });

            modelBuilder.Entity<Questao>(e =>
            {
                e.Property(q => q.Enunciado).HasMaxLength(8000).IsRequired();
                e.Property(q => q.Explicacao).HasMaxLength(8000);
                e.Property(q => q.Banca).HasMaxLength(120);
                e.Property(q => q.Concurso).HasMaxLength(200);
                e.Property(q => q.Cargo).HasMaxLength(200);
                e.Property(q => q.Orgao).HasMaxLength(200);
                e.HasIndex(q => q.Banca);
                e.HasIndex(q => q.Ano);
                e.HasIndex(q => new { q.Status, q.SaldoVotos });
                e.HasOne(q => q.Autor).WithMany().HasForeignKey(q => q.AutorId).OnDelete(DeleteBehavior.Restrict);
                e.HasMany(q => q.Alternativas).WithOne().HasForeignKey(a => a.QuestaoId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(q => q.Assuntos).WithMany(a => a.Questoes).UsingEntity("QuestaoAssunto");
            });

            modelBuilder.Entity<Alternativa>(e =>
            {
                e.Property(a => a.Texto).HasMaxLength(2000).IsRequired();
                e.HasIndex(a => new { a.QuestaoId, a.Letra }).IsUnique();
            });

            modelBuilder.Entity<Prova>(e =>
            {
                e.Property(p => p.Titulo).HasMaxLength(200).IsRequired();
                e.Property(p => p.Descricao).HasMaxLength(4000);
                e.Property(p => p.ConcursoSimulado).HasMaxLength(200);
                e.Property(p => p.Banca).HasMaxLength(120);
                e.Property(p => p.Cargo).HasMaxLength(200);
                e.Property(p => p.Orgao).HasMaxLength(200);
                e.Property(p => p.PontosAcerto).HasPrecision(8, 2);
                e.Property(p => p.PenalidadeErro).HasPrecision(8, 2);
                e.HasIndex(p => new { p.Status, p.SaldoVotos });
                e.HasOne(p => p.Autor).WithMany().HasForeignKey(p => p.AutorId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProvaQuestao>(e =>
            {
                e.HasKey(pq => new { pq.ProvaId, pq.QuestaoId });
                e.Property(pq => pq.Peso).HasPrecision(8, 2);
                e.HasOne(pq => pq.Prova).WithMany(p => p.Questoes).HasForeignKey(pq => pq.ProvaId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(pq => pq.Questao).WithMany().HasForeignKey(pq => pq.QuestaoId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Tentativa>(e =>
            {
                e.Property(t => t.Nota).HasPrecision(10, 2);
                e.Property(t => t.NotaMaxima).HasPrecision(10, 2);
                e.HasIndex(t => new { t.ProvaId, t.UsuarioId })
                    .IsUnique()
                    .HasFilter("\"EhPrimeira\" = true")
                    .HasDatabaseName("IX_Tentativas_PrimeiraPorUsuario");
                e.HasIndex(t => new { t.ProvaId, t.EhPrimeira, t.Nota });
                e.HasOne(t => t.Prova).WithMany(p => p.Tentativas).HasForeignKey(t => t.ProvaId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.Usuario).WithMany().HasForeignKey(t => t.UsuarioId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(t => t.Respostas).WithOne().HasForeignKey(r => r.TentativaId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RespostaTentativa>(e =>
            {
                e.HasOne(r => r.Questao).WithMany().HasForeignKey(r => r.QuestaoId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RespostaAvulsa>(e =>
            {
                e.HasIndex(r => new { r.UsuarioId, r.Data });
                e.HasIndex(r => new { r.UsuarioId, r.QuestaoId });
                e.HasOne(r => r.Questao).WithMany().HasForeignKey(r => r.QuestaoId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<Usuario>().WithMany().HasForeignKey(r => r.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Voto>(e =>
            {
                e.HasIndex(v => new { v.UsuarioId, v.AlvoTipo, v.AlvoId }).IsUnique();
                e.HasIndex(v => new { v.AlvoTipo, v.AlvoId });
                e.HasOne<Usuario>().WithMany().HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Denuncia>(e =>
            {
                e.Property(d => d.Detalhe).HasMaxLength(1000);
                e.HasIndex(d => new { d.AutorId, d.AlvoTipo, d.AlvoId }).IsUnique();
                e.HasIndex(d => new { d.Status, d.AlvoTipo, d.AlvoId });
                e.HasOne(d => d.Autor).WithMany().HasForeignKey(d => d.AutorId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Favorito>(e =>
            {
                e.HasIndex(f => new { f.UsuarioId, f.AlvoTipo, f.AlvoId }).IsUnique();
                e.HasOne<Usuario>().WithMany().HasForeignKey(f => f.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Badge>(e =>
            {
                e.HasIndex(b => b.Codigo).IsUnique();
                e.Property(b => b.Codigo).HasMaxLength(40).IsRequired();
                e.Property(b => b.Nome).HasMaxLength(80).IsRequired();
                e.Property(b => b.Descricao).HasMaxLength(300).IsRequired();
                e.Property(b => b.Icone).HasMaxLength(16).IsRequired();
                e.HasData(BadgeCodigos.Seed);
            });

            modelBuilder.Entity<UsuarioBadge>(e =>
            {
                e.HasKey(ub => new { ub.UsuarioId, ub.BadgeId });
                e.HasOne<Usuario>().WithMany(u => u.Badges).HasForeignKey(ub => ub.UsuarioId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ub => ub.Badge).WithMany().HasForeignKey(ub => ub.BadgeId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<XpEvento>(e =>
            {
                e.Property(x => x.Motivo).HasMaxLength(100).IsRequired();
                e.HasIndex(x => new { x.Data, x.UsuarioId });
                e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
