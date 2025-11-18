namespace ATMScoreBoard.Shared.Models
{
    public class EquipoJugador
    {
        public int EquipoId { get; set; }
        public int JugadorId { get; set; }
        public virtual Equipo? Equipo { get; set; } 
        public virtual Jugador? Jugador { get; set; }
    }
}