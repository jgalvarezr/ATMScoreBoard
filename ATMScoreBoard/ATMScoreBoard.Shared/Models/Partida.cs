using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATMScoreBoard.Shared.Models
{
    public class Partida
    {
        [Key]
        public int Id { get; set; }
        public int EquipoAId { get; set; }
        public int EquipoBId { get; set; }
        public int? EquipoGanadorId { get; set; }
        public int TipoJuegoId { get; set; }
        public DateTime Fecha { get; set; }
        public bool FueVictoriaImpecable { get; set; }

        [ForeignKey("EquipoAId")]
        public virtual Equipo EquipoA { get; set; } = null!;

        [ForeignKey("EquipoBId")]
        public virtual Equipo EquipoB { get; set; } = null!;

        [ForeignKey("TipoJuegoId")]
        public virtual TipoJuego TipoJuego { get; set; } = TipoJuego.Chapolin;
    }
}