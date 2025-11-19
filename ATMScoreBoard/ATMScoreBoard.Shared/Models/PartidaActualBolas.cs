using System.ComponentModel.DataAnnotations;

namespace ATMScoreBoard.Shared.Models
{
    public class PartidaActualBolas
    {
        [Key]
        public int Id { get; set; }
        public int MesaId { get; set; } // A qué partida en curso pertenece
        public int? EquipoId { get; set; } // Quién la metió
        public int NumeroBola { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}