using Microsoft.EntityFrameworkCore;
using RankingPreProva.Models;

namespace RankingPreProva.Data
{
    public class RankingPreProvaDbContext : DbContext
    {
        public RankingPreProvaDbContext(DbContextOptions<RankingPreProvaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Prova> Provas => Set<Prova>();

        public DbSet<Usuario> Usuarios => Set<Usuario>();

        public DbSet<Questao> Questoes => Set<Questao>();

        public DbSet<OpcaoResposta> OpcoesResposta => Set<OpcaoResposta>();

        public DbSet<ProvaQuestao> ProvaQuestoes => Set<ProvaQuestao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Apelido).IsUnique();
            });

            modelBuilder.Entity<Questao>(entity =>
            {
                entity.HasMany(q => q.Opcoes)
                    .WithOne(o => o.Questao)
                    .HasForeignKey(o => o.QuestaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProvaQuestao>(entity =>
            {
                entity.HasKey(pq => new { pq.ProvaId, pq.QuestaoId });

                entity.HasOne(pq => pq.Prova)
                    .WithMany(p => p.ProvaQuestoes)
                    .HasForeignKey(pq => pq.ProvaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pq => pq.Questao)
                    .WithMany()
                    .HasForeignKey(pq => pq.QuestaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
