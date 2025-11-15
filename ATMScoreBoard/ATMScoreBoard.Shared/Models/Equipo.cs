using System.ComponentModel.DataAnnotations;

namespace ATMScoreBoard.Shared.Models
{
    public class Equipo
    {
        [Key]
        public int Id { get; set; }
        public bool IsIndividual { get; set; }
        public virtual ICollection<EquipoJugador> EquipoJugadores { get; set; }
    }
}