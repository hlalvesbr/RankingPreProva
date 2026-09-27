using System.ComponentModel.DataAnnotations;

namespace RankingPreProva.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Apelido { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(400)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    }
}
