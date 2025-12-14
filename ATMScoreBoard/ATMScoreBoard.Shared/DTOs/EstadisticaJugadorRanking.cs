using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMScoreBoard.Shared.DTOs
{
    public class EstadisticaJugadorRanking
    {
        public int Id { get; set; } // Corresponde al Id del Jugador
        public string Nombre { get; set; } = string.Empty;
        public int PuntosRanking { get; set; }
        public int PosicionRanking { get; set; }
        public int TotalPartidas { get; set; }
        public double Average { get; set; }
    }
}
