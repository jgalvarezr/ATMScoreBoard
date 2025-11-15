using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATMScoreBoard.Shared.Models
{
    public class PartidaActual
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Le decimos a EF que nosotros le daremos el ID
        public int MesaId { get; set; } // Clave Primaria y Foránea a la vez

        public int TipoJuegoId { get; set; }
        public int EquipoAId { get; set; }
        public int EquipoBId { get; set; }
                

        // Reglas Específicas de Juego (pueden ser nulas)
        [MaxLength(20)]
        public string? BandaEquipoA { get; set; } // Para Chapolín: "Blanca" o "Roja"
        public int? EquipoLisasId { get; set; } // Para Bola 8

        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    }
}